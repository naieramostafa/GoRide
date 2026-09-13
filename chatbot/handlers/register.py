import re

from api_client import call_api
from helpers import parse_vehicle_details
from logger import logger
from state import ChatState
from state_wrapper import StateWrapper


async def handle_register(state: ChatState) -> ChatState:
    s = StateWrapper(state)
    msg = s.last_message.lower()
    ctx = s.ctx

    if ctx.get("_awaiting_driver_vehicle"):
        return await _register_vehicle(s)

    token = ctx.get("token")
    is_new_user = "first" in msg and "last" in msg and "@" in msg

    if token and "driver" in msg and ("upgrade" in msg or "become" in msg or ("register" in msg and not is_new_user)):
        return await _upgrade_to_driver(s)

    if token and ("car" in msg or "vehicle" in msg or "make" in msg):
        return await _register_vehicle(s)

    return await _register_new_user(s)

async def _register_vehicle(s: StateWrapper) -> ChatState:
    ctx = s.ctx
    token = ctx.get("token")
    user_id = s.user_id
    msg = s.last_message.lower()

    if not token or not user_id:
        ctx.pop("_awaiting_driver_vehicle", None)
        s.response = "Please login first, then tell me your car details."
        return s.to_dict()

    body = parse_vehicle_details(msg, user_id)
    if not body:
        s.response = "Tell me your vehicle details. Example: `my car is Toyota Camry 2024 White plate ABC-1234 license X12345`"
        return s.to_dict()

    try:
        result = await call_api("/api/users/register-driver", "POST", body, token)
        ctx.pop("_awaiting_driver_vehicle", None)
        s.response = f"Vehicle registered: {result.get('make', body['make'])}. Your driver account is pending admin verification. Ask an admin to verify you, then you can start driving."
    except Exception as e:
        logger.error("Vehicle registration failed: %s", e)
        s.response = f"Vehicle registration failed: {e!s}"
    return s.to_dict()

async def _upgrade_to_driver(s: StateWrapper) -> ChatState:
    ctx = s.ctx
    token = ctx.get("token")
    user_id = s.user_id
    msg = s.last_message.lower()

    body = parse_vehicle_details(msg, user_id)
    if not body:
        s.response = "To register as a driver, I need your vehicle details. Example: `register me as driver with Toyota Camry 2024 black plate ABC-1234 license X12345 sedan 4 seats`"
        return s.to_dict()

    try:
        result = await call_api("/api/users/register-driver", "POST", body, token)
        ctx["_awaiting_driver_vehicle"] = True
        s.response = f"Driver registration submitted! Vehicle: {result.get('make', body['make'])}.\nNow tell me your vehicle details again or say `skip`."
    except Exception as e:
        logger.error("Driver upgrade failed: %s", e)
        s.response = f"Driver registration failed: {e!s}"
    return s.to_dict()


async def _register_new_user(s: StateWrapper) -> ChatState:
    ctx = s.ctx
    msg = s.last_message.lower()

    m = re.search(r'register\s+(?:me\s+)?as\s+(\w+)', msg, re.IGNORECASE)
    role = m.group(1).capitalize() if m else "Passenger"
    if role not in ("Passenger", "Driver"):
        role = "Passenger"

    if "first" not in msg or "last" not in msg or "@" not in msg:
        s.response = "To register, I need: First Name, Last Name, Email, Phone, and Password. Example: `register me as Passenger with first John last Doe email john@email.com phone 1234567890 password mypass`"
        return s.to_dict()

    email_m = re.search(r'[\w.+-]+@[\w-]+\.[\w.]+', msg)
    first_m = re.search(r'first\s+(\w+)', msg, re.IGNORECASE)
    last_m = re.search(r'last\s+(\w+)', msg, re.IGNORECASE)
    phone_m = re.search(r'phone\s+(\d+)', msg)
    pass_m = re.search(r'password\s+(\S+)', msg, re.IGNORECASE)

    body = {
        "firstName": first_m.group(1) if first_m else "New",
        "lastName": last_m.group(1) if last_m else "User",
        "email": email_m.group() if email_m else "",
        "phone": phone_m.group(1) if phone_m else "0000000000",
        "password": pass_m.group(1).strip(".,!?") if pass_m else "defaultpass",
        "role": role
    }

    if not body["email"]:
        s.response = "Please provide a valid email address."
        return s.to_dict()

    try:
        result = await call_api("/api/users/register", "POST", body)
        name = f"{result.get('firstName', body['firstName'])} {result.get('lastName', body['lastName'])}"
        reply = f"Registered as {name} ({role}). You can now login with your email and password."

        if role == "Driver":
            reg_token = result.get("token")
            reg_user_id = result.get("user", {}).get("id")
            if reg_token and reg_user_id:
                ctx["token"] = reg_token
                ctx["user"] = result.get("user", {})
                ctx["_awaiting_driver_vehicle"] = True
                reply += "\nNow tell me your vehicle details. Example: `my car is Toyota Camry 2024 White plate ABC-1234 license X12345`"

        s.response = reply
    except Exception as e:
        err_msg = str(e)
        logger.error("User registration failed: %s", err_msg)
        if "Email already registered" in err_msg:
            reply = "That email is already registered. Try logging in instead: `my email is bob@test.com and password is bobpass`"
            if role == "Driver":
                reply += "\nOr register with a different email."
            s.response = reply
        else:
            s.response = f"Registration failed: {err_msg}"
    return s.to_dict()
