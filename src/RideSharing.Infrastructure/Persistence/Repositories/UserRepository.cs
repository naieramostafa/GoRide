using Microsoft.EntityFrameworkCore;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;

namespace RideSharing.Infrastructure.Persistence.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(RideSharingDbContext context) : base(context) { }

    public async Task<User?> GetByIdAsync(Guid id) => await DbSet.FindAsync(id);
    public async Task<User?> GetByEmailAsync(string email) =>
        await DbSet.FirstOrDefaultAsync(u => u.Email == email);
    public async Task<User> AddAsync(User user) { await DbSet.AddAsync(user); return user; }
    public Task UpdateAsync(User user) => Task.CompletedTask;
}
