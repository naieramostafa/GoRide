using FluentAssertions;
using RideSharing.Core.Entities;
using RideSharing.Core.Enums;
using RideSharing.Core.ValueObjects;
using Xunit;

namespace RideSharing.UnitTests.Domain;

public class PaymentTests
{
    [Fact]
    public void CreatePayment_ShouldSetPendingStatus()
    {
        var ride = CreateTestRide();
        var payment = new Payment(ride, new Money(25.50m));

        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Amount.Amount.Should().Be(25.50m);
        payment.RideId.Should().Be(ride.Id);
    }

    [Fact]
    public void Process_ShouldSetProcessingStatus()
    {
        var payment = CreateTestPayment();

        payment.Process("pi_test_123");

        payment.Status.Should().Be(PaymentStatus.Processing);
        payment.StripePaymentIntentId.Should().Be("pi_test_123");
    }

    [Fact]
    public void Complete_ShouldSetCompletedStatus()
    {
        var payment = CreateTestPayment();
        payment.Process("pi_test_123");

        payment.Complete("ch_test_123");

        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.StripeChargeId.Should().Be("ch_test_123");
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Fail_ShouldSetFailedStatus()
    {
        var payment = CreateTestPayment();

        payment.Fail("Insufficient funds");

        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Refund_ShouldSetRefundedStatus()
    {
        var payment = CreateTestPayment();
        payment.Process("pi_test_123");
        payment.Complete("ch_test_123");

        payment.Refund();

        payment.Status.Should().Be(PaymentStatus.Refunded);
    }

    private static Payment CreateTestPayment()
    {
        var ride = CreateTestRide();
        return new Payment(ride, new Money(25.50m));
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
