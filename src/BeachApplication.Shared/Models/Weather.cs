using BeachApplication.Shared.Models.OpenWeatherMap;

namespace BeachApplication.Shared.Models;

public class Weather(CurrentWeather currentWeather)
{
    public string CityName => currentWeather.Name;

    public string? Condition => currentWeather.Conditions?.First().Description;

    public string? ConditionIcon => currentWeather.Conditions?.First().ConditionIcon;

    public string? ConditionIconUrl => $"https://openweathermap.org/img/w/{ConditionIcon}.png";

    public decimal Temperature => currentWeather.Detail?.Temperature ?? 0M;
}