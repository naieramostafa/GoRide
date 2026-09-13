from unittest.mock import AsyncMock, patch

import pytest

from state import ChatState


@pytest.fixture
def base_state():
    return ChatState(
        messages=["hello"],
        intent=None,
        response=None,
        context={},
    )


@pytest.fixture
def auth_state(base_state):
    base_state["context"] = {
        "token": "test-token",
        "user": {"id": "user-123", "firstName": "Test", "lastName": "User", "role": "Passenger"},
    }
    return base_state


import importlib

import handlers as _handlers_mod

_CALL_API_TARGETS = ["helpers.call_api"]
for name in _handlers_mod.__all__:
    mod_name = name.replace("handle_", "")
    if not name.startswith("handle_"):
        continue
    mod = importlib.import_module(f"handlers.{mod_name}")
    if hasattr(mod, "call_api"):
        _CALL_API_TARGETS.append(f"handlers.{mod_name}.call_api")


@pytest.fixture
def mock_call_api():
    mock = AsyncMock()
    patchers = [patch(t, mock) for t in _CALL_API_TARGETS]
    for p in patchers:
        p.start()
    yield mock
    for p in patchers:
        p.stop()


@pytest.fixture
def mock_httpx_client():
    with patch("httpx.AsyncClient") as mock_cls:
        client = AsyncMock()
        mock_cls.return_value.__aenter__ = AsyncMock(return_value=client)
        mock_cls.return_value.__aexit__ = AsyncMock(return_value=None)
        for method in ("get", "post", "put", "delete", "patch"):
            setattr(client, method, AsyncMock())
        yield client
