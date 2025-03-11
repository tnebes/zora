#region

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using zora.Core.Enums;
using zora.Core.Interfaces;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Tests.TestFixtures.v2;

public sealed class ZoraWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            RemoveDbContextRegistrations(services);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryZoraTestDbV2Fixed");
            });

            services.AddScoped<IDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

            Mock<ISecretsManagerService> mockSecretsManager = new Mock<ISecretsManagerService>();
            services.AddSingleton(mockSecretsManager.Object);

            Mock<IEnvironmentManagerService> mockEnvironmentManager = new Mock<IEnvironmentManagerService>();
            mockEnvironmentManager.Setup(m => m.CurrentEnvironment).Returns(EnvironmentType.Development);
            services.AddSingleton(mockEnvironmentManager.Object);
        });
    }

    private static void RemoveDbContextRegistrations(IServiceCollection services)
    {
        List<ServiceDescriptor> descriptorsToRemove = services
            .Where(d =>
                d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                d.ServiceType == typeof(DbContextOptions) ||
                d.ServiceType == typeof(IDbContext) ||
                d.ServiceType == typeof(ApplicationDbContext) ||
                (d.ServiceType.Name.Contains("DbContext") &&
                 d.ServiceType.Namespace?.Contains("EntityFrameworkCore") == true) ||
                (d.ImplementationType?.Name.Contains("DbContext") == true &&
                 d.ImplementationType?.Namespace?.Contains("EntityFrameworkCore") == true) ||
                (d.ServiceType.Name.Contains("SqlServer") &&
                 d.ServiceType.Namespace?.Contains("EntityFrameworkCore") == true) ||
                (d.ImplementationType?.Name.Contains("SqlServer") == true &&
                 d.ImplementationType?.Namespace?.Contains("EntityFrameworkCore") == true) ||
                (d.ServiceType.Name.Contains("InMemory") &&
                 d.ServiceType.Namespace?.Contains("EntityFrameworkCore") == true) ||
                (d.ImplementationType?.Name.Contains("InMemory") == true &&
                 d.ImplementationType?.Namespace?.Contains("EntityFrameworkCore") == true))
            .ToList();

        foreach (ServiceDescriptor descriptor in descriptorsToRemove)
        {
            services.Remove(descriptor);
        }
    }
}
