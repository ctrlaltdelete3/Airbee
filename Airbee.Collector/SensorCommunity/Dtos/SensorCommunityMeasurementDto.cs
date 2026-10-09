namespace Airbee.Collector.SensorCommunity.Dtos;

public class SensorCommunityMeasurementDto
{
    public SensorDto? Sensor { get; set; }
    public LocationDto? Location { get; set; }

    // e.g. "2026-10-07 07:42:51" - UTC, but without a zone suffix. Not ISO 8601, so parsed manually.
    public string Timestamp { get; set; } = string.Empty;

    public List<SensorDataValueDto> SensorDataValues { get; set; } = [];
}
