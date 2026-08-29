from logger import logger
from api_client import call_api
from state import ChatState
from state_wrapper import StateWrapper

async def handle_profile(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    if not s.require_user():
        return s.to_dict()

    try:
        user = await call_api(f"/api/users/{s.user_id}", "GET", token=s.token)
        s.response = (
            f"**Your Profile:**\n"
            f"Name: {user.get('firstName', '?')} {user.get('lastName', '?')}\n"
            f"Email: {user.get('email', '?')}\n"
            f"Phone: {user.get('phone', '?')}\n"
            f"Role: {user.get('role', '?')}\n"
            f"Active: {user.get('isActive', '?')}\n"
            f"Joined: {user.get('createdAt', '?')}"
        )
    except Exception as e:
        logger.error("Error fetching profile: %s", e)
        s.response = f"Error fetching profile: {str(e)}"
    return s.to_dict()
