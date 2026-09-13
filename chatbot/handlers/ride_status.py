from api_client import call_api
from helpers import format_ride_details, resolve_ride_id
from logger import logger
from state import ChatState
from state_wrapper import StateWrapper


async def handle_ride_status(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_auth():
        return s.to_dict()

    # Try to resolve a specific ride first
    ride_id = await resolve_ride_id(state)
    if ride_id:
        return await _show_single_ride(s, ride_id)

    # Fall back to fetching ride list
    return await _show_ride_list(s)


async def _show_single_ride(s: StateWrapper, ride_id: str) -> ChatState:
    try:
        ride = await call_api(f"/api/rides/{ride_id}", "GET", token=s.token)
        s.response = format_ride_details(ride, detailed=False)
    except Exception as e:
        logger.error("Error fetching ride details: %s", e)
        s.response = f"Error fetching ride details: {e!s}"
    return s.to_dict()


async def _show_ride_list(s: StateWrapper) -> ChatState:
    user_id = s.user_id
    try:
        rides = await call_api(f"/api/users/{user_id}/rides", "GET", token=s.token)
        if rides and len(rides) > 0:
            s.ctx["_ride_list"] = rides
            s.ctx["_pending_ride_action"] = "view"
            lines = ["**Your Rides:**"]
            for i, r in enumerate(rides, 1):
                pickup = r.get("pickupLocation", {}).get("address", "?")
                dropoff = r.get("dropoffLocation", {}).get("address", "?")
                lines.append(f"  {i}. {pickup} -> {dropoff} - {r.get('status', '?')} - ${r.get('fare', '?')}")
            s.response = "\n".join(lines)
        else:
            s.response = "No rides found for your account."
    except Exception as e:
        logger.error("Error fetching ride list: %s", e)
        s.response = f"Error fetching ride list: {e!s}"
    return s.to_dict()
