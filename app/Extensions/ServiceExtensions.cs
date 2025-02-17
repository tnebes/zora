#region

using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddCache()
            .AddConfigureMapper()
            .AddZoraControllers()
            .AddEndpointsApiExplorer()
            .AddSwaggerServices()
            .AddZoraCors()
            .AddZoraAuthenticationAndAuthorisation(configuration)
            .AddZoraLogging()
            .AddZoraDbContext(configuration)
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
                    Email = "tnebes@draucode.com",
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
                Scheme = "Bearer"
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
                        Scheme = "oauth2",
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

                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidateAudience = true;
                options.TokenValidationParameters.ValidateLifetime = true;
                options.TokenValidationParameters.ValidAudiences = [Constants.ZORA_URL, "https://localhost:4200"];
                options.TokenValidationParameters.ValidIssuers = [Constants.ZORA_URL, "https://localhost:5001"];
                options.TokenValidationParameters.IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey));
                options.TokenValidationParameters.ValidateIssuerSigningKey = true;

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
        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddZoraCors(this IServiceCollection services)
    {
        static void corsOptions(CorsOptions options)
        {
            options.AddPolicy(Constants.ZORA_CORS_POLICY_NAME, builder =>
            {
                builder.WithOrigins(
                        "http://localhost:4200",
                        "https://localhost:4200",
                        "https://localhost:5001",
                        "http://localhost:5000",
                        "https://draucode.com",
                        "http://draucode.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("WWW-Authenticate")
                    .SetPreflightMaxAge(TimeSpan.FromMinutes(10))
                    .SetIsOriginAllowedToAllowWildcardSubdomains();
            });
        }

        services.AddCors(corsOptions);
        return services;
    }

    private static IServiceCollection AddZoraLogging(this IServiceCollection services)
    {
        services.AddHttpLogging(logging => logging.LoggingFields = HttpLoggingFields.All);
        return services;
    }

    private static IServiceCollection AddZoraDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            string? connectionString = configuration[Constants.CONNECTION_STRING_KEY];
            if (string.IsNullOrEmpty(connectionString))
            {
                Log.Error("{KeyName} not found in configuration. Use dotnet user-secrets.",
                    Constants.CONNECTION_STRING_KEY);
                throw new InvalidOperationException(
                    $"Database connection string {Constants.CONNECTION_STRING_KEY} not found in configuration. Use dotnet user-secrets.");
            }

            options.UseLazyLoadingProxies();
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    3,
                    TimeSpan.FromSeconds(3),
                    null);
            }).LogTo(
                Log.Information, [DbLoggerCategory.Database.Command.Name],
                LogLevel.Information);

            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
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
