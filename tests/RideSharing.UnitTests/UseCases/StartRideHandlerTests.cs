using AutoMapper;
using FluentAssertions;
using Moq;
using RideSharing.Application.DTOs;
using RideSharing.Application.Interfaces;
using RideSharing.Application.UseCases.Rides.Commands;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;
using Xunit;

namespace RideSharing.UnitTests.UseCases;

public class StartRideHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly StartRideHandler _handler;

    public StartRideHandlerTests()
    {
        _handler = new StartRideHandler(_uowMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRide_ShouldStartRide()
    {
        var ride = CreateRideWithDriver(out var driver);
        _uowMock.Setup(x => x.Rides.GetByIdAsync(ride.Id)).ReturnsAsync(ride);
        _uowMock.Setup(x => x.Drivers.GetByUserIdAsync(driver.UserId)).ReturnsAsync(driver);
        _mapperMock.Setup(x => x.Map<RideResponseDto>(ride))
            .Returns(new RideResponseDto { Id = ride.Id, Status = RideStatus.InProgress });

        var cmd = new StartRideCommand(ride.Id, driver.UserId);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(RideStatus.InProgress);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRide_ShouldThrowKeyNotFound()
    {
        _uowMock.Setup(x => x.Rides.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Ride?)null);

        var cmd = new StartRideCommand(Guid.NewGuid(), Guid.NewGuid());

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WrongDriver_ShouldThrowRideStateException()
    {
        var ride = CreateRideWithDriver(out _);
        var otherDriverUser = new User("Other", "Driver", "other@test.com", "789", UserRole.Driver, "pass");
        var otherVehicle = new Vehicle("Honda", "Civic", 2023, "Blue", "XYZ789", VehicleType.Sedan, 4);
        var otherDriver = new Driver(otherDriverUser, "LIC999", otherVehicle);

        _uowMock.Setup(x => x.Rides.GetByIdAsync(ride.Id)).ReturnsAsync(ride);
        _uowMock.Setup(x => x.Drivers.GetByUserIdAsync(otherDriver.UserId)).ReturnsAsync(otherDriver);

        var cmd = new StartRideCommand(ride.Id, otherDriver.UserId);

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<RideSharing.Core.Exceptions.RideStateException>();
    }

    [Fact]
    public async Task Handle_RideNotAccepted_ShouldThrowRideStateException()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        var passenger = new Passenger(user);
        var pickup = new Location(40.7128, -74.0060);
        var dropoff = new Location(40.7580, -73.9855);
        var ride = new Ride(passenger, pickup, dropoff, new Money(25.50m));

        var driverUser = new User("Jane", "Doe", "jane@test.com", "456", UserRole.Driver, "pass");
        var vehicle = new Vehicle("Toyota", "Camry", 2023, "White", "ABC123", VehicleType.Sedan, 4);
        var driver = new Driver(driverUser, "LIC123", vehicle);

        _uowMock.Setup(x => x.Rides.GetByIdAsync(ride.Id)).ReturnsAsync(ride);
        _uowMock.Setup(x => x.Drivers.GetByUserIdAsync(driver.UserId)).ReturnsAsync(driver);

        var cmd = new StartRideCommand(ride.Id, driver.UserId);

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<RideSharing.Core.Exceptions.RideStateException>();
    }

    private static Ride CreateRideWithDriver(out Driver driver)
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        var passenger = new Passenger(user);
        var driverUser = new User("Jane", "Doe", "jane@test.com", "456", UserRole.Driver, "pass");
        var vehicle = new Vehicle("Toyota", "Camry", 2023, "White", "ABC123", VehicleType.Sedan, 4);
        driver = new Driver(driverUser, "LIC123", vehicle);
        var pickup = new Location(40.7128, -74.0060);
        var dropoff = new Location(40.7580, -73.9855);
        var ride = new Ride(passenger, pickup, dropoff, new Money(25.50m));
        ride.AssignDriver(driver);
        return ride;
    }
}
