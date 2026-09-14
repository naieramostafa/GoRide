import re

from langchain_google_genai import ChatGoogleGenerativeAI
from langgraph.graph import END, StateGraph

from config import GEMINI_MODEL, GOOGLE_API_KEY
from handlers import (
    handle_admin,
    handle_book_ride,
    handle_cancel_ride,
    handle_driver_actions,
    handle_find_drivers,
    handle_login,
    handle_payment,
    handle_profile,
    handle_rating,
    handle_register,
    handle_ride_details,
    handle_ride_status,
    handle_support,
    handle_unknown,
)
from logger import logger
from state import ChatState
from state_wrapper import StateWrapper

_llm = None

def _get_llm():
    global _llm
    if _llm is None and GOOGLE_API_KEY:
        _llm = ChatGoogleGenerativeAI(
            model=GEMINI_MODEL,
            temperature=0,
            google_api_key=GOOGLE_API_KEY,
        )
    return _llm

VALID_INTENTS = {
    "book_ride", "ride_status", "ride_details", "cancel_ride",
    "register", "find_drivers", "driver_actions", "payment",
    "rating", "profile", "admin", "support", "login", "unknown"
}

_RULE_INTENTS = [
    (re.compile(r'\bregister\b|become\s+(?:a\s+)?driver|sign\s*up', re.IGNORECASE), "register"),
    (re.compile(r'email.*password|login|sign\s*in', re.IGNORECASE), "login"),
    (re.compile(r'(?:details?|info)\s+(?:of|for|about)\s+(?:ride|trip)|ride\s+details?', re.IGNORECASE), "ride_details"),
    (re.compile(r'\bcancel\b.*\b(?:ride|trip)\b|\b(?:ride|trip)\b.*\bcancel\b', re.IGNORECASE), "cancel_ride"),
    (re.compile(r'\bbook\b.*\b(?:ride|trip)\b|from\s+\S+\s+to\s+\S+', re.IGNORECASE), "book_ride"),
    (re.compile(r'\bfind\b.*\b(?:drivers?|nearby|available)\b|nearby\s+drivers', re.IGNORECASE), "find_drivers"),
    (re.compile(r'\b(?:online|offline|available|start\s+ride|complete\s+ride|finish\s+ride)\b|\bupdate\b.*\blocation\b|\blat\s*[=:]\s*[-\d.]', re.IGNORECASE), "driver_actions"),
    (re.compile(r'\bpay\b|\bpayment\b|\bcheckout\b', re.IGNORECASE), "payment"),
    (re.compile(r'\bra(?:te|ting)\b|\bstars?\b', re.IGNORECASE), "rating"),
    (re.compile(r'\b(?:profile|my\s*account|account\s*info)\b', re.IGNORECASE), "profile"),
    (re.compile(r'\badmin\b|verify\s+driver', re.IGNORECASE), "admin"),
    (re.compile(r'\b(?:help|support|what\s*can\s*you)\b', re.IGNORECASE), "support"),
    (re.compile(r'\b(?:status|my\s*rides?|my\s*trips?)\s*(?:status|history)?$', re.IGNORECASE), "ride_status"),
]


def _classify_by_rules(last_msg: str) -> str | None:
    for pattern, intent in _RULE_INTENTS:
        if pattern.search(last_msg):
            return intent
    return None


async def classify_intent(state: ChatState) -> ChatState:
    s = StateWrapper(state)

    forced = s.ctx.pop("_force_intent", None)
    if forced:
        intent = forced.lower()
        if intent not in VALID_INTENTS:
            intent = "unknown"
        state["intent"] = intent
        return state

    last_msg = s.last_message

    if s.ctx.get("_awaiting_driver_vehicle") or \
       (s.token and re.search(r'\b(my car|my vehicle|car is|vehicle is|make is)\b', last_msg, re.IGNORECASE)):
        state["intent"] = "register"
        return state

    if re.search(r'\bregister\b', last_msg, re.IGNORECASE) and \
       ('@' in last_msg or 'first' in last_msg.lower() or 'last' in last_msg.lower()):
        state["intent"] = "register"
        return state

    if re.search(r'email.*password|login|sign\s*in', last_msg, re.IGNORECASE) or \
       ('@' in last_msg and 'password' in last_msg.lower() and 'register' not in last_msg.lower() and 'first' not in last_msg.lower()):
        state["intent"] = "login"
        return state

    intent = _classify_by_rules(last_msg)
    if intent:
        state["intent"] = intent
        return state

    if not GOOGLE_API_KEY:
        state["intent"] = "unknown"
        return state

    llm = _get_llm()
    prompt = f"""Classify the user message into ONE of these intents. Reply with only the intent name.

intents:
- book_ride: User wants to book a ride, includes pickup/dropoff locations
- ride_status: User wants to see their ride history or current status
- ride_details: User wants details of a specific ride by ID
- cancel_ride: User wants to cancel a ride
- register: User wants to create a new account or become a driver
- find_drivers: User wants to find nearby drivers available for rides
- driver_actions: Driver wants to go online/offline, update location, start a ride, complete a ride, or show their rides
- payment: User wants to pay for or process a ride payment
- rating: User wants to rate a driver or a completed ride
- profile: User wants to view their account/profile information
- admin: User is asking about admin tasks, verifying drivers, or internal operations
- support: User has a general question, needs help, or asks "help"
- login: User is providing email and password credentials
- unknown: Cannot determine the intent

Message: {last_msg}

Intent:"""

    try:
        intent = await llm.ainvoke(prompt)
        raw = intent.content.strip().lower()
        state["intent"] = raw if raw in VALID_INTENTS else "unknown"
    except Exception:
        logger.exception("Intent classification failed, falling back to rules")
        intent = _classify_by_rules(last_msg) or "unknown"
        state["intent"] = intent
    return state


def _safe_route(state: ChatState) -> str:
    return state.get("intent") or "unknown"


def _safe_handler(handler):
    async def wrapped(state: ChatState) -> ChatState:
        try:
            return await handler(state)
        except Exception:
            logger.exception("Handler %s crashed", handler.__name__)
            s = StateWrapper(state)
            s.response = "Sorry, something went wrong processing your request. Please try again."
            return s.to_dict()
    wrapped.__name__ = handler.__name__
    return wrapped


HANDLERS = {
    "login": handle_login,
    "register": handle_register,
    "book_ride": handle_book_ride,
    "ride_status": handle_ride_status,
    "ride_details": handle_ride_details,
    "cancel_ride": handle_cancel_ride,
    "find_drivers": handle_find_drivers,
    "driver_actions": handle_driver_actions,
    "payment": handle_payment,
    "rating": handle_rating,
    "profile": handle_profile,
    "admin": handle_admin,
    "support": handle_support,
    "unknown": handle_unknown,
}

_SAFE_HANDLERS = {name: _safe_handler(h) for name, h in HANDLERS.items()}

def build_graph():
    workflow = StateGraph(ChatState)

    workflow.add_node("classify_intent", classify_intent)
    for name in _SAFE_HANDLERS:
        workflow.add_node(name, _SAFE_HANDLERS[name])

    workflow.set_entry_point("classify_intent")

    workflow.add_conditional_edges(
        "classify_intent",
        _safe_route,
        {name: name for name in _SAFE_HANDLERS},
    )

    for name in _SAFE_HANDLERS:
        workflow.add_edge(name, END)

    return workflow.compile()
