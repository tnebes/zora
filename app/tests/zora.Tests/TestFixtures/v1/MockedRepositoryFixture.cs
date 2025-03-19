#region

using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using zora.Core.Domain;
using zora.Core.Interfaces.Repositories;
using zora.Tests.TestFixtures.v1.MockBuilders;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.TestFixtures.v1;

public sealed class MockedRepositoryFixture : WebApplicationFactory<Program>
{
    private List<Permission> _permissions = [];
    private List<User> _users = [];
    private List<Role> _roles = [];

    public Mock<IUserRepository> MockUserRepository { get; } = new();
    public Mock<IPermissionRepository> MockPermissionRepository { get; } = new();
    public Mock<IRoleRepository> MockRoleRepository { get; } = new();

    public IEnumerable<Claim> Claims { get; set; } = AuthenticationUtils.AnonymousUserClaims;

    public List<Role> Roles
    {
        get => this._roles;
        set
        {
            this._roles = value;
            RoleRepositoryMockBuilder.SetupRoleRepository(this);
        }
    }

    public List<User> Users
    {
        get => this._users;
        set
        {
            this._users = value;
            UserRepositoryMockBuilder.SetupUserRepository(this);
        }
    }

    public List<Permission> Permissions
    {
        get => this._permissions;
        set
        {
            this._permissions = value;
            PermissionRepositoryMockBuilder.SetupPermissionRepository(this);
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton(this);
            services.AddSingleton(this.MockUserRepository.Object);
            services.AddSingleton(this.MockPermissionRepository.Object);
            services.AddSingleton(this.MockRoleRepository.Object);

            List<ServiceDescriptor> descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(IAuthenticationHandler) ||
                            d.ServiceType == typeof(AuthenticationHandler<AuthenticationSchemeOptions>) ||
                            d.ServiceType == typeof(JwtBearerHandler) ||
                            d.ServiceType == typeof(IConfigureOptions<JwtBearerOptions>) ||
                            d.ServiceType == typeof(IConfigureOptions<AuthenticationOptions>) ||
                            d.ServiceType == typeof(IPostConfigureOptions<JwtBearerOptions>))
                .ToList();

            foreach (ServiceDescriptor descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            // Remove authentication scheme providers
            List<ServiceDescriptor> schemeProviders = services
                .Where(d => d.ServiceType == typeof(IAuthenticationSchemeProvider))
                .ToList();

            foreach (ServiceDescriptor provider in schemeProviders)
            {
                services.Remove(provider);
            }

            // Configure JWT Authentication
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = AuthenticationUtils.GetTestTokenValidationParameters();
                });
        });
    }

    public void SetupAuthenticationForClient(HttpClient client)
    {
        string token = AuthenticationUtils.GenerateJwtToken(this.Claims);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
