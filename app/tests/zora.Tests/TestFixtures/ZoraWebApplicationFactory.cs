#region

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

namespace zora.Tests.TestFixtures;

public class ZoraWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            List<ServiceDescriptor> descriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>) ||
                            d.ServiceType == typeof(DbContextOptions) ||
                            d.ServiceType == typeof(IDbContext))
                .ToList();

            foreach (ServiceDescriptor descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryZoraTestDb");
            });

            services.AddScoped<IDbContext>(provider =>
                provider.GetRequiredService<ApplicationDbContext>());

            Mock<ISecretsManagerService> mockSecretsManager = new Mock<ISecretsManagerService>();
            services.AddSingleton(mockSecretsManager.Object);

            Mock<IEnvironmentManagerService> mockEnvironmentManager = new Mock<IEnvironmentManagerService>();
            mockEnvironmentManager.Setup(m => m.CurrentEnvironment).Returns(EnvironmentType.Development);
            services.AddSingleton(mockEnvironmentManager.Object);
        });
    }
}
