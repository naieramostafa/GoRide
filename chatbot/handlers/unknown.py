from state import ChatState
from state_wrapper import StateWrapper

async def handle_unknown(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    s.response = (
        "I can help you with:\n"
        "- Booking a ride (say 'book a ride')\n"
        "- Checking ride status (say 'check status')\n"
        "- Cancelling a ride (say 'cancel ride')\n"
        "- General support\n\n"
        "First, login with your email and password (e.g. 'my email is john@email.com, password is mypass')"
    )
    return s.to_dict()
