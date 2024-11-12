using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using BeachApplication.Authentication;
using BeachApplication.Authentication.DataProtection;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Extensions;
using BeachApplication.Authorization;
using BeachApplication.BusinessLayer.BackgroundServices;
using BeachApplication.BusinessLayer.Mapping;
using BeachApplication.BusinessLayer.Services;
using BeachApplication.BusinessLayer.Settings;
using BeachApplication.BusinessLayer.StartupServices;
using BeachApplication.BusinessLayer.Validations;
using BeachApplication.Clients.Extensions;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.Extensions;
using BeachApplication.Services;
using BeachApplication.StorageProviders.Extensions;
using BeachApplication.Swagger;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using MinimalHelpers.Routing;
using MinimalHelpers.Validation;
using OperationResults.AspNetCore.Http;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using QRCoder;
using Quartz;
using Quartz.AspNetCore;
using Serilog;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;
using TinyHelpers.Extensions;
using TinyHelpers.Json.Serialization;
using ResultErrorResponseFormat = OperationResults.AspNetCore.Http.ErrorResponseFormat;
using ValidationErrorResponseFormat = MinimalHelpers.Validation.ErrorResponseFormat;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

builder.Host.UseSerilog((hostingContext, loggerConfiguration) =>
{
    loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
});

var settings = builder.Services.ConfigureAndGet<AppSettings>(builder.Configuration, nameof(AppSettings));
var swagger = builder.Services.ConfigureAndGet<SwaggerSettings>(builder.Configuration, nameof(SwaggerSettings));

builder.Services.AddRazorPages();
builder.Services.AddRouting();

builder.Services.AddDefaultExceptionHandler();
builder.Services.AddDefaultProblemDetails();

builder.Services.AddRequestLocalization(settings.SupportedCultures);
builder.Services.AddHttpContextAccessor();

builder.Services.AddWebOptimizer(minifyCss: true, minifyJavaScript: builder.Environment.IsProduction());
builder.Services.AddDataProtection().SetApplicationName(settings.ApplicationName).PersistKeysToDbContext<ApplicationDbContext>();

builder.Services.AddScoped<IDataProtectionService, DataProtectionService>();
builder.Services.AddScoped(services =>
{
    var dataProtectionProvider = services.GetRequiredService<IDataProtectionProvider>();
    var dataProtector = dataProtectionProvider.CreateProtector(settings.ApplicationName);

    return dataProtector.ToTimeLimitedDataProtector();
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    options.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(_ =>
    {
        var tokenOptions = new TokenBucketRateLimiterOptions
        {
            TokenLimit = 500,
            TokensPerPeriod = 50,
            ReplenishmentPeriod = TimeSpan.FromHours(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        };

        return RateLimitPartition.GetTokenBucketLimiter("Default", _ => tokenOptions);
    });

    options.OnRejected = (context, token) =>
    {
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var window))
        {
            context.HttpContext.Response.Headers.RetryAfter = window.TotalSeconds.ToString();
        }

        return ValueTask.CompletedTask;
    };
});

builder.Services.AddAutoMapper(typeof(ImageMapperProfile).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<SaveCategoryRequestValidator>();

builder.Services.AddFluentValidationAutoValidation(options =>
{
    options.DisableDataAnnotationsValidation = true;
});

builder.Services.AddOperationResult(options =>
{
    options.ErrorResponseFormat = ResultErrorResponseFormat.List;
});

builder.Services.ConfigureValidation(options =>
{
    options.ErrorResponseFormat = ValidationErrorResponseFormat.List;
});

builder.Services.AddAuthentication(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
    policyBuilder.Requirements.Add(new UserActiveRequirement());

    var policy = policyBuilder.Build();
    options.DefaultPolicy = policy;

    options.AddPolicy("UserActive", policy =>
    {
        policy.Requirements.Add(new UserActiveRequirement());
        policy.RequireRole(RoleNames.User);
    });

    options.AddPolicy("Administrator", policy =>
    {
        policy.RequireRole(RoleNames.Administrator, RoleNames.PowerUser);
        policy.Requirements.Add(new UserActiveRequirement());
    });
});

if (swagger.Enabled)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Beach Api", Version = "v1" });
        options.AddAuthentication();

        options.AddDefaultResponse();
        options.AddAcceptLanguageHeader();
    })
    .AddFluentValidationRulesToSwagger(options =>
    {
        options.SetNotNullableIfMinLengthGreaterThenZero = true;
    });
}

builder.Services.AddHangfireServer();
builder.Services.AddHangfire(options =>
{
    var storageOptions = new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    };

    var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireConnection");

    options.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(hangfireConnectionString, storageOptions);
});

