using Airbee.Collector.SensorCommunity;
using Airbee.Shared.Messages;

namespace Airbee.Collector
{
    public class Worker(SensorCommunityClient client, SensorCommunityMapper mapper, ILogger<Worker> logger) : BackgroundService
    {
        // The filter endpoint only returns the last 5 minutes, so this must stay below that.
        private static readonly TimeSpan PollingInterval = TimeSpan.FromMinutes(2.5);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    var measurements = await client.GetMeasurementsAsync(stoppingToken);

                    var measurementMessages = new List<MeasurementMessage>();

                    foreach (var measurement in measurements)
                    {
                        var measurementMessage = mapper.Map(measurement);
                        if (measurementMessage != null)
                        {
                            measurementMessages.Add(measurementMessage);
                        }
                    }

                    logger.LogInformation("Fetched {Count} measurements from Sensor.Community. Mapped: {MappedCount}", measurements.Count, measurementMessages.Count);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    // Shutting down, not an error.
                    break;
                }
                catch (Exception ex)
                {
                    // Log and keep going; an unhandled exception would stop the whole host.
                    logger.LogError(ex, "Fetching measurements from Sensor.Community failed");
                }

                await Task.Delay(PollingInterval, stoppingToken);
            }
        }
    }
}
