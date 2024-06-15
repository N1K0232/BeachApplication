using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeachApplication.Handlers.Exceptions;

public class DbUpdateExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService problemDetailsService;
    private readonly IWebHostEnvironment environment;

    public DbUpdateExceptionHandler(IProblemDetailsService problemDetailsService, IWebHostEnvironment environment)
    {
        this.problemDetailsService = problemDetailsService;
        this.environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is DbUpdateException dbException)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status502BadGateway,
                Title = dbException.GetType().FullName,
                Detail = dbException.Message,
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