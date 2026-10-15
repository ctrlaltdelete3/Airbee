using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Airbee.Collector.SensorCommunity.Dtos;

// Sensor.Community sends most values as strings, but some as numbers (e.g. BME280 "pressure_at_sealevel").
public class StringOrNumberJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString()!;

            case JsonTokenType.Number:
                // Raw text as written in the JSON, so no culture or rounding issues.
                return Encoding.UTF8.GetString(
                    reader.HasValueSequence ? reader.ValueSequence.ToArray() : reader.ValueSpan);

            default:
                throw new JsonException($"Expected a string or a number, got {reader.TokenType}.");
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
