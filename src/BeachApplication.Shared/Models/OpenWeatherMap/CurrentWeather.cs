﻿using System.Text.Json.Serialization;
using BeachApplication.Shared.Converters;

namespace BeachApplication.Shared.Models.OpenWeatherMap;

public class CurrentWeather
{
    [JsonPropertyName("coord")]
    public Position? Position { get; set; }

    [JsonPropertyName("weather")]
    public IEnumerable<WeatherCondition>? Conditions { get; set; }

    [JsonPropertyName("main")]
    public CurrentWeatherDetail? Detail { get; set; }

    [JsonPropertyName("visibility")]
    public int Visibility { get; set; }

    [JsonPropertyName("wind")]
    public Wind? Wind { get; set; }

    [JsonPropertyName("clouds")]
    public Clouds? Clouds { get; set; }

    [JsonPropertyName("sys")]
    public Sun? Sun { get; set; }

    [JsonPropertyName("dt")]
    [JsonConverter(typeof(UnixToDateTimeConverter))]
    public DateTime Date { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("cod")]
    public int Code { get; set; }
}