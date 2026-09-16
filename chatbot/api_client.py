import httpx

from config import _API_TIMEOUT_SEC, RIDE_API_URL

_client: httpx.AsyncClient | None = None


async def _get_client() -> httpx.AsyncClient:
    global _client
    if _client is None or _client.is_closed:
        _client = httpx.AsyncClient(timeout=_API_TIMEOUT_SEC)
    return _client


async def call_api(endpoint: str, method: str = "GET", data: dict | None = None, token: str | None = None):
    url = f"{RIDE_API_URL}{endpoint}"
    headers = {}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    client = await _get_client()
    if method == "GET":
        resp = await client.get(url, headers=headers)
    elif method == "POST":
        resp = await client.post(url, json=data, headers=headers)
    elif method == "PUT":
        resp = await client.put(url, json=data, headers=headers)
    else:
        raise ValueError(f"Unsupported method: {method}")
    if resp.status_code == 401:
        raise Exception("Your session has expired. Please login again.")
    if resp.status_code >= 400:
        raise Exception(f"Request failed (HTTP {resp.status_code})")
    return resp.json() if resp.content else None
