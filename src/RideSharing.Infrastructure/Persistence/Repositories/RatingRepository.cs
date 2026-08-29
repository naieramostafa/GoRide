using Microsoft.EntityFrameworkCore;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;

namespace RideSharing.Infrastructure.Persistence.Repositories;

public class RatingRepository : BaseRepository<Rating>, IRatingRepository
{
    public RatingRepository(RideSharingDbContext context) : base(context) { }

    public async Task<Rating?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);

    public async Task<IEnumerable<Rating>> GetByUserAsync(Guid userId) =>
        await DbSet.Where(r => r.RatedUserId == userId || r.RatedByUserId == userId).ToListAsync();

    public async Task<IEnumerable<Rating>> GetByRideAsync(Guid rideId) =>
        await DbSet.Where(r => r.RideId == rideId).ToListAsync();

    public async Task<Rating> AddAsync(Rating rating) { await DbSet.AddAsync(rating); return rating; }
}
