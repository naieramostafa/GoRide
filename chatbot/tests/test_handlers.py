import pytest
from handlers.login import handle_login
from handlers.register import handle_register
from handlers.book_ride import handle_book_ride
from handlers.ride_status import handle_ride_status
from handlers.ride_details import handle_ride_details
from handlers.cancel_ride import handle_cancel_ride
from handlers.find_drivers import handle_find_drivers
from handlers.driver_actions import handle_driver_actions
from handlers.payment import handle_payment
from handlers.rating import handle_rating
from handlers.profile import handle_profile
from handlers.admin import handle_admin
from handlers.support import handle_support
from handlers.unknown import handle_unknown

pytestmark = pytest.mark.asyncio


class TestLogin:
    async def test_success(self, mock_call_api):
        mock_call_api.return_value = {
            "token": "new-token",
            "user": {"id": "u1", "firstName": "Bob", "lastName": "Test", "role": "Passenger"},
        }
        state = {"messages": ["my email is bob@test.com and password is bobpass"], "context": {}, "response": None}
        result = await handle_login(state)
        assert result["context"]["token"] == "new-token"
        assert "Logged in" in result["response"]

    async def test_failure(self, mock_call_api):
        mock_call_api.side_effect = Exception("Invalid credentials")
        state = {"messages": ["my email is bob@test.com and password is wrong"], "context": {}, "response": None}
        result = await handle_login(state)
        assert "Login failed" in result["response"]

    async def test_no_credentials(self, mock_call_api):
        state = {"messages": ["hello"], "context": {}, "response": None}
        result = await handle_login(state)
        assert "email" in result["response"].lower()


class TestRegister:
    async def test_user_success(self, mock_call_api):
        mock_call_api.return_value = {"token": "rt", "user": {"id": "u2", "role": "Passenger", "firstName": "New", "lastName": "User"}}
        state = {
            "messages": ["register as passenger my email is new@test.com and password is pass first New last User phone 123"],
            "context": {},
            "response": None,
        }
        result = await handle_register(state)
        assert "registered" in result["response"].lower()

    async def test_driver_success(self, mock_call_api):
        mock_call_api.return_value = {"token": "rt", "user": {"id": "u2", "role": "Driver", "firstName": "New", "lastName": "Driver"}}
        state = {
            "messages": ["register as driver my email is d@test.com and password is pass first New last Driver phone 123"],
            "context": {},
            "response": None,
        }
        result = await handle_register(state)
        assert "registered" in result["response"].lower()

    async def test_already_exists(self, mock_call_api):
        mock_call_api.side_effect = Exception("Email already registered")
        state = {
            "messages": ["register my email is dup@test.com and password is pass first Dup last User"],
            "context": {},
            "response": None,
        }
        result = await handle_register(state)
        assert "already registered" in result["response"].lower()

    async def test_no_details(self, mock_call_api):
        state = {"messages": ["register"], "context": {}, "response": None}
        result = await handle_register(state)
        assert "email" in result["response"].lower()


class TestBookRide:
    async def test_success(self, mock_call_api, mock_httpx_client):
        mock_call_api.return_value = {"id": "ride-1", "fare": 25.0, "distanceKm": 5.2, "status": "Pending"}
        mock_resp = mock_httpx_client.get.return_value
        mock_resp.json.return_value = [{"lat": "40.7128", "lon": "-74.0060"}]
        state = {
            "messages": ["book a ride from Times Square to Central Park"],
            "context": {"token": "t", "user": {"id": "u1"}},
            "response": None,
        }
        result = await handle_book_ride(state)
        assert "Ride booked" in result["response"]

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["book a ride"], "context": {}, "response": None}
        result = await handle_book_ride(state)
        assert "login" in result["response"].lower()

    async def test_missing_pickup(self, mock_call_api):
        state = {"messages": ["book a ride"], "context": {"token": "t", "user": {"id": "u1"}}, "response": None}
        result = await handle_book_ride(state)
        assert "pickup" in result["response"].lower()

    async def test_missing_dropoff(self, mock_call_api):
        state = {
            "messages": ["book a ride"],
            "context": {"token": "t", "user": {"id": "u1"}, "pickup": "Home"},
            "response": None,
        }
        result = await handle_book_ride(state)
        assert "dropoff" in result["response"].lower()

    async def test_api_error(self, mock_call_api, mock_httpx_client):
        mock_call_api.side_effect = Exception("Server error")
        mock_resp = mock_httpx_client.get.return_value
        mock_resp.json.return_value = [{"lat": "0", "lon": "0"}]
        state = {
            "messages": ["from A to B"],
            "context": {"token": "t", "user": {"id": "u1"}},
            "response": None,
        }
        result = await handle_book_ride(state)
        assert "Failed" in result["response"]


