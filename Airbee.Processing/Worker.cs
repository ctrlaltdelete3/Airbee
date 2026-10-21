using System.Text.Json;
using Airbee.Shared.Messages;
using Airbee.Shared.RabbitMq;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Airbee.Processing
{
    public class Worker(IOptions<RabbitMqOptions> options, IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = options.Value.Host,
                Port = options.Value.Port,
                UserName = options.Value.UserName,
                Password = options.Value.Password
            };

            // Connected at startup (not lazy like the publisher), because a consumer has to be listening
            await using var connection = await factory.CreateConnectionAsync(stoppingToken);
            await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            await MeasurementQueue.DeclareAsync(channel, stoppingToken);

            // RabbitMQ sends at most 10 messages that are not acked yet
            await channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (sender, args) =>
            {
                await HandleMessageAsync(channel, args, stoppingToken);
            };

            await channel.BasicConsumeAsync(queue: QueueNames.Measurements, autoAck: false,
                consumer: consumer, cancellationToken: stoppingToken);

            logger.LogInformation("Listening on queue {Queue}", QueueNames.Measurements);

            // Messages arrive through the event above; this only keeps the connection open until shutdown.
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Shutting down, not an error.
            }
        }

        private async Task HandleMessageAsync(IChannel channel, BasicDeliverEventArgs args,
            CancellationToken stoppingToken)
        {
            MeasurementMessage? message = null;
            try
            {
                message = JsonSerializer.Deserialize<MeasurementMessage>(args.Body.Span);
            }
            catch (JsonException ex)
            {
                logger.LogWarning(ex, "Message is not valid JSON");
            }

            if (message == null)
            {
                // Will never succeed, so don't put it back in the queue; it goes to the dead-letter queue
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            try
            {
                // Worker is a singleton, DbContext is scoped: new scope (and DbContext) for every message
                await using var scope = scopeFactory.CreateAsyncScope();
                var saver = scope.ServiceProvider.GetRequiredService<MeasurementSaver>();

                await saver.SaveAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Shutting down. No ack, so RabbitMQ gives the message to us again on the next start.
                return;
            }
            catch (Exception ex)
            {
                // First failure: try once more. Second failure: dead-letter queue.
                bool requeue = args.Redelivered == false;

                logger.LogError(ex, "Saving measurement from sensor {SensorId} failed, requeue: {Requeue}",
                    message.SensorId, requeue);
                await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: requeue);
                return;
            }

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false);
        }
    }
}
