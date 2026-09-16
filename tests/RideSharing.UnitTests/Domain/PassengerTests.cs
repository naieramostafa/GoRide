using FluentAssertions;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using Xunit;

namespace RideSharing.UnitTests.Domain;

public class PassengerTests
{
    [Fact]
    public void CreatePassenger_ShouldSetProperties()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");

        var passenger = new Passenger(user);

        passenger.UserId.Should().Be(user.Id);
        passenger.User.Should().Be(user);
        passenger.Rating.Should().Be(5.0);
        passenger.TotalRides.Should().Be(0);
    }

    [Fact]
    public void UpdateRating_ShouldCalculateAverage()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        var passenger = new Passenger(user);

        passenger.UpdateRating(4.0);

        passenger.Rating.Should().Be(4.0);
        passenger.TotalRides.Should().Be(1);

        passenger.UpdateRating(5.0);

        passenger.Rating.Should().BeApproximately(4.5, 0.01);
        passenger.TotalRides.Should().Be(2);
    }
}
