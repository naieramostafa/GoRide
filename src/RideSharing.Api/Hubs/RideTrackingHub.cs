using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RideSharing.Application.DTOs;
using RideSharing.Infrastructure.Persistence;

namespace RideSharing.Api.Hubs;

[Authorize]
public class RideTrackingHub : Hub
{
    private readonly IServiceScopeFactory _scopeFactory;

    public RideTrackingHub(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private async Task<bool> IsRideParticipant(string rideId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return false;

        if (!Guid.TryParse(rideId, out var rideIdGuid)) return false;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RideSharingDbContext>();

        var ride = await db.Rides.FindAsync(rideIdGuid);
        if (ride == null) return false;

        var userGuid = Guid.Parse(userId);
        return ride.PassengerId == userGuid ||
               (ride.DriverId.HasValue && ride.DriverId == userGuid);
    }

    public async Task JoinRideGroup(string rideId)
    {
        if (!await IsRideParticipant(rideId))
            throw new HubException("You are not a participant of this ride.");
        await Groups.AddToGroupAsync(Context.ConnectionId, rideId);
    }

    public async Task LeaveRideGroup(string rideId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, rideId);
    }

    public async Task UpdateLocation(string rideId, double latitude, double longitude)
    {
        if (!await IsRideParticipant(rideId))
            throw new HubException("You are not a participant of this ride.");
        await Clients.Group(rideId).SendAsync("LocationUpdated", new LocationUpdateDto
        {
            RideId = rideId,
            Latitude = latitude,
            Longitude = longitude,
            Timestamp = DateTime.UtcNow
        });
    }
}
