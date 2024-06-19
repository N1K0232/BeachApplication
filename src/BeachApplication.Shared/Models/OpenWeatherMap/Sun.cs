using System.Text.Json.Serialization;
using BeachApplication.Shared.Converters;

namespace BeachApplication.Shared.Models.OpenWeatherMap;

public class Sun
{
    [JsonPropertyName("country")]
    public string Country { get; set; } = null!;

    [JsonConverter(typeof(UnixToDateTimeConverter))]
    [JsonPropertyName("sunrise")]
    public DateTime Sunrise { get; set; }

    [JsonConverter(typeof(UnixToDateTimeConverter))]
    [JsonPropertyName("sunset")]
    public DateTime Sunset { get; set; }
}