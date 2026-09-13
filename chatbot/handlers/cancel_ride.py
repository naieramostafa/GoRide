from api_client import call_api
from helpers import pick_ride
from logger import logger
from state import ChatState
from state_wrapper import StateWrapper


async def handle_cancel_ride(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_auth():
        return s.to_dict()

    ride_id = await pick_ride(state, "cancel")
    if not ride_id:
        return s.to_dict()

    try:
        result = await call_api(f"/api/rides/{ride_id}/cancel", "POST", data={}, token=s.token)
        s.response = f"Ride cancelled. Status: {result.get('status', 'Cancelled')}"
    except Exception as e:
        logger.error("Failed to cancel ride: %s", e)
        s.response = f"Failed to cancel ride: {e!s}"
    return s.to_dict()
