using RideSharing.Core.Entities;

namespace RideSharing.Application.Interfaces;

public interface IRatingRepository
{
    Task<Rating?> GetByIdAsync(Guid id);
    Task<IEnumerable<Rating>> GetByUserAsync(Guid userId);
    Task<IEnumerable<Rating>> GetByRideAsync(Guid rideId);
    Task<Rating> AddAsync(Rating rating);
}
