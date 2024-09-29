using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class WeatherForecastEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var weatherForecastApiGroup = endpoints.MapGroup("/api/weatherforecast");

        weatherForecastApiGroup.MapGet("{city}", SearchAsync)
            .RequireAuthorization("UserActive")
            .Produces<Weather>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();
    }

    private static async Task<IResult> SearchAsync(IWeatherService weatherService, HttpContext httpContext, [FromQuery(Name = "q")] string city)
    {
        var result = await weatherService.SearchAsync(city);
        return httpContext.CreateResponse(result);
    }
}