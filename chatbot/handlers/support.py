from state import ChatState
from state_wrapper import StateWrapper

async def handle_support(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    s.response = (
        "I'm here to help! Common topics:\n"
        "- How to update payment method\n"
        "- How to become a driver\n"
        "- Cancellation policy\n"
        "- Lost and found\n\n"
        "You can also try:\n"
        "- 'Book a ride'\n"
        "- 'Check my ride status'\n"
        "- 'Cancel a ride'"
    )
    return s.to_dict()
