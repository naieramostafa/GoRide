using Microsoft.Extensions.Logging;
using RideSharing.Application.Interfaces;

namespace RideSharing.Infrastructure.Services.Notifications;

/// <summary>
/// Log-only stub for notifications.
/// TODO: Replace with real push/email/SMS integration (Firebase, SendGrid, Twilio).
/// </summary>
public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendPushNotificationAsync(Guid userId, string title, string body)
    {
        _logger.LogInformation("Push notification to {UserId}: {Title} - {Body}", userId, title, body);
        return Task.CompletedTask;
    }

    public Task SendEmailAsync(string email, string subject, string body)
    {
        _logger.LogInformation("Email to {Email}: {Subject}", email, subject);
        return Task.CompletedTask;
    }

    public Task SendSmsAsync(string phone, string message)
    {
        _logger.LogInformation("SMS to {Phone}: {Message}", phone, message);
        return Task.CompletedTask;
    }
}
