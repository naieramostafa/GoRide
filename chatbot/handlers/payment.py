import re
from logger import logger
from api_client import call_api
from helpers import pick_ride
from state import ChatState
from state_wrapper import StateWrapper
from config import PAYMENT_CHECKOUT_URL

async def handle_payment(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_auth():
        return s.to_dict()

    msg = s.last_message
    ctx = s.ctx
    pending = ctx.get("_pending_payment")

    if pending:
        try:
            result = await call_api(f"/api/payments/{pending['ride_id']}/process", "POST",
                                    {"paymentMethod": "pm_card_visa", "confirm": True}, s.token)
            ctx.pop("_pending_payment", None)
            status = result.get("status", "Processing")
            s.response = f"Payment {status.lower()}."
        except Exception as e:
            logger.error("Payment confirmation failed: %s", e)
            s.response = f"Payment failed: {str(e)}"
        return s.to_dict()

    pi_match = re.search(r'(pi_\w+)', msg)
    if pi_match and not pending:
        s.response = "No pending payment found. First say 'pay for ride' to start."
        return s.to_dict()

    ride_id = await pick_ride(state, "pay for")
    if not ride_id:
        return s.to_dict()

    try:
        result = await call_api(f"/api/payments/{ride_id}/process", "POST",
                                {"paymentMethod": None, "confirm": False}, s.token)
        client_secret = result.get("clientSecret", "")
        pi_id = client_secret.split("_secret_")[0] if "_secret_" in client_secret else ""
        ctx["_pending_payment"] = {"ride_id": ride_id, "client_secret": client_secret}

        link = f"{PAYMENT_CHECKOUT_URL}/{pi_id}" if pi_id else "the payment page"
        s.response = (
            f"Payment initiated! Status: {result.get('status', 'Processing')}.\n"
            f"Complete at: {link}\n"
            f"Or reply 'confirm payment' to complete now with a saved card."
        )
    except Exception as e:
        logger.error("Payment initiation failed: %s", e)
        s.response = f"Payment failed: {str(e)}"
    return s.to_dict()
