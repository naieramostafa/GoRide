using RideSharing.Core.Entities;

namespace RideSharing.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    int RefreshExpiryDays { get; }
}
