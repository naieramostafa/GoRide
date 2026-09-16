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

public class CancelRideHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPassengerRepository> _passengerRepoMock = new();
    private readonly CancelRideHandler _handler;

    public CancelRideHandlerTests()
    {
        _uowMock.Setup(x => x.Passengers).Returns(_passengerRepoMock.Object);
        _handler = new CancelRideHandler(_uowMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRide_ShouldCancelAndReturnDto()
    {
        var ride = CreateTestRide(out var passenger);
        _uowMock.Setup(x => x.Rides.GetByIdAsync(ride.Id))
            .ReturnsAsync(ride);
        _passengerRepoMock.Setup(x => x.GetByUserIdAsync(passenger.UserId))
            .ReturnsAsync(passenger);
        _mapperMock.Setup(x => x.Map<RideResponseDto>(ride))
            .Returns(new RideResponseDto { Id = ride.Id, Status = RideStatus.Cancelled });

        var cmd = new CancelRideCommand(new RideCancelDto { RideId = ride.Id, Reason = "Changed my mind" }, passenger.UserId);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Id.Should().Be(ride.Id);
        result.Status.Should().Be(RideStatus.Cancelled);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRide_ShouldThrowKeyNotFound()
    {
        _uowMock.Setup(x => x.Rides.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Ride?)null);

        var cmd = new CancelRideCommand(new RideCancelDto { RideId = Guid.NewGuid(), Reason = "Test" }, Guid.NewGuid());

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    private static Ride CreateTestRide(out Passenger passenger)
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        passenger = new Passenger(user);
        var pickup = new Location(40.7128, -74.0060);
        var dropoff = new Location(40.7580, -73.9855);
        return new Ride(passenger, pickup, dropoff, new Money(25.50m));
    }
}
