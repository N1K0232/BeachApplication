using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using BeachApplication.Authentication;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Handlers;
using BeachApplication.Authentication.Requirements;
using BeachApplication.BusinessLayer.Services;
using BeachApplication.BusinessLayer.Services.Interfaces;
using BeachApplication.BusinessLayer.Settings;
using BeachApplication.BusinessLayer.StartupServices;
using BeachApplication.DataAccessLayer;
using BeachApplication.Extensions;
using BeachApplication.Handlers.Exceptions;
using BeachApplication.Handlers.Http;
using BeachApplication.StorageProviders.Extensions;
using BeachApplication.Swagger;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using MinimalHelpers.OpenApi;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;
using TinyHelpers.Extensions;
using TinyHelpers.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

ConfigureServices(builder.Services, builder.Configuration, builder.Host, builder.Environment);

var app = builder.Build();
Configure(app, app.Environment, app.Services);

await app.RunAsync();

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostBuilder host, IWebHostEnvironment environment)
{
    var appSettings = services.ConfigureAndGet<AppSettings>(configuration, nameof(AppSettings));
    var jwtSettings = services.ConfigureAndGet<JwtSettings>(configuration, nameof(JwtSettings));

    var sendinblueSettings = services.ConfigureAndGet<SendinblueSettings>(configuration, nameof(SendinblueSettings));
    var swaggerSettings = services.ConfigureAndGet<SwaggerSettings>(configuration, nameof(SwaggerSettings));

    var administratorUserSettingsSection = configuration.GetSection(nameof(AdministratorUserSettings));
    var powerUserSettingsSection = configuration.GetSection(nameof(PowerUserSettings));

    services.Configure<AdministratorUserSettings>(administratorUserSettingsSection);
    services.Configure<PowerUserSettings>(powerUserSettingsSection);

    services.AddRequestLocalization(appSettings.SupportedCultures);
    services.AddWebOptimizer(minifyCss: true, minifyJavaScript: environment.IsProduction());

    services.AddHttpContextAccessor();
    services.AddMemoryCache();

    services.AddExceptionHandler<DefaultExceptionHandler>();
    services.AddExceptionHandler<ApplicationExceptionHandler>();
    services.AddExceptionHandler<DbUpdateExceptionHandler>();

    services.AddRazorPages();

    services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        options.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

    services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            var statusCode = context.ProblemDetails.Status.GetValueOrDefault(StatusCodes.Status500InternalServerError);
            context.ProblemDetails.Type ??= $"https://httpstatuses.io/{statusCode}";
            context.ProblemDetails.Title ??= ReasonPhrases.GetReasonPhrase(statusCode);
            context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;
            context.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        };
    });

    services.AddOperationResult(options =>
    {
        options.ErrorResponseFormat = ErrorResponseFormat.List;
    });

    if (swaggerSettings.Enabled)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(swaggerSettings.Version, new OpenApiInfo
            {
                Title = swaggerSettings.Title,
                Version = swaggerSettings.Version
            });

            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Insert JWT token with the \"Bearer \" prefix",
                Name = HeaderNames.Authorization,
                Type = SecuritySchemeType.ApiKey
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });

            options.AddDefaultResponse();
            options.AddAcceptLanguageHeader();
            options.AddFormFile();
        });
    }

    services.AddResiliencePipeline("timeout", (builder, context) =>
    {
        builder.AddTimeout(new TimeoutStrategyOptions
        {
            Timeout = TimeSpan.FromSeconds(2),
            OnTimeout = args =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Timeout occurred after: {TotalSeconds} seconds", args.Timeout.TotalSeconds);

                return default;
            }
        });
    });

    services.AddResiliencePipeline<string, HttpResponseMessage>("http", (builder, context) =>
    {
        builder.AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        {
            MaxRetryAttempts = 3,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => r.StatusCode is HttpStatusCode.RequestTimeout or HttpStatusCode.TooManyRequests or >= HttpStatusCode.InternalServerError),
            DelayGenerator = args =>
            {
                if (args.Outcome.Result is not null && args.Outcome.Result.Headers.TryGetValues(HeaderNames.RetryAfter, out var value))
                {
                    return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(int.Parse(value.First())));
                }

                return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber + 1)));
            },
            OnRetry = args =>
            {
                var logger = context.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogInformation("Retrying... {AttemptNumber} attempt after {RetryDelay}", args.AttemptNumber + 1, args.RetryDelay);

                return ValueTask.CompletedTask;
            }
        });
    });

    services.AddTransient<TransientErrorDelegatingHandler>();
    services.AddHttpClient("http").AddHttpMessageHandler<TransientErrorDelegatingHandler>();
    services.AddFluentEmail(sendinblueSettings.EmailAddress).WithSendinblue();

    services.AddSqlServer<ApplicationDbContext>(configuration.GetConnectionString("SqlConnection"));
    services.AddScoped<IApplicationDbContext>(services => services.GetRequiredService<ApplicationDbContext>());

    services.AddSqlServer<AuthenticationDbContext>(configuration.GetConnectionString("SqlConnection"));
    services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
        options.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    })
    .AddEntityFrameworkStores<AuthenticationDbContext>()
    .AddTokenProvider<EmailTokenProvider<ApplicationUser>>("emailconfirmation")
    .AddDefaultTokenProviders();

    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecurityKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    services.AddScoped<IAuthorizationHandler, UserActiveHandler>();
    services.AddAuthorization(options =>
    {
        var policyBuilder = new AuthorizationPolicyBuilder().RequireAuthenticatedUser();
        policyBuilder.Requirements.Add(new UserActiveRequirement());

        options.DefaultPolicy = policyBuilder.Build();

        options.AddPolicy("Administrator", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(RoleNames.Administrator, RoleNames.PowerUser);
            policy.Requirements.Add(new UserActiveRequirement());
        });

        options.AddPolicy("UserActive", policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(RoleNames.User);
            policy.Requirements.Add(new UserActiveRequirement());
        });
    });

    services.AddHangfire(options =>
    {
        options.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("SqlConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            });
    });

    services.AddHangfireServer();

    var azureStorageConnectionString = configuration.GetConnectionString("AzureStorageConnection");
    if (azureStorageConnectionString.HasValue() && appSettings.ContainerName.HasValue())
    {
        services.AddAzureStorage(options =>
        {
            options.ConnectionString = azureStorageConnectionString;
            options.ContainerName = appSettings.ContainerName;
        });
    }
    else
    {
        services.AddFileSystemStorage(options =>
        {
            options.StorageFolder = appSettings.StorageFolder;
        });
    }

    services.AddScoped<IIdentityService, IdentityService>();
    services.AddScoped<IMeService, MeService>();

    services.AddHostedService<IdentityRoleService>();
    services.AddHostedService<IdentityUserService>();
}

void Configure(IApplicationBuilder app, IWebHostEnvironment environment, IServiceProvider services)
{
    var appSettings = services.GetRequiredService<IOptions<AppSettings>>().Value;
    var swaggerSettings = services.GetRequiredService<IOptions<SwaggerSettings>>().Value;

    environment.ApplicationName = appSettings.ApplicationName;

    app.UseHttpsRedirection();
    app.UseRequestLocalization();

    app.UseRouting();
    app.UseWebOptimizer();

    app.UseWhen(context => context.IsWebRequest(), builder =>
    {
        if (!environment.IsDevelopment())
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

    app.UseHangfireDashboard("/jobs");
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapEndpoints();
        endpoints.MapRazorPages();
    });
}