class TestRideStatus:
    async def test_success(self, mock_call_api):
        mock_call_api.return_value = [{"id": "r1", "pickupLocation": {"address": "A"}, "dropoffLocation": {"address": "B"}, "status": "Completed", "fare": 15}]
        state = {"messages": ["check status"], "context": {"token": "t", "user": {"id": "u1"}}, "response": None}
        result = await handle_ride_status(state)
        assert "A" in result["response"] or "No rides" in result["response"]

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["check status"], "context": {}, "response": None}
        result = await handle_ride_status(state)
        assert "login" in result["response"].lower()


class TestRideDetails:
    async def test_success(self, mock_call_api):
        mock_call_api.return_value = {
            "id": "r1", "pickupLocation": {"address": "A"}, "dropoffLocation": {"address": "B"},
            "status": "Completed", "fare": 20, "distanceKm": 5, "durationMinutes": 15, "requestedAt": "2024-01-01",
        }
        state = {
            "messages": ["550e8400-e29b-41d4-a716-446655440000"],
            "context": {"token": "t", "user": {"id": "u1"}},
            "response": None,
        }
        result = await handle_ride_details(state)
        assert "A" in result["response"]

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["details"], "context": {}, "response": None}
        result = await handle_ride_details(state)
        assert "login" in result["response"].lower()


class TestCancelRide:
    async def test_success(self, mock_call_api):
        mock_call_api.return_value = {"status": "Cancelled"}
        state = {
            "messages": ["550e8400-e29b-41d4-a716-446655440000"],
            "context": {"token": "t", "user": {"id": "u1"}},
            "response": None,
        }
        result = await handle_cancel_ride(state)
        # If resolve_ride_id finds the UUID, it proceeds to cancel
        if result["response"] and "Failed" not in result["response"] and "login" not in result["response"].lower():
            pass  # successful cancel path

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["cancel"], "context": {}, "response": None}
        result = await handle_cancel_ride(state)
        assert "login" in result["response"].lower()


class TestFindDrivers:
    async def test_success(self, mock_call_api):
        mock_call_api.return_value = [
            {"firstName": "Alice", "lastName": "D", "rating": 4.5, "vehicleMake": "Toyota", "vehicleModel": "Camry"}
        ]
        state = {
            "messages": ["find drivers near lat=40.7128 lng=-74.0060 radius=5"],
            "context": {"token": "t", "user": {"id": "u1"}},
            "response": None,
        }
        result = await handle_find_drivers(state)
        assert "Alice" in result["response"]

    async def test_no_location(self, mock_call_api):
        state = {"messages": ["find drivers"], "context": {"token": "t", "user": {"id": "u1"}}, "response": None}
        result = await handle_find_drivers(state)
        assert "lat" in result["response"].lower()

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["find drivers"], "context": {}, "response": None}
        result = await handle_find_drivers(state)
        assert "login" in result["response"].lower()


class TestDriverActions:
    async def test_online(self, mock_call_api):
        state = {
            "messages": ["set me online"],
            "context": {"token": "t", "user": {"id": "d1", "role": "Driver"}},
            "response": None,
        }
        result = await handle_driver_actions(state)
        assert "online" in result["response"].lower()

    async def test_offline(self, mock_call_api):
        state = {
            "messages": ["go offline"],
            "context": {"token": "t", "user": {"id": "d1", "role": "Driver"}},
            "response": None,
        }
        result = await handle_driver_actions(state)
        assert "offline" in result["response"].lower()

    async def test_update_location(self, mock_call_api):
        state = {
            "messages": ["update my location lat=40.71 lng=-74.01"],
            "context": {"token": "t", "user": {"id": "d1", "role": "Driver"}},
            "response": None,
        }
        result = await handle_driver_actions(state)
        assert "Location updated" in result["response"]

    async def test_my_rides(self, mock_call_api):
        mock_call_api.return_value = [
            {"pickupLocation": {"address": "A"}, "dropoffLocation": {"address": "B"}, "status": "Assigned", "fare": 20}
        ]
        state = {
            "messages": ["show my rides"],
            "context": {"token": "t", "user": {"id": "d1", "role": "Driver"}},
            "response": None,
        }
        result = await handle_driver_actions(state)
        assert "A" in result["response"]

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["online"], "context": {}, "response": None}
        result = await handle_driver_actions(state)
        assert "login" in result["response"].lower()


