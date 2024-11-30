using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Interfaces;

namespace zora.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddZoraControllers()
            .AddEndpointsApiExplorer()
            .AddSwaggerServices()
            .AddZoraCors()
            .AddZoraAuthenticationAndAuthorisation()
            .AddZoraLogging()
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
        services.AddControllers();
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
                    Url = new Uri(Constants.ZoraUrl)
                },
                License = new OpenApiLicense
                {
                    Name = "Educational License",
                    Url = new Uri(Constants.ZoraUrl)
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

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            options.IncludeXmlComments(xmlPath);
        });

        return services;
    }

    private static IServiceCollection AddZoraAuthenticationAndAuthorisation(this IServiceCollection services)
    {
        static void authenticationOptions(AuthenticationOptions options)
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }

        static void bearerOptions(JwtBearerOptions options)
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidAudience = "https://draucode.com",
                ValidIssuer = "https://draucode.com",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Constants.IssuerSigningKey)),
                ValidateIssuerSigningKey = false
            };
        }

        services.AddAuthentication(authenticationOptions).AddJwtBearer(bearerOptions);
        services.AddAuthorization();

        return services;
    }

    private static IServiceCollection AddZoraCors(this IServiceCollection services)
    {
        static void corsOptions(CorsOptions options)
        {
            options.AddPolicy("CorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
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
}
