using FluentAssertions;
using RideSharing.Core.Entities;
using Xunit;

namespace RideSharing.UnitTests.Domain;

public class RefreshTokenTests
{
    [Fact]
    public void CreateRefreshToken_ShouldSetProperties()
    {
        var userId = Guid.NewGuid();
        var expiry = DateTime.UtcNow.AddDays(7);

        var token = new RefreshToken(userId, "test-refresh-token", expiry);

        token.UserId.Should().Be(userId);
        token.Token.Should().Be("test-refresh-token");
        token.ExpiresAt.Should().Be(expiry);
        token.IsRevoked.Should().BeFalse();
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Revoke_ShouldSetRevoked()
    {
        var token = CreateTestToken();

        token.Revoke();

        token.IsRevoked.Should().BeTrue();
        token.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenPastExpiry_ShouldBeTrue()
    {
        var token = new RefreshToken(Guid.NewGuid(), "token", DateTime.UtcNow.AddHours(-1));

        token.IsExpired.Should().BeTrue();
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenNotRevokedAndNotExpired_ShouldBeTrue()
    {
        var token = new RefreshToken(Guid.NewGuid(), "token", DateTime.UtcNow.AddDays(1));

        token.IsActive.Should().BeTrue();
    }

    private static RefreshToken CreateTestToken()
    {
        return new RefreshToken(Guid.NewGuid(), "test-token", DateTime.UtcNow.AddDays(7));
    }
}
