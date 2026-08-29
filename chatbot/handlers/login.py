import re
from logger import logger
from api_client import call_api
from state import ChatState
from state_wrapper import StateWrapper

async def handle_login(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    msg = s.last_message
    email_match = re.search(r'[\w.+-]+@[\w-]+\.[\w.]+', msg)
    pass_match = re.search(r'password\s+(?:is\s+)?(\S+)', msg, re.I)

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
                    f"- Go online/offline\n"
                    f"- View my rides\n"
                    f"- Update my location\n"
                    f"- Help"
                )
            elif role == "Admin":
                suggestions = (
                    f"- Verify a driver\n"
                    f"- List / manage tasks\n"
                    f"- Help"
                )
            else:
                suggestions = (
                    f"- Book a ride\n"
                    f"- Check ride status\n"
                    f"- Cancel a ride\n"
                    f"- Help"
                )
            s.response = (
                f"Logged in as {result['user']['firstName']} {result['user']['lastName']} ({role}).\n"
                f"What would you like to do?\n"
                f"{suggestions}"
            )
        except Exception as e:
            logger.error("Login failed for %s: %s", email, e)
            s.response = f"Login failed: {str(e)}"
    else:
        s.response = "Please provide your email and password, e.g.:\n`my email is john@email.com and password is mypass`"
    return s.to_dict()
