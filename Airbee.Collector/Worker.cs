using Airbee.Collector.RabbitMq;
using Airbee.Collector.SensorCommunity;
using Airbee.Shared.Messages;
using Microsoft.Extensions.Options;

namespace Airbee.Collector
{
    public class Worker(SensorCommunityClient client, SensorCommunityMapper mapper,
        MeasurementPublisher publisher, IOptions<SensorCommunityOptions> options,
        ILogger<Worker> logger) : BackgroundService
    {
        // The filter endpoint only returns the last 5 minutes, so this must stay below that.
        private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(2.5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                foreach (var area in options.Value.Areas)
                {
                    try
                    {
                        var measurements = await client.GetMeasurementsAsync(area, stoppingToken);

                        var measurementMessages = new List<MeasurementMessage>();

                        foreach (var measurement in measurements)
                        {
                            var measurementMessage = mapper.Map(measurement);
                            if (measurementMessage != null)
                            {
                                measurementMessages.Add(measurementMessage);
                            }
                        }

                        logger.LogInformation("{City}: fetched {Count} measurements from Sensor.Community. Mapped: {MappedCount}",
                            area.Name, measurements.Count, measurementMessages.Count);

                        foreach (var measurementMessage in measurementMessages)
                        {
                            await publisher.PublishAsync(measurementMessage, stoppingToken);
                        }

                        logger.LogInformation("{City}: published {Count} measurements to RabbitMQ",
                            area.Name, measurementMessages.Count);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        // Shutting down, not an error.
                        return;
                    }
                    catch (Exception ex)
                    {
                        // Log and move on to the next city; one failing city shouldn't stop the others.
                        logger.LogError(ex, "{City}: fetching or publishing measurements failed", area.Name);
                    }
                }

                await Task.Delay(PollingInterval, stoppingToken);
            }
        }
    }
}
