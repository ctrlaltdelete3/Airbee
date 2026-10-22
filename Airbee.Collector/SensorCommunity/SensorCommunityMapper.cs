using System.Globalization;
using Airbee.Collector.SensorCommunity.Dtos;
using Airbee.Shared.Messages;

namespace Airbee.Collector.SensorCommunity;

public class SensorCommunityMapper
{
    #region ConstValues
    // Sensor.Community value types: P1 = PM10, P2 = PM2.5
    private const string Pm10ValueType = "P1";
    private const string Pm25ValueType = "P2";

    // UTC, but sent without a zone suffix
    private const string TimestampFormat = "yyyy-MM-dd HH:mm:ss";
    #endregion

    // Returns null if the measurement can't be used and should be skipped
    // TODO: log why a measurement was skipped (useful for debugging)
    public MeasurementMessage? Map(SensorCommunityMeasurementDto dto, string city)
    {
        if (dto.Sensor == null || dto.Location == null)
        {
            return null;
        }

        // Indoor sensors say nothing about outdoor air quality
        if (dto.Location.Indoor == 1)
        {
            return null;
        }

        // Non-PM sensors (e.g. BME280) have no P1/P2, and we need both values
        var pm10Value = dto.SensorDataValues.FirstOrDefault(x => x.ValueType == Pm10ValueType);
        var pm25Value = dto.SensorDataValues.FirstOrDefault(x => x.ValueType == Pm25ValueType);
        if (pm10Value == null || pm25Value == null)
        {
            return null;
        }

        if (TryParseDouble(pm10Value.Value, out var pm10) == false
            || TryParseDouble(pm25Value.Value, out var pm25) == false
            || TryParseDouble(dto.Location.Latitude, out var latitude) == false
            || TryParseDouble(dto.Location.Longitude, out var longitude) == false)
        {
            return null;
        }

        if (DateTime.TryParseExact(
                dto.Timestamp,
                TimestampFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var timestampUtc) == false)
        {
            return null;
        }

        return new MeasurementMessage
        {
            SensorId = dto.Sensor.Id,
            SensorType = dto.Sensor.SensorType?.Name ?? string.Empty,
            City = city,
            LocationId = dto.Location.Id,
            Latitude = latitude,
            Longitude = longitude,
            TimestampUtc = timestampUtc,
            Pm10 = pm10,
            Pm25 = pm25
        };
    }

    // Sensor.Community always uses "." as decimal separator (checked in their API responses),
    // so parse with InvariantCulture instead of the machine's culture.
    // Float rejects "12,5" instead of reading it as 125.
    private static bool TryParseDouble(string value, out double result)
    {
        return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result);
    }
}
