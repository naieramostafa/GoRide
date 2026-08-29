using RideSharing.Core.Entities;

namespace RideSharing.Application.Interfaces;

public interface IDriverRepository
{
    Task<Driver?> GetByIdAsync(Guid id);
    Task<Driver?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Driver>> GetAvailableDriversAsync();
    Task<IEnumerable<Driver>> GetNearbyDriversAsync(double latitude, double longitude, double radiusKm);
    Task<Driver> AddAsync(Driver driver);
    Task UpdateAsync(Driver driver);
}
