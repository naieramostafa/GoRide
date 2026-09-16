import asyncio
import os
import time
from typing import Protocol

from config import REDIS_URL
from logger import logger

_RATE_LIMIT = int(os.getenv("RATE_LIMIT", "20"))
_RATE_WINDOW = int(os.getenv("RATE_WINDOW_SEC", "60"))


class RateLimiter(Protocol):
    async def check(self, key: str) -> bool: ...
    async def close(self): ...


class InMemoryRateLimiter:
    def __init__(self):
        self._buckets: dict[str, list[float]] = {}
        self._lock = asyncio.Lock()

    async def check(self, key: str) -> bool:
        async with self._lock:
            now = time.time()
            window_start = now - _RATE_WINDOW
            timestamps = self._buckets.setdefault(key, [])
            timestamps[:] = [t for t in timestamps if t > window_start]
            if len(timestamps) >= _RATE_LIMIT:
                return False
            timestamps.append(now)
            return True

    async def close(self):
        self._buckets.clear()


class RedisRateLimiter:
    def __init__(self, url: str):
        import redis.asyncio as aioredis
        self._client = aioredis.from_url(url, decode_responses=True)

    async def check(self, key: str) -> bool:
        now = int(time.time())
        window_start = now - _RATE_WINDOW
        pipe = self._client.pipeline()
        pipe.zremrangebyscore(f"ratelimit:{key}", "-inf", window_start)
        pipe.zcard(f"ratelimit:{key}")
        result = await pipe.execute()
        count = result[1]
        if count >= _RATE_LIMIT:
            return False
        pipe2 = self._client.pipeline()
        pipe2.zadd(f"ratelimit:{key}", {str(now): now})
        pipe2.expire(f"ratelimit:{key}", _RATE_WINDOW * 2)
        await pipe2.execute()
        return True

    async def close(self):
        await self._client.aclose()


def _create_limiter() -> RateLimiter:
    if REDIS_URL:
        try:
            limiter = RedisRateLimiter(REDIS_URL)
            logger.info("Rate limiter: Redis")
            return limiter
        except Exception as e:
            logger.warning("Redis unavailable for rate limiting (%s), falling back to in-memory", e)
    logger.info("Rate limiter: in-memory")
    return InMemoryRateLimiter()


_limiter: RateLimiter = _create_limiter()


async def check_rate_limit(key: str) -> bool:
    return await _limiter.check(key)


async def close_limiter():
    await _limiter.close()