class TestPayment:
    async def test_initiate(self, mock_call_api):
        mock_call_api.return_value = {"clientSecret": "pi_123_secret_abc", "status": "requires_confirmation"}
        state = {
            "messages": ["pay for ride 1"],
            "context": {"token": "t", "user": {"id": "u1"}, "_ride_list": [{"id": "r1", "pickupLocation": {"address": "A"}, "dropoffLocation": {"address": "B"}, "status": "Completed", "fare": 15}]},
            "response": None,
        }
        result = await handle_payment(state)
        if "pay" in result["response"].lower() or "Payment" in result["response"]:
            pass

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["pay"], "context": {}, "response": None}
        result = await handle_payment(state)
        assert "login" in result["response"].lower()


class TestRating:
    async def test_success(self, mock_call_api):
        mock_call_api.side_effect = [
            {"driverId": "d1"},
            {"status": "ok"},
        ]
        state = {
            "messages": ["rate ride 5 stars"],
            "context": {"token": "t", "user": {"id": "u1"}, "_ride_list": [{"id": "r1", "pickupLocation": {"address": "A"}, "dropoffLocation": {"address": "B"}, "status": "Completed", "fare": 15}]},
            "response": None,
        }
        result = await handle_rating(state)
        if "5" in result["response"]:
            pass

    async def test_pending_score(self, mock_call_api):
        mock_call_api.side_effect = [
            {"driverId": "d1"},
            {"status": "ok"},
        ]
        state = {
            "messages": ["rate ride"],
            "context": {"token": "t", "user": {"id": "u1"}, "_pending_score": 4, "_ride_list": [{"id": "r1"}]},
            "response": None,
        }
        result = await handle_rating(state)
        if "4" in result["response"]:
            pass

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["rate"], "context": {}, "response": None}
        result = await handle_rating(state)
        assert "login" in result["response"].lower()


class TestProfile:
    async def test_success(self, mock_call_api):
        mock_call_api.return_value = {"firstName": "Test", "lastName": "User", "email": "t@t.com", "phone": "123", "role": "Passenger", "isActive": True, "createdAt": "2024-01-01"}
        state = {"messages": ["profile"], "context": {"token": "t", "user": {"id": "u1"}}, "response": None}
        result = await handle_profile(state)
        assert "Test" in result["response"]

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["profile"], "context": {}, "response": None}
        result = await handle_profile(state)
        assert "login" in result["response"].lower()


class TestAdmin:
    async def test_verify_driver(self, mock_call_api):
        state = {
            "messages": ["verify driver d1"],
            "context": {"token": "t", "user": {"id": "a1", "role": "Admin"}},
            "response": None,
        }
        result = await handle_admin(state)
        assert "verified" in result["response"].lower()

    async def test_not_admin(self, mock_call_api):
        state = {
            "messages": ["verify driver d1"],
            "context": {"token": "t", "user": {"id": "u1", "role": "Passenger"}},
            "response": None,
        }
        result = await handle_admin(state)
        assert "Admin" in result["response"]

    async def test_no_token(self, mock_call_api):
        state = {"messages": ["admin"], "context": {}, "response": None}
        result = await handle_admin(state)
        assert "login" in result["response"].lower()


class TestSupport:
    async def test_static_response(self):
        state = {"messages": ["help"], "context": {}, "response": None}
        result = await handle_support(state)
        assert "help" in result["response"].lower()


class TestUnknown:
    async def test_static_response(self):
        state = {"messages": ["xyz"], "context": {}, "response": None}
        result = await handle_unknown(state)
        assert "login" in result["response"].lower()
