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
    private readonly CancelRideHandler _handler;

    public CancelRideHandlerTests()
    {
        _handler = new CancelRideHandler(_uowMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRide_ShouldCancelAndReturnDto()
    {
        var ride = CreateTestRide();
        _uowMock.Setup(x => x.Rides.GetByIdAsync(ride.Id))
            .ReturnsAsync(ride);
        _mapperMock.Setup(x => x.Map<RideResponseDto>(ride))
            .Returns(new RideResponseDto { Id = ride.Id, Status = RideStatus.Cancelled });

        var cmd = new CancelRideCommand(new RideCancelDto { RideId = ride.Id, Reason = "Changed my mind" });
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

        var cmd = new CancelRideCommand(new RideCancelDto { RideId = Guid.NewGuid(), Reason = "Test" });

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
