using System.Text.Json.Serialization;

namespace BeachApplication.Shared.Models.OpenWeatherMap;

public class Error
{
    [JsonPropertyName("cod")]
    public string Code { get; set; } = null!;

    public string Message { get; set; } = null!;
}