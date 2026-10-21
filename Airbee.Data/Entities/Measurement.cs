namespace Airbee.Data.Entities;

public class Measurement
{
    public long Id { get; set; }

    public long SensorId { get; set; }
    public Sensor Sensor { get; set; } = null!;

    public DateTime TimestampUtc { get; set; }

    // µg/m³
    public double Pm10 { get; set; }
    public double Pm25 { get; set; }
}
