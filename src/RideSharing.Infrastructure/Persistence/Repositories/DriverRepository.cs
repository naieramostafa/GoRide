using Microsoft.EntityFrameworkCore;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;
using RideSharing.Core.ValueObjects;

namespace RideSharing.Infrastructure.Persistence.Repositories;

public class DriverRepository : BaseRepository<Driver>, IDriverRepository
{
    public DriverRepository(RideSharingDbContext context) : base(context) { }

    public async Task<Driver?> GetByIdAsync(Guid id) =>
        await DbSet.Include(d => d.User).Include(d => d.Vehicle).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Driver?> GetByUserIdAsync(Guid userId) =>
        await DbSet.Include(d => d.User).Include(d => d.Vehicle).FirstOrDefaultAsync(d => d.UserId == userId);

    public async Task<IEnumerable<Driver>> GetAvailableDriversAsync() =>
        await DbSet.Where(d => d.IsAvailable && d.IsVerified)
                   .Include(d => d.User).Include(d => d.Vehicle).ToListAsync();

    public async Task<IEnumerable<Driver>> GetNearbyDriversAsync(double lat, double lng, double radiusKm)
    {
        var origin = new Location(lat, lng);
        var drivers = await DbSet.Where(d => d.IsAvailable && d.IsVerified && d.CurrentLocation != null)
                                 .Include(d => d.User).Include(d => d.Vehicle)
                                 .ToListAsync();
        return drivers.Where(d => d.CurrentLocation!.DistanceTo(origin) <= radiusKm).ToList();
    }

    public async Task<Driver> AddAsync(Driver driver) { await DbSet.AddAsync(driver); return driver; }
    public Task UpdateAsync(Driver driver) => Task.CompletedTask;
}
