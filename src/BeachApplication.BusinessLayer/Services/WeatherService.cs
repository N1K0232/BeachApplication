using BeachApplication.BusinessLayer.Clients.Refit;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.OpenWeatherMap;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services;

public class WeatherService(IOpenWeatherMapClient openWeatherMapClient) : IWeatherService
{
    public async Task<Result<Weather>> SearchAsync(string city)
    {
        var response = await openWeatherMapClient.SearchAsync(city);
        await response.EnsureSuccessStatusCodeAsync();

        if (response.IsSuccessStatusCode)
        {
            var weather = new Weather(response.Content);
            return weather;
        }

        var error = await response.Error.GetContentAsAsync<Error>();
        return Result.Fail(FailureReasons.ClientError, error!.Code, error!.Message);
    }
}