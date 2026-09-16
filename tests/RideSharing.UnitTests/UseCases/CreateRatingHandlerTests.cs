using AutoMapper;
using FluentAssertions;
using Moq;
using RideSharing.Application.DTOs;
using RideSharing.Application.Interfaces;
using RideSharing.Application.UseCases.Ratings.Commands;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;
using Xunit;

namespace RideSharing.UnitTests.UseCases;

public class CreateRatingHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IRideRepository> _rideRepoMock = new();
    private readonly Mock<IRatingRepository> _ratingRepoMock = new();
    private readonly Mock<IDriverRepository> _driverRepoMock = new();
    private readonly CreateRatingHandler _handler;

    public CreateRatingHandlerTests()
    {
        _uowMock.Setup(x => x.Rides).Returns(_rideRepoMock.Object);
        _uowMock.Setup(x => x.Ratings).Returns(_ratingRepoMock.Object);
        _uowMock.Setup(x => x.Drivers).Returns(_driverRepoMock.Object);
        _handler = new CreateRatingHandler(_uowMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRating_ShouldCreate()
    {
        var ride = CreateCompletedRide(out var driver, out var passengerUser);
        _rideRepoMock.Setup(x => x.GetByIdAsync(ride.Id)).ReturnsAsync(ride);
        _ratingRepoMock.Setup(x => x.GetByRideAsync(ride.Id)).ReturnsAsync(Array.Empty<Rating>());
        _driverRepoMock.Setup(x => x.GetByIdAsync(driver.Id)).ReturnsAsync(driver);
        _mapperMock.Setup(x => x.Map<RatingResponseDto>(It.IsAny<Rating>()))
            .Returns(new RatingResponseDto { RideId = ride.Id, Score = 5 });

        var cmd = new CreateRatingCommand(new CreateRatingDto
        {
            RideId = ride.Id,
            RatedByUserId = passengerUser.Id,
            RatedUserId = driver.Id,
            Score = 5,
            Comment = "Great ride!"
        });
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Score.Should().Be(5);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_RideNotCompleted_ShouldThrow()
    {
        var ride = CreatePendingRide();
        _rideRepoMock.Setup(x => x.GetByIdAsync(ride.Id)).ReturnsAsync(ride);

        var cmd = new CreateRatingCommand(new CreateRatingDto
        {
            RideId = ride.Id,
            RatedByUserId = Guid.NewGuid(),
            RatedUserId = Guid.NewGuid(),
            Score = 5
        });

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<RideSharing.Core.Exceptions.RideStateException>();
    }

    [Fact]
    public async Task Handle_DuplicateRating_ShouldThrow()
    {
        var ride = CreateCompletedRide(out var driver, out var passengerUser);
        var existingRating = new Rating(ride, passengerUser.Id, driver.Id, 4);
        _rideRepoMock.Setup(x => x.GetByIdAsync(ride.Id)).ReturnsAsync(ride);
        _ratingRepoMock.Setup(x => x.GetByRideAsync(ride.Id)).ReturnsAsync([existingRating]);

        var cmd = new CreateRatingCommand(new CreateRatingDto
        {
            RideId = ride.Id,
            RatedByUserId = passengerUser.Id,
            RatedUserId = driver.Id,
            Score = 5
        });

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<RideSharing.Core.Exceptions.RideStateException>();
    }

    private static Ride CreateCompletedRide(out Driver driver, out User passengerUser)
    {
        passengerUser = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        var passenger = new Passenger(passengerUser);
        var driverUser = new User("Jane", "Doe", "jane@test.com", "456", UserRole.Driver, "pass");
        var vehicle = new Vehicle("Toyota", "Camry", 2023, "White", "ABC123", VehicleType.Sedan, 4);
        driver = new Driver(driverUser, "LIC123", vehicle);
        var pickup = new Location(40.7128, -74.0060);
        var dropoff = new Location(40.7580, -73.9855);
        var ride = new Ride(passenger, pickup, dropoff, new Money(25.50m));
        ride.AssignDriver(driver);
        ride.StartRide();
        ride.CompleteRide(10.5, 25, new Money(35.0m));
        return ride;
    }

    private static Ride CreatePendingRide()
    {
        var user = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        var passenger = new Passenger(user);
        var pickup = new Location(40.7128, -74.0060);
        var dropoff = new Location(40.7580, -73.9855);
        return new Ride(passenger, pickup, dropoff, new Money(25.50m));
    }
}
