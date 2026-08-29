using Microsoft.EntityFrameworkCore;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;

namespace RideSharing.Infrastructure.Persistence.Repositories;

public class PassengerRepository : BaseRepository<Passenger>, IPassengerRepository
{
    public PassengerRepository(RideSharingDbContext context) : base(context) { }

    public async Task<Passenger?> GetByIdAsync(Guid id) =>
        await DbSet.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Passenger?> GetByUserIdAsync(Guid userId) =>
        await DbSet.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task<Passenger> AddAsync(Passenger passenger) { await DbSet.AddAsync(passenger); return passenger; }
    public Task UpdateAsync(Passenger passenger) => Task.CompletedTask;
}
