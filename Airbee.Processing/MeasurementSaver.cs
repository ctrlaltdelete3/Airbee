using Airbee.Data.Context;
using Airbee.Data.Entities;
using Airbee.Shared.Messages;
using Microsoft.EntityFrameworkCore;

namespace Airbee.Processing
{
    public class MeasurementSaver
    {
        private readonly AirbeeDbContext _dbContext;
        private readonly ILogger<MeasurementSaver> _logger;

        public MeasurementSaver(AirbeeDbContext dbContext, ILogger<MeasurementSaver> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task SaveAsync(MeasurementMessage message, CancellationToken cancellationToken)
        {
            // Find sensor, if it doesn't exist - create it
            var sensor = await _dbContext.Sensors.FindAsync([message.SensorId], cancellationToken);

            if (sensor == null)
            {
                _dbContext.Sensors.Add(new Sensor
                {
                    Id = message.SensorId,
                    Type = message.SensorType,
                    City = message.City,
                    LocationId = message.LocationId,
                    Latitude = message.Latitude,
                    Longitude = message.Longitude
                });
            }

            // The same measurement can arrive twice: consecutive fetches overlap (the endpoint returns
            // the last 5 minutes) and RabbitMQ redelivers a message if we crash before the ack.
            bool exists = await _dbContext.Measurements.AnyAsync(m => m.SensorId == message.SensorId
                && m.TimestampUtc == message.TimestampUtc, cancellationToken);

            if (exists)
            {
                _logger.LogInformation("Measurement from sensor {SensorId} at {TimestampUtc} already saved, skipping",
                    message.SensorId, message.TimestampUtc);
            }
            else
            {
                _dbContext.Measurements.Add(new Measurement
                {
                    SensorId = message.SensorId,
                    TimestampUtc = message.TimestampUtc,
                    Pm10 = message.Pm10,
                    Pm25 = message.Pm25
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
