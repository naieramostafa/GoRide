using RideSharing.Core.ValueObjects;

namespace RideSharing.Application.Interfaces;

public interface IPaymentService
{
    Task<(string PaymentIntentId, string ClientSecret, string? ChargeId)> CreatePaymentIntent(Money amount, string customerId, string? paymentMethod = null, bool confirm = false);
    Task<string> CapturePayment(string paymentIntentId);
    Task<string> CreateCustomer(string email, string name);
    Task RefundPayment(string chargeId);
}
