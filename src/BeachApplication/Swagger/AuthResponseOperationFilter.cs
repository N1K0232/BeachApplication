using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BeachApplication.Swagger;

public class AuthResponseOperationFilter(IAuthorizationPolicyProvider authorizationPolicyProvider) : IOperationAsyncFilter
{
    public async Task ApplyAsync(OpenApiOperation operation, OperationFilterContext context, CancellationToken cancellationToken)
    {
        var defaultPolicy = await authorizationPolicyProvider.GetDefaultPolicyAsync();
        var requireAuthenticatedUser = defaultPolicy?.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement) ?? false;

        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        var requireAuthorization = endpointMetadata.Any(m => m is AuthorizeAttribute);
        var allowAnonymous = endpointMetadata.Any(m => m is AllowAnonymousAttribute);

        if ((requireAuthenticatedUser || requireAuthorization) && !allowAnonymous)
        {
            operation.Responses.TryAdd(StatusCodes.Status401Unauthorized.ToString(), CreateResponse(HttpStatusCode.Unauthorized.ToString()));
            operation.Responses.TryAdd(StatusCodes.Status403Forbidden.ToString(), CreateResponse(HttpStatusCode.Forbidden.ToString()));
        }
    }

    private static OpenApiResponse CreateResponse(string description)
    {
        var response = new OpenApiResponse()
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new()
                {
                    Schema = new()
                    {
                        Reference = new()
                        {
                            Type = ReferenceType.Schema,
                            Id = nameof(ProblemDetails)
                        }
                    }
                }
            }
        };

        return response;
    }
}