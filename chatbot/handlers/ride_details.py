from logger import logger
from api_client import call_api
from helpers import pick_ride, format_ride_details
from state import ChatState
from state_wrapper import StateWrapper

async def handle_ride_details(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_auth():
        return s.to_dict()

    ride_id = await pick_ride(state, "view details of")
    if not ride_id:
        return s.to_dict()

    try:
        ride = await call_api(f"/api/rides/{ride_id}", "GET", token=s.token)
        s.response = format_ride_details(ride, detailed=True)
    except Exception as e:
        logger.error("Error fetching ride: %s", e)
        s.response = f"Error fetching ride: {str(e)}"
    return s.to_dict()
