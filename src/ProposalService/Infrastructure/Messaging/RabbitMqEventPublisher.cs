using System.Text;
using System.Text.Json;

using ProposalService.Domain.Ports;

using RabbitMQ.Client;

namespace ProposalService.Infrastructure.Messaging;

public sealed class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly IChannel _channel;
    private readonly string _exchangeName;
    private bool _disposed;

    public RabbitMqEventPublisher(IConnection connection, string exchangeName = "insurance-events")
    {
        ArgumentNullException.ThrowIfNull(connection);

        _exchangeName = exchangeName;
        _channel = connection.CreateChannelAsync().GetAwaiter().GetResult();

        _channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false
        ).GetAwaiter().GetResult();
    }

    public async Task PublishAsync<TEvent>(TEvent @event, string routingKey, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        if (string.IsNullOrWhiteSpace(routingKey))
            throw new ArgumentException("Routing key cannot be null or empty", nameof(routingKey));

        var json = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            ContentType = "application/json",
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken
        );
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _channel?.Dispose();
        }

        _disposed = true;
    }
}
