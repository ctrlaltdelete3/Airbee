namespace Airbee.Collector.SensorCommunity.Dtos;

public class LocationDto
{
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;

    // if value == 1 - that means the sensor is located indoors, if value == 0 - that means the sensor is located outdoors
    // we will skip the indoor sensors for now, as they are not relevant for our use case
    public int Indoor { get; set; }
}
