import logging
import os
import re
import uuid
from contextlib import asynccontextmanager

import httpx
import uvicorn
from fastapi import FastAPI, HTTPException
from fastapi import Request as FastAPIRequest
from fastapi.middleware.cors import CORSMiddleware
from fastapi.middleware.gzip import GZipMiddleware

from config import RIDE_API_URL, _is_placeholder, validate_env
from graph import build_graph
from models import ChatRequest, ChatResponse
from rate_limiter import check_rate_limit, close_limiter
from session_store import close_store, load_session, ping_store, save_session
from state import ChatState

logger = logging.getLogger(__name__)

_MAX_MSG_LEN = 2000
_llm_available = False


def _sanitize(text: str) -> str:
    cleaned = re.sub(r"[\x00-\x08\x0b\x0c\x0e-\x1f\x7f]", "", text)
    return cleaned[:_MAX_MSG_LEN]


@asynccontextmanager
async def lifespan(app: FastAPI):
    validate_env()
    global _llm_available
    _llm_available = bool(os.getenv("GOOGLE_API_KEY") and not _is_placeholder(os.getenv("GOOGLE_API_KEY")))
    logger.info("LLM %s", "available" if _llm_available else "unavailable (rule-based only)")
    yield
    logger.info("Shutting down ... closing store and rate limiter")
    await close_store()
    await close_limiter()


app = FastAPI(title="RideSharing Chatbot", version="2.0.0", lifespan=lifespan)
app.add_middleware(GZipMiddleware, minimum_size=1000)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

graph = build_graph()


@app.post("/chat", response_model=ChatResponse)
async def chat(request: ChatRequest, fastapi_req: FastAPIRequest):
    conv_id = request.conversation_id or str(uuid.uuid4())
    req_id = str(uuid.uuid4())[:8]
    client_ip = fastapi_req.client.host if fastapi_req.client else "unknown"

    if not await check_rate_limit(client_ip):
        logger.warning("req=%s rate limit exceeded for ip=%s", req_id, client_ip)
        raise HTTPException(status_code=429, detail="Rate limit exceeded. Try again later.")

    request.message = _sanitize(request.message)
    logger.info("req=%s conv=%s msg=%.50s ip=%s", req_id, conv_id, request.message, client_ip)

    conv = await load_session(conv_id)
    if conv is None:
        conv = {"messages": [], "context": {}}

    conv["messages"].append(request.message)

    ctx = conv["context"]
    awaiting = ctx.get("_awaiting")
    force_intent = None

    if awaiting == "pickup":
        ctx["pickup"] = request.message
        ctx.pop("_awaiting", None)
        ctx["_awaiting"] = "dropoff"
        await save_session(conv_id, conv)
        return ChatResponse(
            reply=f"Pickup: {request.message}. Where is the dropoff location?",
            intent="book_ride",
            conversation_id=conv_id,
        )
    elif awaiting == "dropoff":
        ctx["dropoff"] = request.message
        ctx.pop("_awaiting", None)
        force_intent = "book_ride"

    if ctx.get("_ride_list") and re.match(r"^\d+$", request.message.strip()):
        force_intent = ctx.get("_last_intent")

    if force_intent:
        ctx["_force_intent"] = force_intent

    initial_state: ChatState = {
        "messages": conv["messages"],
        "intent": None,
        "response": None,
        "context": ctx,
    }

    result = await graph.ainvoke(initial_state)

    if result.get("response"):
        conv["messages"].append(result["response"])

    new_ctx = result.get("context", {})
    intent = new_ctx.get("intent") or result.get("intent") or "unknown"
    new_ctx["_last_intent"] = intent
    conv["context"] = new_ctx

    await save_session(conv_id, conv)

    return ChatResponse(
        reply=result.get("response", "Sorry, I couldn't process that."),
        intent=intent,
        conversation_id=conv_id,
    )


@app.get("/health")
async def health():
    downstream = "unknown"
    try:
        async with httpx.AsyncClient(timeout=5) as client:
            resp = await client.get(f"{RIDE_API_URL}/health")
            downstream = "up" if resp.status_code < 500 else "down"
    except Exception:
        downstream = "unreachable"

    store_ok = await ping_store()

    return {
        "status": "healthy",
        "service": "ridesharing-chatbot",
        "llm": "available" if _llm_available else "unavailable",
        "session_store": "ok" if store_ok else "error",
        "downstream_api": downstream,
    }


if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=8000)
