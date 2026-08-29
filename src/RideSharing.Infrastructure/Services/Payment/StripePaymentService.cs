using Microsoft.Extensions.Configuration;
using RideSharing.Application.Interfaces;
using RideSharing.Core.ValueObjects;
using Stripe;

namespace RideSharing.Infrastructure.Services.Payment;

public class StripePaymentService : IPaymentService
{
    public StripePaymentService(IConfiguration configuration)
    {
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    }

    public async Task<(string PaymentIntentId, string ClientSecret, string? ChargeId)> CreatePaymentIntent(Money amount, string customerId, string? paymentMethod = null, bool confirm = false)
    {
        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)(amount.Amount * 100),
            Currency = amount.Currency.ToLower(),
            PaymentMethodTypes = ["card"],
        };

        if (confirm && !string.IsNullOrEmpty(paymentMethod))
        {
            options.PaymentMethod = paymentMethod;
            options.Confirm = true;
        }

        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(options);

        string? chargeId = null;
        if (confirm)
        {
            chargeId = intent.LatestChargeId;
        }

        return (intent.Id, intent.ClientSecret, chargeId);
    }

    public async Task<string> CapturePayment(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        var intent = await service.CaptureAsync(paymentIntentId);
        return intent.Id;
    }

    public async Task<string> CreateCustomer(string email, string name)
    {
        var options = new CustomerCreateOptions { Email = email, Name = name };
        var service = new CustomerService();
        var customer = await service.CreateAsync(options);
        return customer.Id;
    }

    public async Task RefundPayment(string chargeId)
    {
        var options = new RefundCreateOptions { Charge = chargeId };
        var service = new RefundService();
        await service.CreateAsync(options);
    }
}
