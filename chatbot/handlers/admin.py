import re
from logger import logger
from api_client import call_api
from state import ChatState
from state_wrapper import StateWrapper

async def handle_admin(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_auth():
        return s.to_dict()

    if s.user_role != "Admin":
        s.response = (
            "Admin actions require Admin role. Available admin commands:\n"
            "- `verify driver <id>` — Verify a driver's documents\n\n"
            "Contact an administrator if you need access."
        )
        return s.to_dict()

    msg = s.last_message

    m = re.search(r'verify\s+driver\s+([\w-]+)', msg, re.I)
    if m:
        driver_id = m.group(1)
        try:
            result = await call_api(f"/api/drivers/{driver_id}/verify", "PUT", token=s.token)
            s.response = f"Driver {driver_id} verified successfully."
        except Exception as e:
            logger.error("Failed to verify driver: %s", e)
            s.response = f"Failed to verify driver: {str(e)}"
        return s.to_dict()

    s.response = (
        "Admin commands:\n"
        "- `verify driver <id>` — Verify a driver"
    )
    return s.to_dict()
