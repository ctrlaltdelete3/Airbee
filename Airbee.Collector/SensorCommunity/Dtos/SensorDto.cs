using System.Text.Json.Serialization;

namespace Airbee.Collector.SensorCommunity.Dtos;

public class SensorDto
{
    public long Id { get; set; }

    [JsonPropertyName("sensor_type")]
    public SensorTypeDto? SensorType { get; set; }
}
