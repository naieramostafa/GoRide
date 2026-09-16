using FluentAssertions;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;
using Xunit;

namespace RideSharing.UnitTests.Domain;

public class DriverTests
{
    [Fact]
    public void CreateDriver_ShouldSetProperties()
    {
        var user = new User("Jane", "Doe", "jane@test.com", "456", UserRole.Driver, "pass");
        var vehicle = new Vehicle("Toyota", "Camry", 2023, "White", "ABC123", VehicleType.Sedan, 4);

        var driver = new Driver(user, "LIC123", vehicle);

        driver.UserId.Should().Be(user.Id);
        driver.LicenseNumber.Should().Be("LIC123");
        driver.Vehicle.Should().Be(vehicle);
        driver.IsAvailable.Should().BeTrue();
        driver.IsVerified.Should().BeFalse();
        driver.Rating.Should().Be(5.0);
        driver.TotalRides.Should().Be(0);
    }

    [Fact]
    public void SetAvailable_ShouldChangeAvailability()
    {
        var driver = CreateTestDriver();

        driver.SetAvailable(false);
        driver.IsAvailable.Should().BeFalse();

        driver.SetAvailable(true);
        driver.IsAvailable.Should().BeTrue();
    }

    [Fact]
    public void Verify_ShouldSetIsVerified()
    {
        var driver = CreateTestDriver();

        driver.Verify();

        driver.IsVerified.Should().BeTrue();
    }

    [Fact]
    public void UpdateLocation_ShouldSetLocation()
    {
        var driver = CreateTestDriver();
        var location = new Location(40.7128, -74.0060, "NYC");

        driver.UpdateLocation(location);

        driver.CurrentLocation.Should().Be(location);
    }

    [Fact]
    public void UpdateRating_ShouldCalculateAverage()
    {
        var driver = CreateTestDriver();

        driver.UpdateRating(4.0);

        driver.Rating.Should().Be(4.0);
        driver.TotalRides.Should().Be(1);

        driver.UpdateRating(5.0);

        driver.Rating.Should().BeApproximately(4.5, 0.01);
        driver.TotalRides.Should().Be(2);
    }

    private static Driver CreateTestDriver()
    {
        var user = new User("Jane", "Doe", "jane@test.com", "456", UserRole.Driver, "pass");
        var vehicle = new Vehicle("Toyota", "Camry", 2023, "White", "ABC123", VehicleType.Sedan, 4);
        return new Driver(user, "LIC123", vehicle);
    }
}
