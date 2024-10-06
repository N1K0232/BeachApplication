using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using BeachApplication.BusinessLayer.Clients;
using BeachApplication.BusinessLayer.Clients.Interfaces;
using BeachApplication.BusinessLayer.Clients.Refit;
using BeachApplication.BusinessLayer.Core;
using BeachApplication.BusinessLayer.Diagnostics.BackgroundJobs;
using BeachApplication.BusinessLayer.Diagnostics.HealthChecks;
using BeachApplication.BusinessLayer.Mapping;
using BeachApplication.BusinessLayer.Providers;
using BeachApplication.BusinessLayer.Providers.Interfaces;
using BeachApplication.BusinessLayer.Services;
using BeachApplication.BusinessLayer.Settings;
using BeachApplication.BusinessLayer.StartupServices;
using BeachApplication.BusinessLayer.Validations;
using BeachApplication.Contracts;
using BeachApplication.DataAccessLayer;
using BeachApplication.DataAccessLayer.Authorization;
using BeachApplication.DataAccessLayer.DataProtection;
using BeachApplication.DataAccessLayer.Entities.Identity;
using BeachApplication.DataAccessLayer.Extensions;
using BeachApplication.Extensions;
using BeachApplication.Handlers;
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
using Microsoft.AspNetCore.RateLimiting;
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
using Refit;
using Serilog;
using SimpleAuthentication;
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

var appSettings = builder.Services.ConfigureAndGet<AppSettings>(builder.Configuration, nameof(AppSettings));
var emailSettings = builder.Services.ConfigureAndGet<SendinblueSettings>(builder.Configuration, nameof(SendinblueSettings));
var swaggerSettings = builder.Services.ConfigureAndGet<SwaggerSettings>(builder.Configuration, nameof(SwaggerSettings));

var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
var azureStorageConnectionString = builder.Configuration.GetConnectionString("AzureStorageConnection");

var translatorSettingsSection = builder.Configuration.GetSection(nameof(TranslatorSettings));
builder.Services.Configure<TranslatorSettings>(translatorSettingsSection);

var openWeatherMapSettingsSection = builder.Configuration.GetSection(nameof(OpenWeatherMapSettings));
var openWeatherMapSettings = openWeatherMapSettingsSection.Get<OpenWeatherMapSettings>();

builder.Services.AddRequestLocalization(appSettings.SupportedCultures);
builder.Services.AddWebOptimizer(minifyCss: true, minifyJavaScript: builder.Environment.IsProduction());

builder.Services.AddDefaultExceptionHandler();
builder.Services.AddDefaultProblemDetails();

builder.Services.AddHttpContextAccessor();
builder.Services.AddRouting();

builder.Services.AddMemoryCache();
builder.Services.AddSimpleAuthentication(builder.Configuration);

builder.Services.AddRazorPages();
builder.Services.AddHealthChecks().AddCheck<SqlConnectionHealthCheck>("sql")
    .AddDbContextCheck<ApplicationDbContext>("database")
    .AddDbContextCheck<AuthenticationDbContext>("identity");

builder.Services.AddDataProtection().SetApplicationName(appSettings.ApplicationName).PersistKeysToDbContext<ApplicationDbContext>();
builder.Services.AddScoped<IDataProtectionService, DataProtectionService>();
builder.Services.AddScoped<ITimeLimitedDataProtectionService, TimeLimitedDataProtectionService>();

builder.Services.AddScoped(services =>
{
    var dataProtectionProvider = services.GetDataProtectionProvider();
    return dataProtectionProvider.CreateProtector(appSettings.ApplicationName);
});

builder.Services.AddScoped(services =>
{
    var dataProtector = services.GetDataProtector(appSettings.ApplicationName);
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
    options.AddFixedWindowLimiter("beachapplication", options =>
    {
        options.PermitLimit = 5;
        options.Window = TimeSpan.FromSeconds(30);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    });
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

if (swaggerSettings.Enabled)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc(swaggerSettings.Version, new OpenApiInfo
        {
            Title = swaggerSettings.Title,
            Version = swaggerSettings.Version
        });

        options.AddDefaultResponse();
        options.AddAcceptLanguageHeader();
        options.AddSimpleAuthentication(builder.Configuration);
    })
    .AddFluentValidationRulesToSwagger(options =>
    {
        options.SetNotNullableIfMinLengthGreaterThenZero = true;
    });
}

