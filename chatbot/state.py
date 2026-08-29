from typing import TypedDict, Optional

class ChatState(TypedDict):
    messages: list
    intent: Optional[str]
    response: Optional[str]
    context: dict
