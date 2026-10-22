using Airbee.Data.Context;
using Airbee.Shared.Messages;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Airbee.Processing.Tests;

public class MeasurementSaverTests : IDisposable
{
    // In-memory SQLite database lives only while this connection is open
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AirbeeDbContext> _options;

    public MeasurementSaverTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AirbeeDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var dbContext = new AirbeeDbContext(_options);
        dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    // New DbContext for every save, like one scope per message in the Worker
    private async Task SaveAsync(MeasurementMessage message)
    {
        using var dbContext = new AirbeeDbContext(_options);
        var saver = new MeasurementSaver(dbContext, NullLogger<MeasurementSaver>.Instance);

        await saver.SaveAsync(message, CancellationToken.None);
    }

    // Shaped like a real SDS011 measurement from Osijek
    private static MeasurementMessage CreateMessage(DateTime timestampUtc)
    {
        return new MeasurementMessage
        {
            SensorId = 98054,
            SensorType = "SDS011",
            City = "Osijek",
            LocationId = 91038,
            Latitude = 45.56,
            Longitude = 18.67,
            TimestampUtc = timestampUtc,
            Pm10 = 8.35,
            Pm25 = 3.25
        };
    }

    [Fact]
    public async Task SaveAsync_NewSensor_SavesSensorAndMeasurement()
    {
        await SaveAsync(CreateMessage(new DateTime(2026, 10, 21, 10, 9, 7, DateTimeKind.Utc)));

        using var dbContext = new AirbeeDbContext(_options);
        var sensor = Assert.Single(dbContext.Sensors);
        Assert.Equal(98054, sensor.Id);
        Assert.Equal("SDS011", sensor.Type);
        Assert.Equal("Osijek", sensor.City);

        var measurement = Assert.Single(dbContext.Measurements);
        Assert.Equal(98054, measurement.SensorId);
        Assert.Equal(8.35, measurement.Pm10);
    }

    [Fact]
    public async Task SaveAsync_ExistingSensor_AddsMeasurementWithoutDuplicatingSensor()
    {
        await SaveAsync(CreateMessage(new DateTime(2026, 10, 21, 10, 9, 7, DateTimeKind.Utc)));
        await SaveAsync(CreateMessage(new DateTime(2026, 10, 21, 10, 11, 33, DateTimeKind.Utc)));

        using var dbContext = new AirbeeDbContext(_options);
        Assert.Single(dbContext.Sensors);
        Assert.Equal(2, dbContext.Measurements.Count());
    }

    [Fact]
    public async Task SaveAsync_SameMeasurementTwice_SavesItOnce()
    {
        var message = CreateMessage(new DateTime(2026, 10, 21, 10, 9, 7, DateTimeKind.Utc));

        await SaveAsync(message);
        // Without the duplicate check this would throw on the unique constraint
        await SaveAsync(message);

        using var dbContext = new AirbeeDbContext(_options);
        Assert.Single(dbContext.Measurements);
    }
}