builder.Services.AddScoped(_ => new QRCodeGenerator());
builder.Services.AddScoped<IQrCodeGeneratorService, QrCodeGeneratorService>();

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

builder.Services.AddTransient<TransientErrorDelegatingHandler>();
builder.Services.AddHttpClient("http").AddHttpMessageHandler<TransientErrorDelegatingHandler>();

builder.Services.AddFluentEmail(emailSettings.EmailAddress).WithSendinblue();
builder.Services.AddRefitClient<IOpenWeatherMapClient>().ConfigureHttpClient(httpClient =>
{
    httpClient.BaseAddress = new Uri(openWeatherMapSettings.ServiceUrl);
})
.ConfigurePrimaryHttpMessageHandler(_ =>
{
    var handler = new QueryStringInjectorHttpMessageHandler();
    handler.Parameters.Add("units", "metric");
    handler.Parameters.Add("lang", "IT");
    handler.Parameters.Add("APPID", openWeatherMapSettings.ApiKey);

    return handler;
});

builder.Services.AddSqlServerCaching(connectionString);
builder.Services.AddSqlServerContext(connectionString);

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

builder.Services.AddScoped<IAuthorizationHandler, UserActiveHandler>();
builder.Services.AddScoped<IUserService, HttpUserService>();

builder.Services.AddAuthorization(options =>
{
    var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
    policyBuilder.Requirements.Add(new UserActiveRequirement());

    options.DefaultPolicy = policyBuilder.Build();

    options.AddPolicy("Administrator", policy =>
    {
        policy.RequireAuthenticatedUser().RequireRole(RoleNames.Administrator);
        policy.Requirements.Add(new UserActiveRequirement());
    });

    options.AddPolicy("PowerUser", policy =>
    {
        policy.RequireAuthenticatedUser().RequireRole(RoleNames.PowerUser);
        policy.Requirements.Add(new UserActiveRequirement());
    });

    options.AddPolicy("UserActive", policy =>
    {
        policy.RequireAuthenticatedUser().RequireRole(RoleNames.User);
        policy.Requirements.Add(new UserActiveRequirement());
    });
});

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

    options.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, storageOptions);
});

builder.Services.AddHangfireServer();

if (azureStorageConnectionString.HasValue() && appSettings.ContainerName.HasValue())
{
    builder.Services.AddAzureStorage(options =>
    {
        options.ConnectionString = azureStorageConnectionString;
        options.ContainerName = appSettings.ContainerName;
    });
}
else
{
    builder.Services.AddFileSystemStorage(options =>
    {
        options.StorageFolder = appSettings.StorageFolder;
    });
}

builder.Services.AddHttpClient<IAzureTokenProvider, AzureTokenProvider>();
builder.Services.AddHttpClient<ITranslatorClient, TranslatorClient>();

builder.Services.Scan(scan => scan.FromAssemblyOf<OrderService>()
    .AddClasses(classes => classes.InNamespaceOf<OrderService>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

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
    var ordersManagerBackgroundJobKey = nameof(OrdersManagerBackgroundJob);
    options.AddJob<OrdersManagerBackgroundJob>(jobOptions => jobOptions.WithIdentity(ordersManagerBackgroundJobKey));

    options.AddTrigger(triggerOptions =>
    {
        triggerOptions.ForJob(ordersManagerBackgroundJobKey)
            .WithIdentity($"{ordersManagerBackgroundJobKey}-trigger")
            .WithCronSchedule("0 0 0 * * ?");
    });
});

builder.Services.AddQuartzServer();

var app = builder.Build();
app.Environment.ApplicationName = appSettings.ApplicationName;

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

if (swaggerSettings.Enabled)
{
    app.UseMiddleware<SwaggerBasicAuthenticationMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{swaggerSettings.Title} {swaggerSettings.Version}");
        options.InjectStylesheet("/css/swagger.css");
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.UseSerilogRequestLogging(options =>
{
    options.IncludeQueryInRequestPath = true;
});

app.UseHangfireDashboard("/jobs");

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