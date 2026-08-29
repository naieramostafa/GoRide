using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RideSharing.Application.DTOs;

namespace RideSharing.Api.Hubs;

[Authorize]
public class RideTrackingHub : Hub
{
    public async Task JoinRideGroup(string rideId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, rideId);
    }

    public async Task LeaveRideGroup(string rideId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, rideId);
    }

    public async Task UpdateLocation(string rideId, double latitude, double longitude)
    {
        await Clients.Group(rideId).SendAsync("LocationUpdated", new LocationUpdateDto
        {
            RideId = rideId,
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = DateTime.UtcNow
        });
    }
}
