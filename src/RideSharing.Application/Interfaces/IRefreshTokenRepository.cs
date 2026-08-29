using RideSharing.Core.Entities;

namespace RideSharing.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task AddAsync(RefreshToken refreshToken);
    Task RevokeAllForUserAsync(Guid userId);
}
