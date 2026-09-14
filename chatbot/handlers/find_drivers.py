import re

from api_client import call_api
from logger import logger
from state import ChatState
from state_wrapper import StateWrapper


async def handle_find_drivers(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_auth():
        return s.to_dict()

    msg = s.last_message
    lat = None
    lng = None
    radius: float = 5

    m = re.search(r'lat\s*[=:]\s*([-\d.]+)', msg, re.IGNORECASE)
    if m: lat = float(m.group(1))
    m = re.search(r'lng\s*[=:]\s*([-\d.]+)', msg, re.IGNORECASE)
    if m: lng = float(m.group(1))
    m = re.search(r'(?:radius|within|near)\s+([\d.]+)\s*(?:km|miles|mi)?', msg, re.IGNORECASE)
    if m: radius = float(m.group(1))

    if lat is None or lng is None:
        s.response = "I need your location to find nearby drivers. Example: `find drivers near lat=40.7128 lng=-74.0060 radius=5`"
        return s.to_dict()

    try:
        raw = await call_api(f"/api/drivers/nearby?lat={lat}&lng={lng}&radius={radius}", "GET", token=s.token)
        drivers: list = []
        if isinstance(raw, list):
            drivers = raw
        elif isinstance(raw, dict):
            drivers = raw.get("value") or raw.get("drivers") or []
        else:
            drivers = []
        if drivers and len(drivers) > 0:
            lines = [f"**{len(drivers)} nearby driver(s):**"]
            for d in drivers:
                name = f"{d.get('firstName', '')} {d.get('lastName', '')}".strip() or "Unknown"
                rating = d.get("rating", "?")
                veh = d.get("vehicleType") or d.get("type") or ""
                veh_make = d.get("vehicleMake") or d.get("make") or ""
                veh_model = d.get("vehicleModel") or d.get("model") or ""
                vehicle = f" - {veh} {veh_make} {veh_model}".strip() if veh else ""
                lines.append(f"- {name} ({rating}){vehicle}")
            s.response = "\n".join(lines)
        else:
            s.response = "No drivers found nearby."
    except Exception as e:
        logger.error("Error finding drivers: %s", e)
        s.response = f"Error finding drivers: {e!s}"
    return s.to_dict()
