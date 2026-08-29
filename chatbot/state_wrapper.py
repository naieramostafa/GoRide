from state import ChatState


class StateWrapper:
    """Encapsulates access to ChatState dict with cleaner properties and helpers."""

    def __init__(self, state: ChatState):
        self._state = state

    @property
    def state(self) -> ChatState:
        return self._state

    @property
    def response(self) -> str | None:
        return self._state.get("response")

    @response.setter
    def response(self, value: str):
        self._state["response"] = value

    @property
    def ctx(self) -> dict:
        return self._state.setdefault("context", {})

    @property
    def last_message(self) -> str:
        msgs = self._state.get("messages", [])
        return msgs[-1] if msgs else ""

    @property
    def token(self) -> str | None:
        return self.ctx.get("token")

    @property
    def user_id(self) -> str | None:
        user = self.ctx.get("user") or {}
        return user.get("id")

    @property
    def user_role(self) -> str | None:
        user = self.ctx.get("user") or {}
        return user.get("role")

    def require_auth(self) -> bool:
        if not self.token:
            self.response = "Please login first. Tell me your email and password."
            return False
        return True

    def require_user(self) -> bool:
        if not self.require_auth():
            return False
        if not self.user_id:
            self.response = "I need your user ID. Please login again."
            return False
        return True

    def to_dict(self) -> ChatState:
        return self._state
