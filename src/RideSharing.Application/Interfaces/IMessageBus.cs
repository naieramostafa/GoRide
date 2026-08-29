namespace RideSharing.Application.Interfaces;

public interface IMessageBus
{
    Task PublishAsync<T>(string exchange, string routingKey, T message);
    Task SubscribeAsync<T>(string queue, string routingKey, Func<T, Task> handler);
}
