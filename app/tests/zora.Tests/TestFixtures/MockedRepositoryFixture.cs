#region

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using zora.Core.Domain;
using zora.Core.Interfaces.Repositories;

#endregion

namespace zora.Tests.TestFixtures;

public sealed class MockedRepositoryFixture : WebApplicationFactory<Program>
{
    public Mock<IUserRepository> MockUserRepository { get; set; } = new();
    public Mock<IPermissionRepository> MockPermissionRepository { get; set; } = new();
    public List<User> Users { get; set; } = [];
    public List<Permission> Permissions { get; set; } = [];

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Dictionary<string, string?> inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" }
        };

        builder.ConfigureAppConfiguration(config => { config.AddInMemoryCollection(inMemorySettings); });

        builder.ConfigureServices(services =>
        {
            services.AddSingleton(this.MockUserRepository.Object);
            services.AddSingleton(this.MockPermissionRepository.Object);

            List<ServiceDescriptor> authHandlers =
                services.Where(d => d.ServiceType == typeof(IAuthenticationHandler)).ToList();
            foreach (ServiceDescriptor handler in authHandlers)
            {
                services.Remove(handler);
            }

            List<ServiceDescriptor> authSchemeHandlers = services
                .Where(d => d.ServiceType == typeof(AuthenticationHandler<AuthenticationSchemeOptions>)).ToList();
            foreach (ServiceDescriptor handler in authSchemeHandlers)
            {
                services.Remove(handler);
            }

            List<ServiceDescriptor> jwtHandlers =
                services.Where(d => d.ServiceType == typeof(JwtBearerHandler)).ToList();
            foreach (ServiceDescriptor handler in jwtHandlers)
            {
                services.Remove(handler);
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultScheme = "TestScheme";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
        });
    }

    public void SetupAuthenticationForClient(HttpClient client) => client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", "test-token");
}

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!this.Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(AuthenticateResult.Fail("Authorization header not found."));
        }

        Claim[] claims =
        [
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Role, "Admin")
        ];

        ClaimsIdentity identity = new ClaimsIdentity(claims, "Test");
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);

        AuthenticationTicket ticket = new AuthenticationTicket(principal, "TestScheme");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
