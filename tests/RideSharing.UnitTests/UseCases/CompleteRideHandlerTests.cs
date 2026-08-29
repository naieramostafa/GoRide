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

public class CompleteRideHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
    private readonly CompleteRideHandler _handler;

    public CompleteRideHandlerTests()
    {
        _uowMock.Setup(x => x.Payments).Returns(_paymentRepoMock.Object);
        _handler = new CompleteRideHandler(_uowMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRide_ShouldComplete()
    {
        var ride = CreateRideInProgress(out var driver);
        _uowMock.Setup(x => x.Rides.GetByIdAsync(ride.Id)).ReturnsAsync(ride);
        _uowMock.Setup(x => x.Drivers.GetByUserIdAsync(driver.UserId)).ReturnsAsync(driver);
        _mapperMock.Setup(x => x.Map<RideResponseDto>(ride))
            .Returns(new RideResponseDto { Id = ride.Id, Status = RideStatus.Completed });

        var cmd = new CompleteRideCommand(new RideCompleteDto
        {
            RideId = ride.Id,
            DriverId = driver.UserId,
            DistanceKm = 10.5,
            DurationMinutes = 25,
            FinalFare = 35.0m
        });
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(RideStatus.Completed);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRide_ShouldThrowKeyNotFound()
    {
        _uowMock.Setup(x => x.Rides.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Ride?)null);

        var cmd = new CompleteRideCommand(new RideCompleteDto { RideId = Guid.NewGuid(), DriverId = Guid.NewGuid() });

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    private static Ride CreateRideInProgress(out Driver driver)
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        var driverUser = new User("Jane", "Doe", "jane@test.com", "456", UserRole.Driver, "pass");
        var passenger = new Passenger(user);
        var vehicle = new Vehicle("Toyota", "Camry", "2023", "White", "ABC123", VehicleType.Sedan, 4);
        driver = new Driver(driverUser, "LIC123", vehicle);
        var pickup = new Location(40.7128, -74.0060);
        var dropoff = new Location(40.7580, -73.9855);
        var ride = new Ride(passenger, pickup, dropoff, new Money(25.50m));
        ride.AssignDriver(driver);
        ride.StartRide();
        return ride;
    }
}
