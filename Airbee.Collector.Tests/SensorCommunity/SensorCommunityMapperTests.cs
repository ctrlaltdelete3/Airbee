using System.Globalization;
using Airbee.Collector.SensorCommunity;
using Airbee.Collector.SensorCommunity.Dtos;

namespace Airbee.Collector.Tests.SensorCommunity;

public class SensorCommunityMapperTests
{
    private const string City = "Osijek";

    private readonly SensorCommunityMapper _mapper = new();

    // Shaped like a real SDS011 measurement from Osijek
    private static SensorCommunityMeasurementDto CreateValidDto()
    {
        return new SensorCommunityMeasurementDto
        {
            Sensor = new SensorDto
            {
                Id = 98054,
                SensorType = new SensorTypeDto { Name = "SDS011" }
            },
            Location = new LocationDto
            {
                Id = 85212,
                Latitude = "45.56",
                Longitude = "18.67",
                Indoor = 0
            },
            Timestamp = "2026-10-07 07:42:51",
            SensorDataValues =
            [
                new SensorDataValueDto { ValueType = "P1", Value = "12.47" },
                new SensorDataValueDto { ValueType = "P2", Value = "8.3" }
            ]
        };
    }

    [Fact]
    public void Map_ValidMeasurement_ReturnsMessage()
    {
        var dto = CreateValidDto();

        var message = _mapper.Map(dto, City);

        Assert.NotNull(message);
        Assert.Equal(98054, message.SensorId);
        Assert.Equal("SDS011", message.SensorType);
        Assert.Equal("Osijek", message.City);
        Assert.Equal(85212, message.LocationId);
        Assert.Equal(45.56, message.Latitude);
        Assert.Equal(18.67, message.Longitude);
        Assert.Equal(12.47, message.Pm10);
        Assert.Equal(8.3, message.Pm25);
    }

    [Fact]
    public void Map_TimestampWithoutZone_IsParsedAsUtc()
    {
        var dto = CreateValidDto();

        var message = _mapper.Map(dto, City);

        Assert.NotNull(message);
        // DateTime equality ignores Kind, so it has to be checked separately
        Assert.Equal(new DateTime(2026, 10, 7, 7, 42, 51, DateTimeKind.Utc), message.TimestampUtc);
        Assert.Equal(DateTimeKind.Utc, message.TimestampUtc.Kind);
    }

    [Fact]
    public void Map_WithCroatianCulture_StillParsesDecimalPoint()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("hr-HR");
        try
        {
            var message = _mapper.Map(CreateValidDto(), City);

            Assert.NotNull(message);
            Assert.Equal(12.47, message.Pm10);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void Map_IndoorSensor_ReturnsNull()
    {
        var dto = CreateValidDto();
        dto.Location!.Indoor = 1;

        Assert.Null(_mapper.Map(dto, City));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Map_MissingSensorOrLocation_ReturnsNull(bool sensorMissing, bool locationMissing)
    {
        var dto = CreateValidDto();
        if (sensorMissing)
        {
            dto.Sensor = null;
        }
        if (locationMissing)
        {
            dto.Location = null;
        }

        Assert.Null(_mapper.Map(dto, City));
    }

    [Theory]
    [InlineData("P1")]
    [InlineData("P2")]
    public void Map_MissingPmValue_ReturnsNull(string missingValueType)
    {
        var dto = CreateValidDto();
        dto.SensorDataValues.RemoveAll(x => x.ValueType == missingValueType);

        Assert.Null(_mapper.Map(dto, City));
    }

    [Fact]
    public void Map_Bme280Sensor_ReturnsNull()
    {
        var dto = CreateValidDto();
        dto.Sensor!.SensorType = new SensorTypeDto { Name = "BME280" };
        dto.SensorDataValues =
        [
            new SensorDataValueDto { ValueType = "temperature", Value = "14.2" },
            new SensorDataValueDto { ValueType = "humidity", Value = "71.5" }
        ];

        Assert.Null(_mapper.Map(dto, City));
    }

    [Theory]
    [InlineData("12,5")]
    [InlineData("abc")]
    [InlineData("")]
    public void Map_InvalidPmValue_ReturnsNull(string invalidValue)
    {
        var dto = CreateValidDto();
        dto.SensorDataValues.First(x => x.ValueType == "P1").Value = invalidValue;

        Assert.Null(_mapper.Map(dto, City));
    }

    [Fact]
    public void Map_InvalidTimestamp_ReturnsNull()
    {
        var dto = CreateValidDto();
        dto.Timestamp = "07.10.2026. 07:42:51";

        Assert.Null(_mapper.Map(dto, City));
    }
}
