import re

from api_client import call_api
from config import DEFAULT_RIDE_DISTANCE_KM, DEFAULT_RIDE_DURATION_MIN, DEFAULT_RIDE_FINAL_FARE
from helpers import pick_ride
from logger import logger
from state import ChatState
from state_wrapper import StateWrapper


async def handle_driver_actions(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_auth():
        return s.to_dict()

    user_id = s.user_id
    if not user_id:
        s.response = "I need your user ID. Please login again."
        return s.to_dict()

    msg = s.last_message.lower()
    ctx = s.ctx
    driver_endpoint = f"/api/drivers/{user_id}/rides"

    if re.match(r'^\d+$', msg.strip()) and ctx.get("_ride_list"):
        action = ctx.get("_pending_ride_action", "")
        if action in ("start", "complete"):
            return await _perform_ride_action(s, action, driver_endpoint)

    if "start" in msg and ("ride" in msg or "trip" in msg):
        ctx["_pending_ride_action"] = "start"
        return await _perform_ride_action(s, "start", driver_endpoint)

    if "complete" in msg or "finish" in msg or "end" in msg:
        ctx["_pending_ride_action"] = "complete"
        return await _perform_ride_action(s, "complete", driver_endpoint)

    if "available" in msg or "online" in msg or "offline" in msg:
        available = "online" in msg or ("available" in msg and "unavailable" not in msg and "offline" not in msg)
        try:
            await call_api(f"/api/drivers/{user_id}/availability", "PUT", available, s.token)
            status = "online" if available else "offline"
            s.response = f"You are now {status}."
        except Exception as e:
            err = str(e)
            logger.error("Failed to set online: %s", err)
            if "not verified" in err.lower():
                s.response = f"Cannot go online: {err}. Ask an admin to verify you."
            else:
                s.response = f"Failed to update availability: {err}"
        return s.to_dict()

    if "location" in msg or "lat" in msg or "lng" in msg:
        lat_m = re.search(r'lat\s*[=:]\s*([-\d.]+)', msg, re.IGNORECASE)
        lng_m = re.search(r'lng\s*[=:]\s*([-\d.]+)', msg, re.IGNORECASE)
        if lat_m and lng_m:
            try:
                body = {"latitude": float(lat_m.group(1)), "longitude": float(lng_m.group(1))}
                await call_api(f"/api/drivers/{user_id}/location", "PUT", body, s.token)
                s.response = f"Location updated to ({lat_m.group(1)}, {lng_m.group(1)})."
            except Exception as e:
                logger.error("Failed to update location: %s", e)
                s.response = f"Failed to update location: {e!s}"
            return s.to_dict()
        s.response = "Please provide latitude and longitude. Example: `update my location lat=40.7128 lng=-74.0060`"
        return s.to_dict()

    if "my rides" in msg or "my trips" in msg:
        try:
            rides = await call_api(driver_endpoint, "GET", token=s.token)
            if rides and len(rides) > 0:
                lines = ["**Your Assigned Rides (Driver):**"]
                for i, r in enumerate(rides, 1):
                    pickup = r.get("pickupLocation", {}).get("address", "?")
                    dropoff = r.get("dropoffLocation", {}).get("address", "?")
                    lines.append(f"  {i}. {pickup} -> {dropoff} - {r.get('status', '?')} - ${r.get('fare', '?')}")
                s.response = "\n".join(lines)
            else:
                s.response = "No rides assigned to you as a driver."
        except Exception as e:
            logger.error("Failed to get driver rides: %s", e)
            s.response = f"Error: {e!s}"
        return s.to_dict()

    s.response = (
        "Driver commands:\n"
        "- `set me online/offline` - toggle availability\n"
        "- `update my location lat=... lng=...` - update GPS\n"
        "- `show my rides` - view assigned rides\n"
        "- `start ride` - start the current ride\n"
        "- `complete ride` - finish the current ride"
    )
    return s.to_dict()


async def _perform_ride_action(s: StateWrapper, action: str, driver_endpoint: str) -> ChatState:
    token = s.token
    user_id = s.user_id
    ride_id = await pick_ride(s.state, action, driver_endpoint)
    if not ride_id:
        return s.to_dict()

    try:
        if action == "start":
            await call_api(f"/api/rides/{ride_id}/start", "POST", user_id, token)
            s.response = "Ride has been started!"
        elif action == "complete":
            body = {
                "rideId": ride_id,
                "driverId": user_id,
                "distanceKm": DEFAULT_RIDE_DISTANCE_KM,
                "durationMinutes": DEFAULT_RIDE_DURATION_MIN,
                "finalFare": DEFAULT_RIDE_FINAL_FARE
            }
            await call_api(f"/api/rides/{ride_id}/complete", "POST", body, token)
            s.response = "Ride completed! The passenger can now rate and pay."
    except Exception as e:
        logger.error("Failed to %s ride: %s", action, e)
        s.response = f"Failed to {action} ride: {e!s}"
    return s.to_dict()
