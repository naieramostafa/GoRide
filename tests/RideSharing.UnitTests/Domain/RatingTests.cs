using FluentAssertions;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;
using Xunit;

namespace RideSharing.UnitTests.Domain;

public class RatingTests
{
    [Fact]
    public void CreateRating_ShouldSetProperties()
    {
        var ride = CreateTestRide();
        var ratedBy = Guid.NewGuid();
        var ratedUser = Guid.NewGuid();

        var rating = new Rating(ride, ratedBy, ratedUser, 5, "Great ride!");

        rating.RideId.Should().Be(ride.Id);
        rating.RatedByUserId.Should().Be(ratedBy);
        rating.RatedUserId.Should().Be(ratedUser);
        rating.Score.Should().Be(5);
        rating.Comment.Should().Be("Great ride!");
        rating.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void CreateRating_WithoutComment_ShouldAllowNull()
    {
        var ride = CreateTestRide();

        var rating = new Rating(ride, Guid.NewGuid(), Guid.NewGuid(), 4);

        rating.Comment.Should().BeNull();
    }

    [Fact]
    public void CreateRating_ScoreBelow1_ShouldThrow()
    {
        var ride = CreateTestRide();

        Action act = () => new Rating(ride, Guid.NewGuid(), Guid.NewGuid(), 0);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateRating_ScoreAbove5_ShouldThrow()
    {
        var ride = CreateTestRide();

        Action act = () => new Rating(ride, Guid.NewGuid(), Guid.NewGuid(), 6);

        act.Should().Throw<ArgumentException>();
    }

    private static Ride CreateTestRide()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        var passenger = new Passenger(user);
        var pickup = new Location(40.7128, -74.0060);
        var dropoff = new Location(40.7580, -73.9855);
        return new Ride(passenger, pickup, dropoff, new Money(25.50m));
    }
}
