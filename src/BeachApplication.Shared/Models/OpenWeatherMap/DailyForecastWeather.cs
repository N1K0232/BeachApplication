using System.Text.Json.Serialization;

namespace BeachApplication.Shared.Models.OpenWeatherMap;

public class DailyForecastWeather
{
    [JsonPropertyName("city")]
    public ForecastCity? City { get; set; }

    [JsonPropertyName("cod")]
    public string Code { get; set; } = null!;

    [JsonPropertyName("list")]
    public IEnumerable<DailyForecastWeatherData>? WeatherData { get; set; }
}