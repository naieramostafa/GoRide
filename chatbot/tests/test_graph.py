import pytest

from graph import _classify_by_rules, _safe_handler, _safe_route, build_graph
from state import ChatState


class TestClassifyByRules:
    def test_book_ride(self):
        assert _classify_by_rules("book a ride from A to B") == "book_ride"

    def test_cancel_ride(self):
        assert _classify_by_rules("cancel my ride") == "cancel_ride"

    def test_ride_status(self):
        assert _classify_by_rules("what is my ride status") == "ride_status"

    def test_my_rides(self):
        assert _classify_by_rules("my rides") == "ride_status"

    def test_ride_details(self):
        assert _classify_by_rules("details of ride") == "ride_details"

    def test_find_drivers(self):
        assert _classify_by_rules("find nearby drivers") == "find_drivers"

    def test_driver_online(self):
        assert _classify_by_rules("go online") == "driver_actions"

    def test_driver_offline(self):
        assert _classify_by_rules("set me offline") == "driver_actions"

    def test_start_ride(self):
        assert _classify_by_rules("start ride") == "driver_actions"

    def test_payment(self):
        assert _classify_by_rules("pay for my ride") == "payment"

    def test_rating(self):
        assert _classify_by_rules("rate the ride 5 stars") == "rating"

    def test_profile(self):
        assert _classify_by_rules("show my profile") == "profile"

    def test_admin(self):
        assert _classify_by_rules("admin verify driver abc") == "admin"

    def test_verify_driver(self):
        assert _classify_by_rules("verify driver abc-123") == "admin"

    def test_support(self):
        assert _classify_by_rules("help") == "support"

    def test_register(self):
        assert _classify_by_rules("register me as driver") == "register"

    def test_login(self):
        assert _classify_by_rules("my email is a@b.com and password is p") == "login"

    def test_unknown(self):
        assert _classify_by_rules("xyzzy plugh") is None


class TestSafeRoute:
    def test_returns_intent(self):
        assert _safe_route({"intent": "book_ride"}) == "book_ride"

    def test_unknown_when_missing(self):
        assert _safe_route({}) == "unknown"


class TestSafeHandler:
    @pytest.mark.asyncio
    async def test_normal_execution(self):
        async def good_handler(state):
            state["response"] = "ok"
            return state
        wrapped = _safe_handler(good_handler)
        result = await wrapped(ChatState(messages=["hi"], intent=None, response=None, context={}))
        assert result["response"] == "ok"

    @pytest.mark.asyncio
    async def test_crashed_execution(self):
        async def bad_handler(state):
            raise ValueError("boom")
        wrapped = _safe_handler(bad_handler)
        result = await wrapped(ChatState(messages=["hi"], intent=None, response=None, context={}))
        assert "something went wrong" in result.get("response", "").lower()


class TestBuildGraph:
    def test_graph_is_compiled(self):
        graph = build_graph()
        assert graph is not None
        assert hasattr(graph, "ainvoke")