builder.Services.AddScoped(_ => new QRCodeGenerator());
builder.Services.AddScoped<IQRCodeGeneratorService, QRCodeGeneratorService>();

builder.Services.AddChatServer(builder.Configuration);
builder.Services.AddEmailSender(builder.Configuration);
builder.Services.AddMessageSender(builder.Configuration);

builder.Services.AddResiliencePipeline("timeout", (builder, context) =>
{
    builder.AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(2),
        OnTimeout = args =>
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Timeout occurred after: {TotalSeconds} seconds", args.Timeout.TotalSeconds);

            return ValueTask.CompletedTask;
        }
    });
});

builder.Services.AddResiliencePipeline<string, HttpResponseMessage>("http", (builder, context) =>
{
    builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
    {
        MaxRetryAttempts = 3,
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<HttpRequestException>()
        .HandleResult(r => r.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError),
        DelayGenerator = args =>
        {
            var result = args.Outcome.Result;
            var retryAfter = HeaderNames.RetryAfter;
            double seconds;

            if (result is not null && result.Headers.TryGetValues(retryAfter, out var value))
            {
                seconds = double.Parse(value.First());
            }
            else
            {
                seconds = Math.Pow(2, args.AttemptNumber + 1);
            }

            var delay = TimeSpan.FromSeconds(seconds);
            return new ValueTask<TimeSpan?>(delay);
        },
        OnRetry = args =>
        {
            var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Retrying... {AttemptNumber} attempt after {RetryDelay}", args.AttemptNumber + 1, args.RetryDelay);

            return ValueTask.CompletedTask;
        }
    });
});

builder.Services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>("database");
builder.Services.AddDbContext<IApplicationDbContext, ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(2), null);
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

var azureStorageConnectionString = builder.Configuration.GetConnectionString("AzureStorageConnection");
if (azureStorageConnectionString.HasValue())
{
    builder.Services.AddAzureStorage(options =>
    {
        options.ConnectionString = azureStorageConnectionString;
        options.ContainerName = settings.StorageFolder;
    });
}
else
{
    builder.Services.AddFileSystemStorage(options =>
    {
        options.StorageFolder = settings.StorageFolder;
    });
}

builder.Services.Scan(scan => scan.FromAssemblyOf<IdentityService>()
    .AddClasses(classes => classes.InNamespaceOf<IdentityService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

builder.Services.AddScoped<IUserService, HttpUserService>();
builder.Services.AddScoped<IAuthorizationHandler, UserActiveHandler>();

builder.Services.AddHostedService<DatabaseService>();
builder.Services.AddHostedService<IdentityRoleService>();
builder.Services.AddHostedService<IdentityUserService>();

builder.Services.AddQuartzHostedService(options =>
{
    options.StartDelay = TimeSpan.Zero;
    options.AwaitApplicationStarted = true;
    options.WaitForJobsToComplete = true;
});

builder.Services.AddQuartz(options =>
{
    options.AddScheduledJob<OrdersManagerBackgroundJob>("0 0 0 * * ?");
    options.AddScheduledJob<ProductsManagerBackgroundJob>("0 30 0 ? * * *");
});

builder.Services.AddQuartzServer();

var app = builder.Build();
app.Environment.ApplicationName = settings.ApplicationName;

app.UseHttpsRedirection();
app.UseRequestLocalization();

app.UseRouting();
app.UseWebOptimizer();

app.UseWhen(context => context.IsWebRequest(), builder =>
{
    if (!app.Environment.IsDevelopment())
    {
        builder.UseExceptionHandler("/Errors/500");
        builder.UseHsts();
    }

    builder.UseStatusCodePagesWithReExecute("/Errors/{0}");
});

app.UseWhen(context => context.IsApiRequest(), builder =>
{
    builder.UseExceptionHandler();
    builder.UseStatusCodePages();
    builder.UseRateLimiter();
});

app.UseDefaultFiles();
app.UseStaticFiles();

if (swagger.Enabled)
{
    app.UseMiddleware<SwaggerBasicAuthenticationMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Beach Api v1");
        options.InjectStylesheet("/css/swagger.css");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/jobs");
app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
});

app.MapRazorPages();
app.MapEndpoints();

app.MapHealthChecks("/healthchecks", new HealthCheckOptions
{
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status400BadRequest
    },
    ResponseWriter = async (context, report) =>
    {
        var result = JsonSerializer.Serialize(
        new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.TotalMilliseconds,
            details = report.Entries.Select(entry => new
            {
                service = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                exception = entry.Value.Exception?.Message,
            })
        });

        context.Response.ContentType = MediaTypeNames.Application.Json;
        await context.Response.WriteAsync(result);
    }
});

await app.RunAsync();