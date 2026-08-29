import re
from logger import logger
from api_client import call_api
from helpers import pick_ride
from state import ChatState
from state_wrapper import StateWrapper

async def handle_rating(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_auth():
        return s.to_dict()

    user_id = s.user_id
    msg = s.last_message
    ctx = s.ctx

    score = ctx.get("_pending_score")
    if not score:
        score_m = re.search(r'(\d+)\s*(?:star|point|out\s*of\s*\d)', msg, re.I)
        if score_m:
            score = int(score_m.group(1))
            ctx["_pending_score"] = score
        elif re.search(r'\b(rate|rating)\b', msg, re.I):
            s.response = "How many stars would you rate? (1-5)"
            return s.to_dict()

    if not score:
        s.response = "Please provide a rating score (1-5). Example: `rate ride 5 stars`"
        return s.to_dict()

    ride_id = await pick_ride(state, "rate")
    if not ride_id:
        return s.to_dict()

    try:
        ride = await call_api(f"/api/rides/{ride_id}", "GET", token=s.token)
        driver_id = ride.get("driverId") or ride.get("driver", {}).get("id", "00000000-0000-0000-0000-000000000000")
    except Exception:
        logger.exception("Failed to fetch ride %s for rating", ride_id)
        driver_id = "00000000-0000-0000-0000-000000000000"

    comment_m = re.search(r'(?:comment|said|reason)[:\s]+(.+)', msg, re.I)
    comment = comment_m.group(1).strip() if comment_m else None

    try:
        body = {
            "rideId": ride_id,
            "ratedByUserId": user_id,
            "ratedUserId": driver_id,
            "score": score,
        }
        if comment:
            body["comment"] = comment
        await call_api("/api/ratings", "POST", body, s.token)
        s.response = f"Rated your ride with {score} stars. Thank you!"
    except Exception as e:
        logger.error("Failed to submit rating: %s", e)
        s.response = f"Failed to submit rating: {str(e)}"
    return s.to_dict()
