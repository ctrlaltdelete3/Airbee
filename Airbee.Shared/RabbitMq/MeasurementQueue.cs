using Airbee.Shared.Messages;
using RabbitMQ.Client;

namespace Airbee.Shared.RabbitMq;

// Collector and Processing both declare the queues here, so the settings always match.
// RabbitMQ refuses to redeclare an existing queue with different arguments (PRECONDITION_FAILED).
public static class MeasurementQueue
{
    public const string DeadLetterExchange = "measurements.dlx";
    public const string DeadLetterQueue = "measurements.dead";

    public static async Task DeclareAsync(IChannel channel, CancellationToken cancellationToken)
    {
        // Fanout ignores the routing key. Dead-lettered messages keep their original one ("measurements").
        await channel.ExchangeDeclareAsync(
            exchange: DeadLetterExchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: DeadLetterQueue,
            exchange: DeadLetterExchange,
            routingKey: string.Empty,
            cancellationToken: cancellationToken);

        // Rejected messages (BasicNack with requeue: false) go to the dead-letter exchange instead of being deleted
        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = DeadLetterExchange
        };

        await channel.QueueDeclareAsync(
            queue: QueueNames.Measurements,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: cancellationToken);
    }
}
