from typing import TypedDict


class ChatState(TypedDict):
    messages: list
    intent: str | None
    response: str | None
    context: dict
