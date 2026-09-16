using AutoMapper;
using FluentAssertions;
using Moq;
using RideSharing.Application.DTOs;
using RideSharing.Application.Interfaces;
using RideSharing.Application.UseCases.Payments.Commands;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;
using Xunit;

namespace RideSharing.UnitTests.UseCases;

public class ProcessPaymentHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IPaymentService> _paymentServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IRideRepository> _rideRepoMock = new();
    private readonly Mock<IPaymentRepository> _paymentRepoMock = new();
    private readonly Mock<IPassengerRepository> _passengerRepoMock = new();
    private readonly ProcessPaymentHandler _handler;

    public ProcessPaymentHandlerTests()
    {
        _uowMock.Setup(x => x.Rides).Returns(_rideRepoMock.Object);
        _uowMock.Setup(x => x.Payments).Returns(_paymentRepoMock.Object);
        _uowMock.Setup(x => x.Passengers).Returns(_passengerRepoMock.Object);
        _handler = new ProcessPaymentHandler(_uowMock.Object, _paymentServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ValidPayment_ShouldProcess()
    {
        var ride = CreateRideWithPayment(out var payment, out var passenger);
        _rideRepoMock.Setup(x => x.GetByIdAsync(ride.Id)).ReturnsAsync(ride);
        _passengerRepoMock.Setup(x => x.GetByUserIdAsync(passenger.UserId)).ReturnsAsync(passenger);
        _paymentRepoMock.Setup(x => x.GetByRideIdAsync(ride.Id)).ReturnsAsync(payment);
        _paymentServiceMock.Setup(x => x.CreatePaymentIntent(
            It.IsAny<Money>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .ReturnsAsync(("pi_test", "pi_test_secret", (string?)null));
        _mapperMock.Setup(x => x.Map<PaymentResponseDto>(payment))
            .Returns(new PaymentResponseDto { Id = payment.Id, Status = PaymentStatus.Processing });

        var cmd = new ProcessPaymentCommand(ride.Id, new ProcessPaymentDto { PaymentMethod = "pm_card_visa" }, passenger.UserId);
        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(PaymentStatus.Processing);
        _uowMock.Verify(x => x.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task Handle_NonExistentRide_ShouldThrowKeyNotFound()
    {
        _rideRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Ride?)null);

        var cmd = new ProcessPaymentCommand(Guid.NewGuid(), new ProcessPaymentDto(), Guid.NewGuid());

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_NotPassenger_ShouldThrowUnauthorized()
    {
        var ride = CreateRideWithPayment(out _, out _);
        _rideRepoMock.Setup(x => x.GetByIdAsync(ride.Id)).ReturnsAsync(ride);
        _passengerRepoMock.Setup(x => x.GetByUserIdAsync(It.IsAny<Guid>())).ReturnsAsync((Passenger?)null);

        var cmd = new ProcessPaymentCommand(ride.Id, new ProcessPaymentDto(), Guid.NewGuid());

        await FluentActions.Awaiting(() => _handler.Handle(cmd, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    private static Ride CreateRideWithPayment(out Payment payment, out Passenger passenger)
    {
        var passengerUser = new User("John", "Doe", "john@test.com", "123", UserRole.Passenger, "pass");
        passenger = new Passenger(passengerUser);
        var pickup = new Location(40.7128, -74.0060);
        var dropoff = new Location(40.7580, -73.9855);
        var ride = new Ride(passenger, pickup, dropoff, new Money(25.50m));

        var driverUser = new User("Jane", "Doe", "jane@test.com", "456", UserRole.Driver, "pass");
        var vehicle = new Vehicle("Toyota", "Camry", 2023, "White", "ABC123", VehicleType.Sedan, 4);
        var driver = new Driver(driverUser, "LIC123", vehicle);
        ride.AssignDriver(driver);
        ride.StartRide();
        ride.CompleteRide(10.5, 25, new Money(35.0m));

        payment = new Payment(ride, new Money(35.0m));
        return ride;
    }
}
