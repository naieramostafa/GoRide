import re
from logger import logger
from api_client import call_api
from helpers import geocode_address
from state import ChatState
from state_wrapper import StateWrapper
from config import DEFAULT_PICKUP_LAT, DEFAULT_PICKUP_LNG, DEFAULT_DROPOFF_LAT, DEFAULT_DROPOFF_LNG

def _parse_pickup_dropoff(msg: str, pickup: str | None, dropoff: str | None) -> tuple[str | None, str | None]:
    m = re.search(r'(?:from|pickup)\s+(.+?)\s+(?:to|dropoff)\s+(.+)', msg, re.I)
    if m:
        return m.group(1).strip(), m.group(2).strip()
    for l in [x.strip() for x in msg.split("\n") if x.strip()]:
        l_lower = l.lower()
        if ("pickup" in l_lower or l_lower.startswith("from")) and not pickup:
            pickup = re.sub(r'^(?:pickup|from)[:\s]+', '', l, flags=re.I).strip()
        elif ("dropoff" in l_lower or l_lower.startswith("to")) and not dropoff:
            dropoff = re.sub(r'^(?:dropoff|to|destination)[:\s]+', '', l, flags=re.I).strip()
    return pickup, dropoff


async def handle_book_ride(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_auth():
        return s.to_dict()

    msg = s.last_message
    ctx = s.ctx

    pickup = ctx.get("pickup")
    dropoff = ctx.get("dropoff")

    full = re.search(r'(?:from|pickup)\s+(.+?)\s+(?:to|dropoff)\s+(.+)', msg, re.I)
    if full:
        pickup, dropoff = full.group(1).strip(), full.group(2).strip()
    else:
        if not pickup or not dropoff:
            pickup, dropoff = _parse_pickup_dropoff(msg, pickup, dropoff)

    if not pickup:
        ctx["_awaiting"] = "pickup"
        s.response = "Where is the pickup location?"
        return s.to_dict()

    if not dropoff:
        ctx["_awaiting"] = "dropoff"
        ctx["pickup"] = pickup
        s.response = f"Pickup: {pickup}. Where is the dropoff location?"
        return s.to_dict()

    ctx["pickup"] = pickup
    ctx["dropoff"] = dropoff
    user_id = s.user_id

    if not user_id:
        s.response = "I need your user ID. Please login again with email and password."
        return s.to_dict()

    pickup_lat, pickup_lng = DEFAULT_PICKUP_LAT, DEFAULT_PICKUP_LNG
    dropoff_lat, dropoff_lng = DEFAULT_DROPOFF_LAT, DEFAULT_DROPOFF_LNG

    geo = await geocode_address(pickup)
    if geo:
        pickup_lat, pickup_lng = geo
    geo = await geocode_address(dropoff)
    if geo:
        dropoff_lat, dropoff_lng = geo

    body = {
        "userId": user_id,
        "pickupLatitude": pickup_lat,
        "pickupLongitude": pickup_lng,
        "dropoffLatitude": dropoff_lat,
        "dropoffLongitude": dropoff_lng,
        "pickupAddress": pickup,
        "dropoffAddress": dropoff
    }

    try:
        ride = await call_api("/api/rides", "POST", body, s.token)
        s.response = (
            f"Ride booked!\n"
            f"From: {pickup}\n"
            f"To: {dropoff}\n"
            f"Fare: ${ride['fare']}\n"
            f"Distance: {ride.get('distanceKm', '?')} km\n"
            f"Status: {ride['status']}"
        )
        ctx["lastRideId"] = ride["id"]
        ctx.pop("_awaiting", None)
        ctx.pop("pickup", None)
        ctx.pop("dropoff", None)
    except Exception as e:
        logger.error("Failed to book ride: %s", e)
        s.response = f"Failed to book ride: {str(e)}"

    return s.to_dict()
