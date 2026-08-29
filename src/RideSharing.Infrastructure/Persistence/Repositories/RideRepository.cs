using Microsoft.EntityFrameworkCore;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;

namespace RideSharing.Infrastructure.Persistence.Repositories;

public class RideRepository : BaseRepository<Ride>, IRideRepository
{
    public RideRepository(RideSharingDbContext context) : base(context) { }

    public async Task<Ride?> GetByIdAsync(Guid id) =>
        await DbSet.Include(r => r.Passenger).ThenInclude(p => p!.User)
                   .Include(r => r.Driver).ThenInclude(d => d!.User)
                   .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Ride>> GetByPassengerAsync(Guid passengerId) =>
        await DbSet.Where(r => r.PassengerId == passengerId)
                   .Include(r => r.Passenger).ThenInclude(p => p!.User)
                   .Include(r => r.Driver).ThenInclude(d => d!.User)
                   .ToListAsync();

    public async Task<IEnumerable<Ride>> GetByDriverAsync(Guid driverId) =>
        await DbSet.Where(r => r.DriverId == driverId)
                   .Include(r => r.Driver).ThenInclude(d => d!.User)
                   .Include(r => r.Passenger).ThenInclude(p => p!.User)
                   .ToListAsync();

    public async Task<IEnumerable<Ride>> GetByStatusAsync(RideStatus status) =>
        await DbSet.Where(r => r.Status == status)
                   .Include(r => r.Passenger).ThenInclude(p => p!.User)
                   .Include(r => r.Driver).ThenInclude(d => d!.User)
                   .ToListAsync();

    public async Task<IEnumerable<Ride>> GetUnassignedRidesAsync() =>
        await DbSet.Where(r => r.Status == RideStatus.Pending || r.Status == RideStatus.Searching)
                   .OrderByDescending(r => r.RequestedAt)
                   .Include(r => r.Passenger).ThenInclude(p => p!.User)
                   .Include(r => r.Driver).ThenInclude(d => d!.User)
                   .ToListAsync();

    public async Task<IEnumerable<Ride>> GetActiveRidesAsync() =>
        await DbSet.Where(r => r.Status == RideStatus.InProgress || r.Status == RideStatus.DriverArriving)
                   .Include(r => r.Passenger).ThenInclude(p => p!.User)
                   .Include(r => r.Driver).ThenInclude(d => d!.User)
                   .ToListAsync();

    public async Task<Ride> AddAsync(Ride ride)
    {
        await DbSet.AddAsync(ride);
        return ride;
    }

    public Task UpdateAsync(Ride ride) => Task.CompletedTask;
}
