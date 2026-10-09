using System.ComponentModel.DataAnnotations;

namespace Airbee.Collector.SensorCommunity;

public class AreaOptions
{
    // Nullable so a missing value fails validation instead of silently becoming 0 (a real spot in the ocean).
    [Required]
    [Range(-90, 90)]
    public double? Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double? Longitude { get; set; }

    [Range(1, 100)]
    public double RadiusKm { get; set; }
}
