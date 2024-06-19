using BeachApplication.Shared.Models.OpenWeatherMap;
using Refit;

namespace BeachApplication.BusinessLayer.Clients.Refit;

public interface IOpenWeatherMapClient
{
    [Get("/weather?units=metric")]
    Task<ApiResponse<CurrentWeather>> SearchAsync([AliasAs("q")] string city);
}