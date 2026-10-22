namespace Airbee.Api.Dtos;

public record MeasurementDto
{
    public DateTime TimestampUtc { get; init; }

    // µg/m³
    public double Pm10 { get; init; }
    public double Pm25 { get; init; }
}
