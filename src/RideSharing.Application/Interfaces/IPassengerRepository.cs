using RideSharing.Core.Entities;

namespace RideSharing.Application.Interfaces;

public interface IPassengerRepository
{
    Task<Passenger?> GetByIdAsync(Guid id);
    Task<Passenger?> GetByUserIdAsync(Guid userId);
    Task<Passenger> AddAsync(Passenger passenger);
    Task UpdateAsync(Passenger passenger);
}
