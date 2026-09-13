import re

from api_client import call_api
from logger import logger
from state import ChatState
from state_wrapper import StateWrapper


async def handle_login(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    msg = s.last_message
    email_match = re.search(r'[\w.+-]+@[\w-]+\.[\w.]+', msg)
    pass_match = re.search(r'password\s+(?:is\s+)?(\S+)', msg, re.IGNORECASE)

    if email_match and pass_match:
        email = email_match.group()
        password = pass_match.group(1).strip(".,!?")
        try:
            result = await call_api("/api/users/login", "POST", {"email": email, "password": password})
            s.ctx["token"] = result["token"]
            s.ctx["user"] = result["user"]
            role = result['user']['role']
            if role == "Driver":
                suggestions = (
                    "- Go online/offline\n"
                    "- View my rides\n"
                    "- Update my location\n"
                    "- Help"
                )
            elif role == "Admin":
                suggestions = (
                    "- Verify a driver\n"
                    "- List / manage tasks\n"
                    "- Help"
                )
            else:
                suggestions = (
                    "- Book a ride\n"
                    "- Check ride status\n"
                    "- Cancel a ride\n"
                    "- Help"
                )
            s.response = (
                f"Logged in as {result['user']['firstName']} {result['user']['lastName']} ({role}).\n"
                f"What would you like to do?\n"
                f"{suggestions}"
            )
        except Exception as e:
            logger.error("Login failed for %s: %s", email, e)
            s.response = f"Login failed: {e!s}"
    else:
        s.response = "Please provide your email and password, e.g.:\n`my email is john@email.com and password is mypass`"
    return s.to_dict()
