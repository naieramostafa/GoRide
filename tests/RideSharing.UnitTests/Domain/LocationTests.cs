using FluentAssertions;
using RideSharing.Core.ValueObjects;
using Xunit;

namespace RideSharing.UnitTests.Domain;

public class LocationTests
{
    [Fact]
    public void CreateLocation_ShouldSetCoordinates()
    {
        var loc = new Location(40.7128, -74.0060, "New York");

        loc.Latitude.Should().Be(40.7128);
        loc.Longitude.Should().Be(-74.0060);
        loc.Address.Should().Be("New York");
    }

    [Fact]
    public void CreateLocation_WithoutAddress_ShouldBeNull()
    {
        var loc = new Location(40.7128, -74.0060);

        loc.Address.Should().BeNull();
    }

    [Fact]
    public void DistanceTo_ShouldCalculateHaversine()
    {
        var nyc = new Location(40.7128, -74.0060);
        var la = new Location(34.0522, -118.2437);

        var distance = nyc.DistanceTo(la);

        distance.Should().BeApproximately(3944, 10);
    }

    [Fact]
    public void DistanceTo_SamePoint_ShouldBeZero()
    {
        var loc1 = new Location(40.7128, -74.0060);
        var loc2 = new Location(40.7128, -74.0060);

        var distance = loc1.DistanceTo(loc2);

        distance.Should().Be(0);
    }

    [Fact]
    public void Equality_SameCoordinates_ShouldBeEqual()
    {
        var loc1 = new Location(40.7128, -74.0060);
        var loc2 = new Location(40.7128, -74.0060);

        (loc1 == loc2).Should().BeTrue();
        loc1.Equals(loc2).Should().BeTrue();
    }
}
