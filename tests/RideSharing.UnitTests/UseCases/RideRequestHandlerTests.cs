using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using RideSharing.Application.DTOs;
using RideSharing.Application.Interfaces;
using RideSharing.Application.Mappings;
using RideSharing.Application.UseCases.Rides.Commands;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;

namespace RideSharing.UnitTests.UseCases;

public class RideRequestHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IPassengerRepository> _passengerRepoMock;
    private readonly Mock<IRideRepository> _rideRepoMock;
    private readonly Mock<IMapService> _mapServiceMock;
    private readonly IMapper _mapper;
    private readonly RequestRideHandler _handler;

    public RideRequestHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _passengerRepoMock = new Mock<IPassengerRepository>();
        _rideRepoMock = new Mock<IRideRepository>();
        _mapServiceMock = new Mock<IMapService>();

        _uowMock.Setup(u => u.Passengers).Returns(_passengerRepoMock.Object);
        _uowMock.Setup(u => u.Rides).Returns(_rideRepoMock.Object);

        _mapServiceMock.Setup(m => m.CalculateDistanceAsync(It.IsAny<Location>(), It.IsAny<Location>())).ReturnsAsync(10.0);
        _mapServiceMock.Setup(m => m.EstimateDurationAsync(It.IsAny<Location>(), It.IsAny<Location>())).ReturnsAsync(15);

        _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>()).CreateMapper();
        _handler = new RequestRideHandler(_uowMock.Object, _mapper, _mapServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateRide()
    {
        var user = new User("John", "Doe", "j@t.com", "123", UserRole.Passenger, "pass");
        var passenger = new Passenger(user);
        _passengerRepoMock.Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync(passenger);
        _rideRepoMock.Setup(r => r.AddAsync(It.IsAny<Ride>())).ReturnsAsync((Ride r) => r);

        var cmd = new RequestRideCommand(new RideRequestDto
        {
            UserId = passenger.UserId,
            PickupLatitude = 40.7128,
            PickupLongitude = -74.0060,
            PickupAddress = "NYC",
            DropoffLatitude = 40.7580,
            DropoffLongitude = -73.9855,
            DropoffAddress = "Times Square"
        });

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be(RideStatus.Pending);
        _rideRepoMock.Verify(r => r.AddAsync(It.IsAny<Ride>()), Times.Once);
        _uowMock.Verify(u => u.CommitAsync(), Times.Once);
    }
}
