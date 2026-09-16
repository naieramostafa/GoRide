using FluentAssertions;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using Xunit;

namespace RideSharing.UnitTests.Domain;

public class VehicleTests
{
    [Fact]
    public void CreateVehicle_ShouldSetProperties()
    {
        var vehicle = new Vehicle("Toyota", "Camry", 2023, "White", "ABC123", VehicleType.Sedan, 4);

        vehicle.Make.Should().Be("Toyota");
        vehicle.Model.Should().Be("Camry");
        vehicle.Year.Should().Be(2023);
        vehicle.Color.Should().Be("White");
        vehicle.LicensePlate.Should().Be("ABC123");
        vehicle.Type.Should().Be(VehicleType.Sedan);
        vehicle.Capacity.Should().Be(4);
    }
}
