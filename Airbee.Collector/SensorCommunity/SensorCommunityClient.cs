using System.Globalization;
using System.Net.Http.Json;
using Airbee.Collector.SensorCommunity.Dtos;

namespace Airbee.Collector.SensorCommunity;

public class SensorCommunityClient(HttpClient httpClient)
{
    public async Task<List<SensorCommunityMeasurementDto>> GetMeasurementsAsync(AreaOptions area,
        CancellationToken cancellationToken)
    {
        // Invariant culture, otherwise on a hr-HR machine 45.55 is formatted as "45,55".
        var url = string.Create(CultureInfo.InvariantCulture,
            $"filter/area={area.Latitude},{area.Longitude},{area.RadiusKm}");

        var measurements = await httpClient.GetFromJsonAsync<List<SensorCommunityMeasurementDto>>(url, cancellationToken);

        return measurements ?? [];
    }
}
