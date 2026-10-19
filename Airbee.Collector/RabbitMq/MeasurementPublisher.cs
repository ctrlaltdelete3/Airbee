using System.Text.Json;
using Airbee.Shared.Messages;
using Airbee.Shared.RabbitMq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Airbee.Collector.RabbitMq;

// One connection for the whole life of the Collector. Only the Worker calls this, one message at a time.
public class MeasurementPublisher(IOptions<RabbitMqOptions> options) : IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;

    public async Task PublishAsync(MeasurementMessage message, CancellationToken cancellationToken)
    {
        var channel = await GetChannelAsync(cancellationToken);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);

        var properties = new BasicProperties
        {
            // Written to disk, so it survives a RabbitMQ restart
            Persistent = true,
            ContentType = "application/json"
        };

        // Default exchange ("") routes straight to the queue with the same name as the routing key.
        // mandatory: true + publisher confirms -> throws if the message wasn't routed or saved.
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: QueueNames.Measurements,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    // Opened on first publish, so the Collector can start before RabbitMQ is up.
    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel != null)
        {
            return _channel;
        }

        if (_connection == null)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Value.Host,
                Port = options.Value.Port,
                UserName = options.Value.UserName,
                Password = options.Value.Password
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
        }

        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        var channel = await _connection.CreateChannelAsync(channelOptions, cancellationToken);

        await channel.QueueDeclareAsync(
            queue: QueueNames.Measurements,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        // Saved only after the queue exists, so we never publish to a missing queue
        _channel = channel;
        return _channel;
    }

    // Called by the DI container when the Collector shuts down
    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
        {
            await _channel.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}
