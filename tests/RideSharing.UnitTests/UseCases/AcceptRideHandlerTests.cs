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

public class AcceptRideHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly AcceptRideHandler _handler;

    public AcceptRideHandlerTests()
    {
        _handler = new AcceptRideHandler(_uowMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRideAndDriver_ShouldAccept()
    {
        var driverUser = new User("Jane", "Doe", "jane@test.com", "456", UserRole.Driver, "pass");
        var vehicle = new Vehicle("Toyota", "Camry", "2023", "White", "ABC123", VehicleType.Sedan, 4);
        var driver = new Driver(driverUser, "LIC123", vehicle);
        var ride = CreateTestRide();

        _uowMock.Setup(x => x.Rides.GetByIdAsync(ride.Id)).ReturnsAsync(ride);
        _uowMock.Setup(x => x.Drivers.GetByUserIdAsync(driver.UserId)).ReturnsAsync(driver);
        _mapperMock.Setup(x => x.Map<RideResponseDto>(ride))
            .Returns(new RideResponseDto { Id = ride.Id, Status = RideStatus.DriverAccepted });

        var cmd = new AcceptRideCommand(new RideAcceptDto { RideId = ride.Id, DriverId = driver.UserId });
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(RideStatus.DriverAccepted);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRide_ShouldThrowKeyNotFound()
    {
        _uowMock.Setup(x => x.Rides.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Ride?)null);

        var cmd = new AcceptRideCommand(new RideAcceptDto { RideId = Guid.NewGuid(), DriverId = Guid.NewGuid() });

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
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
