from .admin import handle_admin
from .book_ride import handle_book_ride
from .cancel_ride import handle_cancel_ride
from .driver_actions import handle_driver_actions
from .find_drivers import handle_find_drivers
from .login import handle_login
from .payment import handle_payment
from .profile import handle_profile
from .rating import handle_rating
from .register import handle_register
from .ride_details import handle_ride_details
from .ride_status import handle_ride_status
from .support import handle_support
from .unknown import handle_unknown

__all__ = [
    "handle_admin",
    "handle_book_ride",
    "handle_cancel_ride",
    "handle_driver_actions",
    "handle_find_drivers",
    "handle_login",
    "handle_payment",
    "handle_profile",
    "handle_rating",
    "handle_register",
    "handle_ride_details",
    "handle_ride_status",
    "handle_support",
    "handle_unknown",
]
