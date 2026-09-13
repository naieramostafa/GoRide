from unittest.mock import MagicMock

import pytest

from api_client import call_api


class TestCallApi:
    @pytest.mark.asyncio
    async def test_get_success(self, mock_httpx_client):
        resp = MagicMock(status_code=200, content=b'{"key": "value"}')
        resp.json.return_value = {"key": "value"}
        mock_httpx_client.get.return_value = resp
        result = await call_api("/test", "GET")
        assert result == {"key": "value"}

    @pytest.mark.asyncio
    async def test_post_success(self, mock_httpx_client):
        resp = MagicMock(status_code=201, content=b'{"id": 1}')
        resp.json.return_value = {"id": 1}
        mock_httpx_client.post.return_value = resp
        result = await call_api("/test", "POST", {"name": "test"})
        assert result == {"id": 1}

    @pytest.mark.asyncio
    async def test_put_success(self, mock_httpx_client):
        resp = MagicMock(status_code=200, content=b'{"ok": true}')
        resp.json.return_value = {"ok": True}
        mock_httpx_client.put.return_value = resp
        result = await call_api("/test", "PUT", {"key": "val"})
        assert result == {"ok": True}

    @pytest.mark.asyncio
    async def test_unauthorized(self, mock_httpx_client):
        resp = MagicMock(status_code=401)
        mock_httpx_client.get.return_value = resp
        with pytest.raises(Exception, match="TOKEN_EXPIRED"):
            await call_api("/test", "GET")

    @pytest.mark.asyncio
    async def test_api_error(self, mock_httpx_client):
        resp = MagicMock(status_code=404, text="Not found")
        mock_httpx_client.get.return_value = resp
        with pytest.raises(Exception, match="API error"):
            await call_api("/test", "GET")

    @pytest.mark.asyncio
    async def test_unsupported_method(self, mock_httpx_client):
        with pytest.raises(ValueError, match="Unsupported method"):
            await call_api("/test", "DELETE")
