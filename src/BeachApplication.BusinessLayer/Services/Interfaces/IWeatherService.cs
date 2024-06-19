using BeachApplication.Shared.Models;
using OperationResults;

namespace BeachApplication.BusinessLayer.Services.Interfaces;

public interface IWeatherService
{
    Task<Result<Weather>> SearchAsync(string city);
}