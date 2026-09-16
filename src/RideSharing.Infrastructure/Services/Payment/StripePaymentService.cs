using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RideSharing.Application.Interfaces;
using RideSharing.Core.ValueObjects;
using Stripe;

namespace RideSharing.Infrastructure.Services.Payment;

public class StripePaymentService : IPaymentService
{
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
    {
        _logger = logger;
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
    }

    public async Task<(string PaymentIntentId, string ClientSecret, string? ChargeId)> CreatePaymentIntent(Money amount, string customerId, string? paymentMethod = null, bool confirm = false)
    {
        try
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
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment intent creation failed");
            throw new InvalidOperationException($"Payment processing failed: {ex.Message}");
        }
    }

    public async Task<string> CapturePayment(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            var intent = await service.CaptureAsync(paymentIntentId);
            return intent.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe payment capture failed for {PaymentIntentId}", paymentIntentId);
            throw new InvalidOperationException($"Payment capture failed: {ex.Message}");
        }
    }

    public async Task<string> CreateCustomer(string email, string name)
    {
        try
        {
            var options = new CustomerCreateOptions { Email = email, Name = name };
            var service = new CustomerService();
            var customer = await service.CreateAsync(options);
            return customer.Id;
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe customer creation failed for {Email}", email);
            throw new InvalidOperationException($"Customer creation failed: {ex.Message}");
        }
    }

    public async Task RefundPayment(string chargeId)
    {
        try
        {
            var options = new RefundCreateOptions { Charge = chargeId };
            var service = new RefundService();
            await service.CreateAsync(options);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe refund failed for {ChargeId}", chargeId);
            throw new InvalidOperationException($"Refund failed: {ex.Message}");
        }
    }
}
