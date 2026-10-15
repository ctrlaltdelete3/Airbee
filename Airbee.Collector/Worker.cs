using Airbee.Collector.SensorCommunity;

namespace Airbee.Collector
{
    public class Worker(SensorCommunityClient client, ILogger<Worker> logger) : BackgroundService
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
                    logger.LogInformation("Fetched {Count} measurements from Sensor.Community", measurements.Count);
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
