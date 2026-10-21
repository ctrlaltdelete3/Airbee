using System.Globalization;
using System.Net.Http.Json;
using Airbee.Collector.SensorCommunity.Dtos;
using Microsoft.Extensions.Options;

namespace Airbee.Collector.SensorCommunity;

public class SensorCommunityClient(HttpClient httpClient, IOptions<SensorCommunityOptions> options)
{
    private readonly AreaOptions _area = options.Value.Area;

    public async Task<List<SensorCommunityMeasurementDto>> GetMeasurementsAsync(CancellationToken cancellationToken)
    {
        // Invariant culture, otherwise on a hr-HR machine 45.55 is formatted as "45,55".
        var url = string.Create(CultureInfo.InvariantCulture,
            $"filter/area={_area.Latitude},{_area.Longitude},{_area.RadiusKm}");

        var measurements = await httpClient.GetFromJsonAsync<List<SensorCommunityMeasurementDto>>(url, cancellationToken);

        return measurements ?? [];
    }
}
