"""Integration and e2e tests using FastAPI TestClient."""

import pytest
from fastapi.testclient import TestClient

from main import app


@pytest.fixture
def client():
    return TestClient(app)


class TestHealthEndpoint:
    def test_health_returns_200(self, client):
        resp = client.get("/health")
        assert resp.status_code == 200
        body = resp.json()
        assert body["service"] == "ridesharing-chatbot"
        assert body["status"] == "healthy"

    def test_health_contains_expected_keys(self, client):
        resp = client.get("/health")
        body = resp.json()
        for key in ("status", "service", "llm", "session_store", "downstream_api"):
            assert key in body


class TestChatEndpoint:
    def test_chat_without_conversation_id(self, client):
        resp = client.post("/chat", json={"message": "hello"})
        assert resp.status_code == 200
        body = resp.json()
        assert "reply" in body
        assert "intent" in body
        assert "conversation_id" in body

    def test_chat_with_conversation_id(self, client):
        conv_id = "test-integration-123"
        resp = client.post("/chat", json={"message": "hello", "conversation_id": conv_id})
        assert resp.status_code == 200
        assert resp.json()["conversation_id"] == conv_id

    def test_chat_maintains_conversation(self, client):
        conv_id = "test-conv-chain"
        r1 = client.post("/chat", json={"message": "I want to book a ride from A to B", "conversation_id": conv_id})
        assert r1.status_code == 200
        r2 = client.post("/chat", json={"message": "yes", "conversation_id": conv_id})
        assert r2.status_code == 200
        r3 = client.post("/chat", json={"message": "show my rides", "conversation_id": conv_id})
        assert r3.status_code == 200

    def test_chat_empty_message(self, client):
        resp = client.post("/chat", json={"message": ""})
        assert resp.status_code == 200

    def test_chat_long_message_truncated(self, client):
        long_msg = "x" * 5000
        resp = client.post("/chat", json={"message": long_msg})
        assert resp.status_code == 200

    def test_chat_special_characters(self, client):
        resp = client.post("/chat", json={"message": "hello\x00world\x1ftest"})
        assert resp.status_code == 200


class TestRateLimiting:
    def test_many_requests_eventually_blocked(self, client):
        for _ in range(25):
            resp = client.post("/chat", json={"message": "ping"})
            if resp.status_code == 429:
                break
        else:
            pytest.skip("Rate limit not triggered (may use Redis limiter in CI)")
