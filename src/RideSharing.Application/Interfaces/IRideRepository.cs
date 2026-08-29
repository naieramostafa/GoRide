using RideSharing.Core.Entities;
using RideSharing.Core.Enums;

namespace RideSharing.Application.Interfaces;

public interface IRideRepository
{
    Task<Ride?> GetByIdAsync(Guid id);
    Task<IEnumerable<Ride>> GetByPassengerAsync(Guid passengerId);
    Task<IEnumerable<Ride>> GetByDriverAsync(Guid driverId);
    Task<IEnumerable<Ride>> GetByStatusAsync(RideStatus status);
    Task<IEnumerable<Ride>> GetUnassignedRidesAsync();
    Task<IEnumerable<Ride>> GetActiveRidesAsync();
    Task<Ride> AddAsync(Ride ride);
    Task UpdateAsync(Ride ride);
}
