using RideSharing.Core.Enums;
using RideSharing.Core.Exceptions;
using RideSharing.Core.ValueObjects;

namespace RideSharing.Core.Entities;

public class Payment
{
    public Guid Id { get; private set; }
    public Guid RideId { get; private set; }
    public Ride Ride { get; private set; } = null!;
    public Guid PassengerId { get; private set; }
    public Money Amount { get; private set; } = Money.Zero;
    public PaymentStatus Status { get; private set; }
    public string? StripePaymentIntentId { get; private set; }
    public string? StripeChargeId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private Payment() { }

    public Payment(Ride ride, Money amount)
    {
        Id = Guid.NewGuid();
        RideId = ride.Id;
        Ride = ride;
        PassengerId = ride.PassengerId;
        Amount = amount;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void Process(string paymentIntentId)
    {
        StripePaymentIntentId = paymentIntentId;
        Status = PaymentStatus.Processing;
    }

    public void Complete(string chargeId)
    {
        StripeChargeId = chargeId;
        Status = PaymentStatus.Completed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
    }

    public void Refund()
    {
        if (Status != PaymentStatus.Completed)
            throw new PaymentFailedException("Only completed payments can be refunded");
        Status = PaymentStatus.Refunded;
    }
}
