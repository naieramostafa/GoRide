namespace RideSharing.Application.Interfaces;

public interface INotificationService
{
    Task SendPushNotificationAsync(Guid userId, string title, string body);
    Task SendEmailAsync(string email, string subject, string body);
    Task SendSmsAsync(string phone, string message);
}
