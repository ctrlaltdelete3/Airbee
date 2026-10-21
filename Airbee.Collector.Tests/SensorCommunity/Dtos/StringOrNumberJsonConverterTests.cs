using System.Text.Json;
using Airbee.Collector.SensorCommunity.Dtos;

namespace Airbee.Collector.Tests.SensorCommunity.Dtos;

public class StringOrNumberJsonConverterTests
{
    // GetFromJsonAsync in the client uses Web settings (ignores letter case: "value" = "Value").
    // Deserialize doesn't by default, so the test passes the same settings.
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Read_StringValue_ReturnsSameString()
    {
        var json = """{ "value_type": "P1", "value": "12.47" }""";

        var dto = JsonSerializer.Deserialize<SensorDataValueDto>(json, Options);

        Assert.NotNull(dto);
        Assert.Equal("12.47", dto.Value);
    }

    [Theory]
    [InlineData("101325.5")]
    [InlineData("101325")]
    public void Read_NumberValue_ReturnsRawText(string number)
    {
        // e.g. BME280 sends pressure_at_sealevel as a number
        var json = $$"""{ "value_type": "pressure_at_sealevel", "value": {{number}} }""";

        var dto = JsonSerializer.Deserialize<SensorDataValueDto>(json, Options);

        Assert.NotNull(dto);
        Assert.Equal(number, dto.Value);
    }

    [Fact]
    public void Read_UnsupportedToken_ThrowsJsonException()
    {
        var json = """{ "value_type": "P1", "value": true }""";

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SensorDataValueDto>(json, Options));
    }
}
