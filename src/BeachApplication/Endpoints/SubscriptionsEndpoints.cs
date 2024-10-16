using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.Shared.Models;
using BeachApplication.Shared.Models.Requests;
using MinimalHelpers.Routing;
using OperationResults;
using OperationResults.AspNetCore.Http;

namespace BeachApplication.Endpoints;

public class SubscriptionsEndpoints : IEndpointRouteHandlerBuilder
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var subscriptionsApiGroup = endpoints.MapGroup("/api/subscriptions");

        subscriptionsApiGroup.MapDelete("{id:guid}", DeleteAsync)
            .RequireAuthorization("admin", "poweruser", "user")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithOpenApi();

        subscriptionsApiGroup.MapGet("{id:guid}", GetAsync)
            .RequireAuthorization("admin", "poweruser", "user")
            .Produces<Subscription>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("GetSubscription")
            .WithOpenApi();

        subscriptionsApiGroup.MapGet(string.Empty, GetListAsync)
            .RequireAuthorization("admin", "poweruser", "user")
            .Produces<PaginatedList<Subscription>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .WithOpenApi();

        subscriptionsApiGroup.MapPost(string.Empty, InsertAsync)
            .RequireAuthorization("user")
            .Produces<Subscription>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem()
            .WithOpenApi();

        subscriptionsApiGroup.MapPut("{id:guid}", UpdateAsync)
            .RequireAuthorization("user")
            .Produces<Subscription>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .WithOpenApi();
    }

    private static async Task<IResult> DeleteAsync(ISubscriptionService subscriptionService, Guid id, HttpContext httpContext)
    {
        var result = await subscriptionService.DeleteAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetAsync(ISubscriptionService subscriptionService, Guid id, HttpContext httpContext)
    {
        var result = await subscriptionService.GetAsync(id);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> GetListAsync(ISubscriptionService subscriptionService, HttpContext httpContext, string userName = null)
    {
        var result = await subscriptionService.GetListAsync(userName);
        return httpContext.CreateResponse(result);
    }

    private static async Task<IResult> InsertAsync(ISubscriptionService subscriptionService, SaveSubscriptionRequest request, HttpContext httpContext)
    {
        var result = await subscriptionService.InsertAsync(request);
        return httpContext.CreateResponse(result, "GetSubscription", new { id = result.Content?.Id });
    }

    private static async Task<IResult> UpdateAsync(ISubscriptionService subscriptionService, Guid id, SaveSubscriptionRequest request, HttpContext httpContext)
    {
        var result = await subscriptionService.UpdateAsync(id, request);
        return httpContext.CreateResponse(result);
    }
}