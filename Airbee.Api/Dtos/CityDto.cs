namespace Airbee.Api.Dtos;

public record CityDto
{
    public string Name { get; init; } = string.Empty;
    public int SensorCount { get; init; }
}
