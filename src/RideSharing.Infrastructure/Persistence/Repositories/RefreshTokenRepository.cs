using Microsoft.EntityFrameworkCore;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Entities;

namespace RideSharing.Infrastructure.Persistence.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly RideSharingDbContext _context;

    public RefreshTokenRepository(RideSharingDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task AddAsync(RefreshToken refreshToken)
    {
        await _context.Set<RefreshToken>().AddAsync(refreshToken);
    }

    public async Task RevokeAllForUserAsync(Guid userId)
    {
        var active = await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && !rt.IsRevoked)
            .ToListAsync();
        foreach (var token in active)
            token.Revoke();
    }
}
