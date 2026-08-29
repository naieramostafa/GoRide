using RideSharing.Core.Enums;

namespace RideSharing.Application.DTOs;

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid RideId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public PaymentStatus Status { get; set; }
    public string? ClientSecret { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class ProcessPaymentDto
{
    public string? PaymentMethod { get; set; }
    public bool? Confirm { get; set; }
}

public class PaymentIntentResponseDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
}
