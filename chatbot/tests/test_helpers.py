import pytest

from helpers import geocode_address, parse_vehicle_details, pick_ride, resolve_ride_id


class TestParseVehicleDetails:
    def test_full(self):
        msg = "my car is Toyota model Camry 2024 color White plate ABC1234 license X12345"
        result = parse_vehicle_details(msg, "u1")
        assert result == {
            "userId": "u1",
            "make": "Toyota",
            "model": "Camry",
            "year": "2024",
            "color": "White",
            "licensePlate": "ABC1234",
            "vehicleType": 0,
            "capacity": 4,
            "licenseNumber": "X12345",
        }

    def test_minimal(self):
        result = parse_vehicle_details("my car is Honda", "u1")
        assert result is not None
        assert result["make"] == "Honda"
        assert result["model"] == ""

    def test_no_match(self):
        assert parse_vehicle_details("hello world", "u1") is None

    def test_color_and_plate_only(self):
        result = parse_vehicle_details("car is Ford color Red plate XYZ789", "u1")
        assert result["make"] == "Ford"
        assert result["color"] == "Red"
        assert result["licensePlate"] == "XYZ789"


class TestResolveRideId:
    @pytest.mark.asyncio
    async def test_number_with_list(self):
        rides = [{"id": "r1"}, {"id": "r2"}]
        state = {"messages": ["2"], "context": {"_ride_list": rides}}
        assert await resolve_ride_id(state) == "r2"
        assert state["context"]["lastRideId"] == "r2"

    @pytest.mark.asyncio
    async def test_number_out_of_range(self):
        rides = [{"id": "r1"}]
        state = {"messages": ["5"], "context": {"_ride_list": rides}}
        assert await resolve_ride_id(state) is None
        assert "lastRideId" not in state["context"]

    @pytest.mark.asyncio
    async def test_last_keyword(self):
        state = {"messages": ["show me the last ride"], "context": {"lastRideId": "r-last"}}
        assert await resolve_ride_id(state) == "r-last"

    @pytest.mark.asyncio
    async def test_number_with_last_id(self):
        state = {"messages": ["3"], "context": {"lastRideId": "r3"}}
        assert await resolve_ride_id(state) == "r3"

    @pytest.mark.asyncio
    async def test_uuid(self):
        rid = "550e8400-e29b-41d4-a716-446655440000"
        state = {"messages": [rid], "context": {}}
        assert await resolve_ride_id(state) == rid

    @pytest.mark.asyncio
    async def test_ride_ref_with_list(self):
        rides = [{"id": "r10"}, {"id": "r20"}]
        state = {"messages": ["ride 2"], "context": {"_ride_list": rides}}
        assert await resolve_ride_id(state) == "r20"

    @pytest.mark.asyncio
    async def test_no_match(self):
        state = {"messages": ["what is the weather"], "context": {}}
        assert await resolve_ride_id(state) is None


class TestPickRide:
    @pytest.mark.asyncio
    async def test_no_token(self, mock_call_api):
        state = {"messages": ["test"], "context": {}, "response": None}
        result = await pick_ride(state, "check")
        assert result is None
        assert "login" in state["response"].lower()

    @pytest.mark.asyncio
    async def test_fetches_list(self, mock_call_api):
        state = {"messages": ["show rides"], "context": {"token": "t", "user": {"id": "u1"}}, "response": None}
        mock_call_api.return_value = [
            {"id": "r1", "pickupLocation": {"address": "A"}, "dropoffLocation": {"address": "B"}, "status": "pending", "fare": 10}
        ]
        result = await pick_ride(state, "check")
        assert result is None
        assert state["context"]["_ride_list"] is not None
        assert "Which ride" in state["response"]

    @pytest.mark.asyncio
    async def test_api_error(self, mock_call_api):
        state = {"messages": ["test"], "context": {"token": "t", "user": {"id": "u1"}}, "response": None}
        mock_call_api.side_effect = Exception("API down")
        result = await pick_ride(state, "check")
        assert result is None
        assert "error" in state["response"].lower()


class TestGeocodeAddress:
    @pytest.mark.asyncio
    async def test_success(self, mock_httpx_client):
        from unittest.mock import MagicMock
        resp = MagicMock(status_code=200)
        resp.json.return_value = [{"lat": "40.7128", "lon": "-74.0060"}]
        mock_httpx_client.get.return_value = resp
        result = await geocode_address("New York")
        assert result == (40.7128, -74.0060)

    @pytest.mark.asyncio
    async def test_no_results(self, mock_httpx_client):
        from unittest.mock import MagicMock
        resp = MagicMock(status_code=200)
        resp.json.return_value = []
        mock_httpx_client.get.return_value = resp
        result = await geocode_address("Atlantis")
        assert result is None

    @pytest.mark.asyncio
    async def test_http_error(self, mock_httpx_client):
        mock_httpx_client.get.side_effect = Exception("timeout")
        result = await geocode_address("Nowhere")
        assert result is None
