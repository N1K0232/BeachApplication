using System.Text.Json.Serialization;

namespace BeachApplication.Shared.Models.OpenWeatherMap;

public class Clouds
{
    [JsonPropertyName("all")]
    public int Cloudiness { get; set; }
}