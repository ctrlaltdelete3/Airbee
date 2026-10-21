using System.Text.Json.Serialization;

namespace Airbee.Collector.SensorCommunity.Dtos;

public class SensorDataValueDto
{
    [JsonConverter(typeof(StringOrNumberJsonConverter))]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("value_type")]
    public string ValueType { get; set; } = string.Empty;
}
