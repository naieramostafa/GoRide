import asyncio
import json
import os
from typing import Protocol

from config import _SESSION_STORE_PATH, REDIS_URL
from logger import logger


class SessionStore(Protocol):
    async def load(self, conv_id: str) -> dict | None: ...
    async def save(self, conv_id: str, session: dict): ...
    async def delete(self, conv_id: str): ...
    async def ping(self) -> bool: ...
    async def close(self): ...


class JsonSessionStore:
    def __init__(self, path: str = _SESSION_STORE_PATH):
        self._path = path
        self._lock = asyncio.Lock()

    async def _read(self) -> dict:
        try:
            with open(self._path, "r") as f:
                return json.load(f)
        except (FileNotFoundError, json.JSONDecodeError):
            return {}

    async def _write(self, data: dict):
        with open(self._path, "w") as f:
            json.dump(data, f, indent=2)

    async def load(self, conv_id: str) -> dict | None:
        async with self._lock:
            store = await self._read()
            return store.get(conv_id)

    async def save(self, conv_id: str, session: dict):
        async with self._lock:
            store = await self._read()
            store[conv_id] = session
            await self._write(store)

    async def delete(self, conv_id: str):
        async with self._lock:
            store = await self._read()
            store.pop(conv_id, None)
            await self._write(store)

    async def ping(self) -> bool:
        return os.path.exists(self._path) or True

    async def close(self):
        pass


class RedisSessionStore:
    def __init__(self, url: str):
        import redis.asyncio as aioredis
        self._client = aioredis.from_url(url, decode_responses=True)

    async def load(self, conv_id: str) -> dict | None:
        raw = await self._client.get(f"session:{conv_id}")
        if raw is None:
            return None
        return json.loads(raw)

    async def save(self, conv_id: str, session: dict):
        await self._client.set(f"session:{conv_id}", json.dumps(session))

    async def delete(self, conv_id: str):
        await self._client.delete(f"session:{conv_id}")

    async def ping(self) -> bool:
        try:
            return await self._client.ping()
        except Exception:
            return False

    async def close(self):
        await self._client.aclose()


def _create_store() -> SessionStore:
    if REDIS_URL:
        try:
            store = RedisSessionStore(REDIS_URL)
            logger.info("Session store: Redis at %s", REDIS_URL)
            return store
        except Exception as e:
            logger.warning("Redis unavailable (%s), falling back to JSON store", e)
    logger.info("Session store: JSON file at %s", _SESSION_STORE_PATH)
    return JsonSessionStore()


_store: SessionStore = _create_store()


async def load_session(conv_id: str) -> dict | None:
    return await _store.load(conv_id)


async def save_session(conv_id: str, session: dict):
    await _store.save(conv_id, session)


async def delete_session(conv_id: str):
    await _store.delete(conv_id)


async def ping_store() -> bool:
    return await _store.ping()


async def close_store():
    await _store.close()
