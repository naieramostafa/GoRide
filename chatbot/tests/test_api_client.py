from unittest.mock import AsyncMock, MagicMock, patch

import pytest

from api_client import call_api


@pytest.fixture
def mock_api_client():
    client = AsyncMock()
    client.is_closed = False

    async def _fake_get_client():
        return client

    with patch("api_client._get_client", _fake_get_client):
        yield client


class TestCallApi:
    @pytest.mark.asyncio
    async def test_get_success(self, mock_api_client):
        resp = MagicMock(status_code=200, content=b'{"key": "value"}')
        resp.json.return_value = {"key": "value"}
        mock_api_client.get.return_value = resp
        result = await call_api("/test", "GET")
        assert result == {"key": "value"}

    @pytest.mark.asyncio
    async def test_post_success(self, mock_api_client):
        resp = MagicMock(status_code=201, content=b'{"id": 1}')
        resp.json.return_value = {"id": 1}
        mock_api_client.post.return_value = resp
        result = await call_api("/test", "POST", {"name": "test"})
        assert result == {"id": 1}

    @pytest.mark.asyncio
    async def test_put_success(self, mock_api_client):
        resp = MagicMock(status_code=200, content=b'{"ok": true}')
        resp.json.return_value = {"ok": True}
        mock_api_client.put.return_value = resp
        result = await call_api("/test", "PUT", {"key": "val"})
        assert result == {"ok": True}

    @pytest.mark.asyncio
    async def test_unauthorized(self, mock_api_client):
        resp = MagicMock(status_code=401)
        mock_api_client.get.return_value = resp
        with pytest.raises(Exception, match="session has expired"):
            await call_api("/test", "GET")

    @pytest.mark.asyncio
    async def test_api_error(self, mock_api_client):
        resp = MagicMock(status_code=404, text="Not found")
        mock_api_client.get.return_value = resp
        with pytest.raises(Exception, match="HTTP 404"):
            await call_api("/test", "GET")

    @pytest.mark.asyncio
    async def test_unsupported_method(self, mock_api_client):
        with pytest.raises(ValueError, match="Unsupported method"):
            await call_api("/test", "DELETE")
