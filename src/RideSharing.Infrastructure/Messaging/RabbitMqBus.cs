using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using RideSharing.Application.Interfaces;

namespace RideSharing.Infrastructure.Messaging;

public class RabbitMqBus : IMessageBus, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMqBus> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private bool _disposed;
    private int _retryCount;

    private const int MaxRetries = 5;
    private static readonly TimeSpan BaseDelay = TimeSpan.FromSeconds(1);

    public RabbitMqBus(IConfiguration configuration, ILogger<RabbitMqBus> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private void EnsureConnection()
    {
        if (_channel is { IsOpen: true })
        {
            _retryCount = 0;
            return;
        }

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMq:Host"] ?? "localhost",
            Port = int.Parse(_configuration["RabbitMq:Port"] ?? "5672"),
            UserName = _configuration["RabbitMq:Username"] ?? "guest",
            Password = _configuration["RabbitMq:Password"] ?? "guest",
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
            RequestedHeartbeat = TimeSpan.FromSeconds(30)
        };

        while (_retryCount < MaxRetries)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                _retryCount = 0;
                _logger.LogInformation("Connected to RabbitMQ");
                return;
            }
            catch (BrokerUnreachableException ex)
            {
                _retryCount++;
                var delay = TimeSpan.FromTicks(BaseDelay.Ticks * (long)Math.Pow(2, _retryCount - 1));
                _logger.LogWarning(ex,
                    "RabbitMQ unreachable (attempt {Retry}/{MaxRetries}), retrying in {Delay}s",
                    _retryCount, MaxRetries, delay.TotalSeconds);
                Thread.Sleep(delay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to RabbitMQ");
                break;
            }
        }

        _logger.LogError("Could not connect to RabbitMQ after {MaxRetries} attempts", MaxRetries);
    }

    public Task PublishAsync<T>(string exchange, string routingKey, T message)
    {
        EnsureConnection();

        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("Cannot publish message - RabbitMQ not connected");
            return Task.CompletedTask;
        }

        try
        {
            _channel.ExchangeDeclare(exchange, ExchangeType.Topic, durable: true);
            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);
            _channel.BasicPublish(exchange, routingKey, null, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish message to {Exchange}/{RoutingKey}", exchange, routingKey);
        }

        return Task.CompletedTask;
    }

    public Task SubscribeAsync<T>(string queue, string routingKey, Func<T, Task> handler)
    {
        EnsureConnection();

        if (_channel == null || !_channel.IsOpen)
        {
            _logger.LogWarning("Cannot subscribe - RabbitMQ not connected");
            return Task.CompletedTask;
        }

        try
        {
            _channel.ExchangeDeclare("ride-sharing", ExchangeType.Topic, durable: true);
            _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueBind(queue, "ride-sharing", routingKey);
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var message = JsonSerializer.Deserialize<T>(json);
                    if (message != null)
                        await handler(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling message from {Queue}", queue);
                }
            };
            _channel.BasicConsume(queue, true, consumer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to {Queue}", queue);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _channel?.Close();
        _connection?.Close();
    }
}
