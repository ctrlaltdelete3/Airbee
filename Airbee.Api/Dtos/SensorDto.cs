namespace Airbee.Api.Dtos;

public record SensorDto
{
    public long Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    // Null when the sensor has no measurements left (e.g. stopped reporting and old data was deleted)
    public MeasurementDto? LatestMeasurement { get; init; }
}
