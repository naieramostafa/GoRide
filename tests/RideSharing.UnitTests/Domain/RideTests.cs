using FluentAssertions;
using RideSharing.Core.Entities;
using Xunit;
using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;

namespace RideSharing.UnitTests.Domain;

public class RideTests
{
    private readonly Passenger _passenger;
    private readonly Driver _driver;

    public RideTests()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        var driverUser = new User("Jane", "Doe", "jane@test.com", "456", UserRole.Driver, "pass");
        _passenger = new Passenger(user);
        var vehicle = new Vehicle("Toyota", "Camry", "2023", "White", "ABC123", VehicleType.Sedan, 4);
        _driver = new Driver(driverUser, "LIC123", vehicle);
    }

    [Fact]
    public void CreateRide_ShouldSetPendingStatus()
    {
        var pickup = new Location(40.7128, -74.0060, "NYC");
        var dropoff = new Location(40.7580, -73.9855, "Times Square");
        var fare = new Money(25.50m);

        var ride = new Ride(_passenger, pickup, dropoff, fare);

        ride.Status.Should().Be(RideStatus.Pending);
        ride.Fare.Amount.Should().Be(25.50m);
    }

    [Fact]
    public void AssignDriver_ShouldSetDriverAccepted()
    {
        var ride = CreateTestRide();
        ride.AssignDriver(_driver);

        ride.Status.Should().Be(RideStatus.DriverAccepted);
        ride.DriverId.Should().Be(_driver.Id);
        ride.AcceptedAt.Should().NotBeNull();
    }

    [Fact]
    public void CompleteRide_ShouldSetCompletedStatus()
    {
        var ride = CreateTestRide();
        ride.AssignDriver(_driver);
        ride.StartRide();
        ride.CompleteRide(10.5, 25, new Money(35.0m));

        ride.Status.Should().Be(RideStatus.Completed);
        ride.DistanceKm.Should().Be(10.5);
        ride.DurationMinutes.Should().Be(25);
        ride.FinalFare!.Amount.Should().Be(35.0m);
    }

    [Fact]
    public void CancelRide_ShouldSetCancelled()
    {
        var ride = CreateTestRide();
        ride.Cancel("No longer needed");

        ride.Status.Should().Be(RideStatus.Cancelled);
        ride.CancellationReason.Should().Be("No longer needed");
    }

    private Ride CreateTestRide()
    {
        var pickup = new Location(40.7128, -74.0060);
        var dropoff = new Location(40.7580, -73.9855);
        return new Ride(_passenger, pickup, dropoff, new Money(25.50m));
    }
}
