import re
import httpx
from logger import logger
from api_client import call_api

def parse_vehicle_details(msg: str, user_id: str) -> dict | None:
    make_m = re.search(r'(?:car|make)\s+(?:is\s+)?(\w+)', msg, re.I)
    if not make_m:
        return None
    body = {
        "userId": user_id,
        "make": make_m.group(1),
        "model": "",
        "year": "",
        "color": "",
        "licensePlate": "",
        "vehicleType": 0,
        "capacity": 4,
        "licenseNumber": ""
    }
    m = re.search(r'model\s+(\w+)', msg, re.I)
    if m: body["model"] = m.group(1)
    m = re.search(r'(?:year|20\d{2})', msg)
    if m: body["year"] = m.group()
    m = re.search(r'(?:color|plate)\s+(\w+)', msg, re.I)
    if m: body["color"] = m.group(1)
    m = re.search(r'(?:plate|license\s*plate)\s+(\S+)', msg, re.I)
    if m: body["licensePlate"] = m.group(1)
    m = re.search(r'(?:license\s*(?:number|#)?)\s+(\S+)', msg, re.I)
    if m: body["licenseNumber"] = m.group(1)
    return body

async def geocode_address(address: str) -> tuple[float, float] | None:
    try:
        async with httpx.AsyncClient() as client:
            resp = await client.get(
                "https://nominatim.openstreetmap.org/search",
                params={"q": address, "format": "json", "limit": 1},
                headers={"User-Agent": "RideSharingChatbot/1.0"},
                timeout=10
            )
            results = resp.json()
            if results:
                return (float(results[0]["lat"]), float(results[0]["lon"]))
    except Exception:
        logger.exception("Geocoding failed for address: %s", address)
    return None

def format_ride_details(ride: dict, detailed: bool = False) -> str:
    pickup = ride.get("pickupLocation", {}).get("address", "?")
    dropoff = ride.get("dropoffLocation", {}).get("address", "?")
    lines = [
        f"**Ride: {pickup} -> {dropoff}**",
        f"Status: {ride.get('status', '?')}",
        f"Fare: ${ride.get('fare', '?')}",
    ]
    if detailed:
        lines.append(f"Distance: {ride.get('distanceKm', '?')} km")
        lines.append(f"Duration: {ride.get('durationMinutes', '?')} min")
    driver = ride.get('driver', {}).get('fullName') or ride.get('driverName') or "Not assigned"
    lines.append(f"Driver: {driver}")
    lines.append(f"Requested: {ride.get('requestedAt', '?')}")
    return "\n".join(lines)


async def resolve_ride_id(state) -> str | None:
    msg = state["messages"][-1].strip()
    ctx = state["context"]
    last_id = ctx.get("lastRideId")
    ride_list = ctx.get("_ride_list")

    m = re.search(r'([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})', msg, re.I)
    if m:
        ctx["lastRideId"] = m.group(1)
        return m.group(1)

    if ride_list:
        idx = None
        if re.match(r'^\d+$', msg):
            idx = int(msg)
        else:
            m = re.search(r'(?:#|number|ride)\s*(\d+)', msg, re.I)
            if m:
                idx = int(m.group(1))
        if idx and 1 <= idx <= len(ride_list):
            ctx["lastRideId"] = ride_list[idx - 1]["id"]
            return ride_list[idx - 1]["id"]

    if last_id and (re.match(r'^\d+$', msg) or re.search(r'\b(last|previous|most recent)\b', msg, re.I)):
        return last_id

    return None

async def pick_ride(state, action_label: str, endpoint: str | None = None) -> str | None:
    """Try to resolve ride_id, or ask user to pick from list.
    endpoint defaults to /api/users/{user_id}/rides (passenger).
    For drivers, pass endpoint='/api/drivers/{user_id}/rides'."""
    token = state["context"].get("token")
    user_id = state["context"].get("user", {}).get("id")
    if not token or not user_id:
        state["response"] = "Please login first."
        return None

    ride_id = await resolve_ride_id(state)
    if ride_id:
        state["context"].pop("_ride_list", None)
        return ride_id

    if state.get("response"):
        return None

    url = endpoint or f"/api/users/{user_id}/rides"
    try:
        rides = await call_api(url, "GET", token=token)
        if rides and len(rides) > 0:
            state["context"]["_ride_list"] = rides
            state["context"]["_pending_ride_action"] = action_label
            lines = [f"Which ride would you like to {action_label}?"]
            for i, r in enumerate(rides, 1):
                pickup = r.get("pickupLocation", {}).get("address", "?")
                dropoff = r.get("dropoffLocation", {}).get("address", "?")
                lines.append(f"  {i}. {pickup} -> {dropoff} - {r.get('status', '?')} - ${r.get('fare', '?')}")
            state["response"] = "\n".join(lines)
        else:
            state["response"] = "No rides found for your account."
    except Exception as e:
        state["response"] = f"Error fetching rides: {str(e)}"
    return None
