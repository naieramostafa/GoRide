import httpx

from config import _API_TIMEOUT_SEC, RIDE_API_URL


async def call_api(endpoint: str, method: str = "GET", data: dict | None = None, token: str | None = None):
    url = f"{RIDE_API_URL}{endpoint}"
    headers = {}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    async with httpx.AsyncClient(timeout=_API_TIMEOUT_SEC) as client:
        if method == "GET":
            resp = await client.get(url, headers=headers)
        elif method == "POST":
            resp = await client.post(url, json=data, headers=headers)
        elif method == "PUT":
            resp = await client.put(url, json=data, headers=headers)
        else:
            raise ValueError(f"Unsupported method: {method}")
        if resp.status_code == 401:
            raise Exception("TOKEN_EXPIRED: Your session has expired. Please login again.")
        if resp.status_code >= 400:
            detail = resp.text[:200]
            raise Exception(f"API error ({resp.status_code}): {detail}")
        return resp.json() if resp.content else None
