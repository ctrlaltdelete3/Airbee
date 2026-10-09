namespace Airbee.Shared.Messages;

// A single PM measurement, published by Collector and consumed by Processing.
public record MeasurementMessage
{
    public long SensorId { get; init; }
    public string SensorType { get; init; } = string.Empty;

    public long LocationId { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public DateTime TimestampUtc { get; init; }

    // µg/m³
    public double Pm10 { get; init; }
    public double Pm25 { get; init; }
}
