using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace Airbee.Collector.SensorCommunity;

public class SensorCommunityOptions
{
    public const string SectionName = "SensorCommunity";

    [Required]
    public Uri BaseUrl { get; set; } = null!;

    // Sensor.Community rejects requests without a User-Agent header.
    [Required]
    public string UserAgent { get; set; } = string.Empty;

    [MinLength(1)]
    [ValidateEnumeratedItems]
    public List<AreaOptions> Areas { get; set; } = [];
}
