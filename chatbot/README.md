# RideSharing Chatbot

AI-powered conversational chatbot for a ride-sharing platform. Built with FastAPI, LangGraph, and Google Gemini.

## Quick Start

```bash
pip install -r requirements.txt
cp .env.example .env
# edit .env with your GOOGLE_API_KEY
uvicorn main:app --host 0.0.0.0 --port 8000
```

## Docker

```bash
docker-compose up --build
```

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `GOOGLE_API_KEY` | — | Google Gemini API key |
| `GEMINI_MODEL` | `gemini-2.0-flash` | LLM model name |
| `RIDE_API_URL` | `http://localhost:5000` | Backend ride API base URL |
| `REDIS_URL` | — | Redis URL (enables Redis session store + rate limiter) |
| `SESSION_STORE_PATH` | `sessions.json` | JSON file path (fallback when no Redis) |
| `API_TIMEOUT_SEC` | `30` | Backend API request timeout |
| `RATE_LIMIT` | `20` | Max requests per window |
| `RATE_WINDOW_SEC` | `60` | Rate limit window in seconds |
| `LOG_JSON` | `false` | Enable JSON-formatted logs |
| `LOG_LEVEL` | `INFO` | Log level |
| `APP_ENV` | `dev` | Environment name (loads `.env.{APP_ENV}`) |

## API

### POST /chat
Send a message and get a chatbot response.

```json
{
  "message": "I want to book a ride from Times Square to JFK",
  "conversation_id": "optional-uuid"
}
```

Response:
```json
{
  "reply": "Where are you starting from?",
  "intent": "book_ride",
  "conversation_id": "generated-uuid"
}
```

### GET /health
Returns service health including downstream API, LLM availability, and session store status.

## Architecture

```
main.py          — FastAPI app, routing, rate limiting, CORS
graph.py         — LangGraph state machine, intent classification
handlers/        — One module per intent (book, cancel, status, etc.)
helpers.py       — Shared utilities (geocoding, ride selection, formatting)
session_store.py — Persistent conversation storage (Redis or JSON)
rate_limiter.py  — Request throttling (Redis or in-memory)
config.py        — Environment configuration with validation
api_client.py    — HTTP client for downstream ride API
```

## Testing

```bash
pip install pytest pytest-asyncio
python -m pytest . -v
```
