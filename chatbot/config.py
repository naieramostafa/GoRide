import os
import sys
from dotenv import load_dotenv

APP_ENV = os.getenv("APP_ENV", "dev")
load_dotenv(f".env.{APP_ENV}", override=True)
load_dotenv(override=True)

RIDE_API_URL = os.getenv("RIDE_API_URL", "http://localhost:5000")
GOOGLE_API_KEY = os.getenv("GOOGLE_API_KEY")
GEMINI_MODEL = os.getenv("GEMINI_MODEL", "gemini-2.0-flash")
PAYMENT_CHECKOUT_URL = os.getenv("PAYMENT_CHECKOUT_URL", "http://localhost:5173/checkout")

DEFAULT_PICKUP_LAT = float(os.getenv("DEFAULT_PICKUP_LAT", "40.7128"))
DEFAULT_PICKUP_LNG = float(os.getenv("DEFAULT_PICKUP_LNG", "-74.0060"))
DEFAULT_DROPOFF_LAT = float(os.getenv("DEFAULT_DROPOFF_LAT", "40.6892"))
DEFAULT_DROPOFF_LNG = float(os.getenv("DEFAULT_DROPOFF_LNG", "-74.0445"))

DEFAULT_RIDE_DISTANCE_KM = float(os.getenv("DEFAULT_RIDE_DISTANCE_KM", "5.2"))
DEFAULT_RIDE_DURATION_MIN = int(os.getenv("DEFAULT_RIDE_DURATION_MIN", "15"))
DEFAULT_RIDE_FINAL_FARE = float(os.getenv("DEFAULT_RIDE_FINAL_FARE", "15.50"))

_API_KEY_PLACEHOLDERS = ["your-api-key", "sk-placeholder", "xxxxx", "CHANGE_ME"]
_SESSION_STORE_PATH = os.getenv("SESSION_STORE_PATH", "sessions.json")
_API_TIMEOUT_SEC = int(os.getenv("API_TIMEOUT_SEC", "30"))
REDIS_URL = os.getenv("REDIS_URL", "")
LOG_JSON = os.getenv("LOG_JSON", "").lower() in ("1", "true", "yes")
_LOG_LEVEL = os.getenv("LOG_LEVEL", "INFO")


def _is_placeholder(val: str | None) -> bool:
    if not val:
        return True
    stripped = val.strip()
    if not stripped:
        return True
    for p in _API_KEY_PLACEHOLDERS:
        if stripped.startswith(p):
            return True
    return False


def validate_env():
    missing: list[str] = []
    if not RIDE_API_URL:
        missing.append("RIDE_API_URL")
    if missing and not os.getenv("PYTEST_CURRENT_TEST"):
        print(f"[ERROR] Missing or invalid environment variables: {', '.join(missing)}", file=sys.stderr)
        sys.exit(1)

    if _is_placeholder(GOOGLE_API_KEY):
        print("[WARN] GOOGLE_API_KEY is missing or a placeholder — intent classification will use rule-based fallback only.", file=sys.stderr)
