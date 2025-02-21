#region

using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Interfaces;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration,
        bool isDevelopment)
    {
        return services.AddCache()
            .AddConfigureMapper()
            .AddZoraControllers()
            .AddEndpointsApiExplorer()
            .AddSwaggerServices()
            .AddZoraCors(isDevelopment)
            .AddZoraAuthenticationAndAuthorisation(configuration)
            .AddZoraLogging()
            .AddZoraDbContext(configuration, isDevelopment)
            .AddZoraServices();
    }

    private static IServiceCollection AddZoraServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IZoraService>()
            .AddClasses(classes => classes
                .AssignableTo<IZoraService>()
                .Where(type =>
                    type.GetCustomAttribute<ServiceLifetimeAttribute>()?.Lifetime
                    == ServiceLifetime.Singleton))
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
            .AddClasses(classes => classes
                .AssignableTo<IZoraService>()
                .Where(type =>
                    type.GetCustomAttribute<ServiceLifetimeAttribute>()?.Lifetime
                    == ServiceLifetime.Transient))
            .AsImplementedInterfaces()
            .WithTransientLifetime()
            .AddClasses(classes => classes
                .AssignableTo<IZoraService>()
                .Where(type =>
                    type.GetCustomAttribute<ServiceLifetimeAttribute>()?.Lifetime
                    != ServiceLifetime.Singleton &&
                    type.GetCustomAttribute<ServiceLifetimeAttribute>()?.Lifetime
                    != ServiceLifetime.Transient))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        return services;
    }

    private static IServiceCollection AddZoraControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        return services;
    }

    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            OpenApiInfo info = new()
            {
                Title = "Zora API",
                Version = "v1",
                Description = "API for Zora",
                Contact = new OpenApiContact
                {
                    Name = "Tomislav Nebes",
                    Email = $"tnebes@{Constants.DRAUCODE_COM}",
                    Url = new Uri(Constants.ZORA_URL)
                },
                License = new OpenApiLicense
                {
                    Name = "Educational License",
                    Url = new Uri(Constants.ZORA_URL)
                }
            };

            options.SwaggerDoc("v1", info);

            OpenApiSecurityScheme securityScheme = new()
            {
                Description =
                    "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            OpenApiSecurityRequirement securityRequirement = new()
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        },
                        Scheme = "Bearer",
                        Name = "Bearer",
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            };

            options.AddSecurityRequirement(securityRequirement);

            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    private static IServiceCollection AddZoraAuthenticationAndAuthorisation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                string? issuerSigningKey = configuration[Constants.ISSUER_SIGNING_KEY];

                if (string.IsNullOrEmpty(issuerSigningKey))
                {
                    Log.Error("{KeyName} not found in configuration. Use dotnet user-secrets.",
                        Constants.ISSUER_SIGNING_KEY);
                    throw new InvalidOperationException(
                        $"{Constants.ISSUER_SIGNING_KEY} not found in configuration. Use dotnet user-secrets.");
                }

                options.SaveToken = true;
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters =
                    TokenValidationExtensions.CreateTokenValidationParameters(issuerSigningKey);

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Log.Debug("Authentication failed: {Exception}", context.Exception);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Log.Debug("Token validated successfully");
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Log.Debug("Authentication challenge issued");
                        return Task.CompletedTask;
                    }
                };
            });

        Log.Information("Adding authentication and authorisation.");
        services.AddAuthorization();
        return services;
    }

    private static IServiceCollection AddZoraCors(this IServiceCollection services, bool isDevelopment)
    {
        void CorsOptions(CorsOptions options)
        {
            if (isDevelopment)
            {
                options.AddPolicy(Constants.ZORA_CORS_POLICY_NAME, builder =>
                {
                    builder.SetIsOriginAllowed(origin => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("WWW-Authenticate")
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
            }
            else
            {
                options.AddPolicy(Constants.ZORA_CORS_POLICY_NAME, builder =>
                {
                    builder.WithOrigins(Constants.ZORA_SUBDOMAIN_URL, Constants.ZORA_URL, Constants.ZORA_URL_WITH_PORT)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("WWW-Authenticate")
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                });
            }
        }

        services.AddCors(CorsOptions);
        return services;
    }

    private static IServiceCollection AddZoraLogging(this IServiceCollection services)
    {
        services.AddHttpLogging(logging => logging.LoggingFields = HttpLoggingFields.All);
        return services;
    }

    private static IServiceCollection AddZoraDbContext(this IServiceCollection services, IConfiguration configuration,
        bool isDevelopment)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            string? connectionString = configuration[Constants.CONNECTION_STRING_KEY];
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
                    $"Database connection string {Constants.CONNECTION_STRING_KEY} not found in configuration. Use dotnet user-secrets or environment variables.");
            }

            options.UseLazyLoadingProxies();
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    3,
                    TimeSpan.FromSeconds(3),
                    null);
            }).LogTo(
                message => Log.Information(message),
                [
                    DbLoggerCategory.Database.Command.Name,
                    DbLoggerCategory.Database.Connection.Name,
                    DbLoggerCategory.Query.Name,
                    DbLoggerCategory.Update.Name,
                    DbLoggerCategory.Infrastructure.Name
                ],
                isDevelopment ? LogLevel.Debug : LogLevel.Information);

            if (isDevelopment)
            {
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            }
        });

        services.AddScoped<IDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    private static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }

    private static IServiceCollection AddConfigureMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(Program).Assembly);
        return services;
    }
}
