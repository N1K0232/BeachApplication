using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BeachApplication.Handlers.Exceptions;

public class SqlExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService problemDetailsService;
    private readonly IWebHostEnvironment environment;

    public SqlExceptionHandler(IProblemDetailsService problemDetailsService, IWebHostEnvironment environment)
    {
        this.problemDetailsService = problemDetailsService;
        this.environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is SqlException sqlException)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = sqlException.GetType().FullName,
                Detail = sqlException.Message,
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            if (environment.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            await problemDetailsService.WriteAsync(new()
            {
                HttpContext = httpContext,
                AdditionalMetadata = httpContext.Features.Get<IExceptionHandlerFeature>()?.Endpoint?.Metadata,
                ProblemDetails = problemDetails,
                Exception = exception
            });

            return true;
        }

        return false;
    }
}