﻿using System.Diagnostics;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using BeachApplication.Authentication;
using BeachApplication.Authentication.Entities;
using BeachApplication.Authentication.Handlers;
using BeachApplication.Authentication.Requirements;
using BeachApplication.BusinessLayer.Clients;
using BeachApplication.BusinessLayer.Clients.Interfaces;
using BeachApplication.BusinessLayer.Clients.Refit;
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
using BeachApplication.DataAccessLayer.Caching;
using BeachApplication.DataAccessLayer.Settings;
using BeachApplication.DataProtectionLayer;
using BeachApplication.DataProtectionLayer.Services;
using BeachApplication.Extensions;
using BeachApplication.Handlers.Exceptions;
using BeachApplication.Handlers.Http;
using BeachApplication.Services;
using BeachApplication.StorageProviders.Extensions;
using BeachApplication.Swagger;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using MinimalHelpers.Routing;
using OperationResults.AspNetCore.Http;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Quartz;
using Quartz.AspNetCore;
using Refit;
using Serilog;
using TinyHelpers.AspNetCore.Extensions;
using TinyHelpers.AspNetCore.Swagger;
using TinyHelpers.Extensions;
using TinyHelpers.Json.Serialization;

namespace BeachApplication;

public class Startup(IConfiguration configuration, IWebHostEnvironment environment)
{
    public void ConfigureServices(IServiceCollection services)
    {
        var appSettings = services.ConfigureAndGet<AppSettings>(configuration, nameof(AppSettings))!;
        var jwtSettings = services.ConfigureAndGet<JwtSettings>(configuration, nameof(JwtSettings))!;

        var sendinblueSettings = services.ConfigureAndGet<SendinblueSettings>(configuration, nameof(SendinblueSettings))!;
        var swaggerSettings = services.ConfigureAndGet<SwaggerSettings>(configuration, nameof(SwaggerSettings))!;

        var sqlConnectionString = configuration.GetConnectionString("SqlConnection");
        var azureStorageConnectionString = configuration.GetConnectionString("AzureStorageConnection");

        var translatorSettingsSection = configuration.GetSection(nameof(TranslatorSettings));
        services.Configure<TranslatorSettings>(translatorSettingsSection);

        var dataContextSettingsSection = configuration.GetSection(nameof(DataContextSettings));
        var dataContextSettings = dataContextSettingsSection.Get<DataContextSettings>()!;

        var openWeatherMapSettingsSection = configuration.GetSection(nameof(OpenWeatherMapSettings));
        var openWeatherMapSettings = openWeatherMapSettingsSection.Get<OpenWeatherMapSettings>()!;

        services.AddRequestLocalization(appSettings.SupportedCultures);
        services.AddWebOptimizer(minifyCss: true, minifyJavaScript: environment.IsProduction());

        services.AddHttpContextAccessor();
        services.AddMemoryCache();

        services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = sqlConnectionString;
            options.TableName = "CacheStore";
            options.SchemaName = "dbo";
            options.DefaultSlidingExpiration = TimeSpan.FromDays(1);
        });

        services.AddExceptionHandler<DefaultExceptionHandler>();
        services.AddExceptionHandler<ApplicationExceptionHandler>();
        services.AddExceptionHandler<DbUpdateExceptionHandler>();

        services.AddRazorPages();
        services.AddHealthChecks().AddCheck<SqlConnectionHealthCheck>("sql")
            .AddDbContextCheck<ApplicationDbContext>("database")
            .AddDbContextCheck<AuthenticationDbContext>("identity")
            .AddDbContextCheck<DataProtectionDbContext>("dataprotection");

        services.AddDataProtection().PersistKeysToDbContext<DataProtectionDbContext>();
        services.AddScoped<IDataProtectionService, DataProtectionService>();

        services.AddScoped(services =>
        {
            var dataProtectionProvider = services.GetRequiredService<IDataProtectionProvider>();
            var protector = dataProtectionProvider.CreateProtector("beachapplication");

            return protector;
        });

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            options.SerializerOptions.Converters.Add(new UtcDateTimeConverter());
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddRateLimiter(options =>
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

        services.AddAutoMapper(typeof(ImageMapperProfile).Assembly);
        services.AddValidatorsFromAssemblyContaining<SaveCategoryRequestValidator>();

        services.AddFluentValidationAutoValidation(options =>
        {
            options.DisableDataAnnotationsValidation = true;
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
            })
            .AddFluentValidationRulesToSwagger(options =>
            {
                options.SetNotNullableIfMinLengthGreaterThenZero = true;
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
        services.AddRefitClient<IOpenWeatherMapClient>().ConfigureHttpClient(httpClient =>
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

        services.AddSingleton<ISqlClientCache, SqlClientCache>();
        services.AddScoped<IApplicationDbContext>(services => services.GetRequiredService<ApplicationDbContext>());

        services.AddSqlServer<ApplicationDbContext>(sqlConnectionString, options =>
        {
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            options.CommandTimeout(dataContextSettings.CommandTimeout);
            options.EnableRetryOnFailure(dataContextSettings.MaxRetryCount, dataContextSettings.MaxRetryDelay, null);
        });

        services.AddSqlContext(options =>
        {
            options.ConnectionString = sqlConnectionString;
        });

        services.AddSqlServer<DataProtectionDbContext>(sqlConnectionString, options =>
        {
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            options.CommandTimeout(dataContextSettings.CommandTimeout);
            options.EnableRetryOnFailure(dataContextSettings.MaxRetryCount, dataContextSettings.MaxRetryDelay, null);
        });

        services.AddSqlServer<AuthenticationDbContext>(sqlConnectionString, options =>
        {
            options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            options.CommandTimeout(dataContextSettings.CommandTimeout);
            options.EnableRetryOnFailure(dataContextSettings.MaxRetryCount, dataContextSettings.MaxRetryDelay, null);
        });

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
        services.AddScoped<IUserService, HttpUserService>();

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
                .UseSqlServerStorage(sqlConnectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                });
        });

        services.AddHangfireServer();

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

        services.AddHttpClient<IAzureTokenProvider, AzureTokenProvider>();
        services.AddHttpClient<ITranslatorClient, TranslatorClient>();

        services.Scan(scan => scan.FromAssemblyOf<IdentityService>()
            .AddClasses(classes => classes.InNamespaceOf<IdentityService>())
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddHostedService<IdentityRoleService>();
        services.AddHostedService<IdentityUserService>();

        services.AddQuartzHostedService(options =>
        {
            options.StartDelay = TimeSpan.Zero;
            options.AwaitApplicationStarted = true;
            options.WaitForJobsToComplete = true;
        });

        services.AddQuartz(options =>
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

        services.AddQuartzServer();
    }

    public void Configure(IApplicationBuilder app)
    {
        var appSettings = app.ApplicationServices.GetRequiredService<IOptions<AppSettings>>().Value;
        var swaggerSettings = app.ApplicationServices.GetRequiredService<IOptions<SwaggerSettings>>().Value;

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
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/healthchecks", new HealthCheckOptions
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

            endpoints.MapEndpoints();
            endpoints.MapRazorPages();
        });
    }
}