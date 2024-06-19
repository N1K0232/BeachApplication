﻿using System.Text.Json.Serialization;

namespace BeachApplication.Shared.Models.OpenWeatherMap;

public class ForecastCity
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;

    [JsonPropertyName("coord")]
    public Position? Position { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; } = null!;
}