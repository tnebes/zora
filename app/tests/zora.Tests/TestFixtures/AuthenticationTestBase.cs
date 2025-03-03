#region

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using zora.Core;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Core.Enums;
using zora.Core.Interfaces.Services;

#endregion

namespace zora.Tests.TestFixtures;

public abstract class AuthenticationTestBase : BaseIntegrationTest
{
    protected readonly Mock<IAuthenticationService> MockAuthService;
    protected readonly Mock<IJwtService> MockJwtService;
    protected readonly Mock<IUserService> MockUserService;

    protected AuthenticationTestBase(MockedRepositoryFixture fixture) : base(fixture)
    {
        this.MockJwtService = new Mock<IJwtService>();
        this.MockUserService = new Mock<IUserService>();
        this.MockAuthService = new Mock<IAuthenticationService>();

        this.SetupDefaultMocks();

        this.Client = fixture.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                RemoveExistingServices<IJwtService>(services);
                RemoveExistingServices<IUserService>(services);
                RemoveExistingServices<IAuthenticationService>(services);

                services.AddSingleton(this.MockJwtService.Object);
                services.AddSingleton(this.MockUserService.Object);
                services.AddSingleton(this.MockAuthService.Object);
            });
        }).CreateClient();
    }

    private void SetupDefaultMocks()
    {
        this.MockJwtService.Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns("test-token");
        this.MockJwtService.Setup(s => s.GetTokenExpiration())
            .Returns(3600);
    }

    protected static void RemoveExistingServices<T>(IServiceCollection services)
    {
        List<ServiceDescriptor> descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (ServiceDescriptor descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    protected void SetupValidAuthentication(string username, string password, User user)
    {
        this.MockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(true);

        this.MockAuthService.Setup(s => s.AuthenticateUser(It.Is<LoginRequestDto>(r =>
                r.Username == username && r.Password == password)))
            .ReturnsAsync(Result.Ok(user));
    }

    protected void SetupInvalidAuthentication(string username, string password, AuthenticationErrorType errorType,
        string errorMessage)
    {
        this.MockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(true);

        this.MockAuthService.Setup(s => s.AuthenticateUser(It.Is<LoginRequestDto>(r =>
                r.Username == username && r.Password == password)))
            .ReturnsAsync(Result.Fail<User>(new Error(errorMessage)
                .WithMetadata(Constants.ERROR_TYPE, errorType)));
    }

    protected void SetupInvalidLoginRequest()
    {
        this.MockAuthService.Setup(s => s.IsValidLoginRequest(It.Is<LoginRequestDto>(r =>
                string.IsNullOrEmpty(r.Username) || string.IsNullOrEmpty(r.Password))))
            .Returns(false);
    }

    protected async Task<HttpResponseMessage> RequestToken(string username, string password)
    {
        LoginRequestDto loginRequest = new()
        {
            Username = username,
            Password = password
        };

        return await TestHelpers.ExecutePostRequest(this.Client, "/api/v1/authentication/token", loginRequest);
    }

    protected async Task<HttpResponseMessage> GetCurrentUser() =>
        await this.Client.GetAsync("/api/v1/authentication/current-user");

    protected async Task AssertSuccessfulTokenResponse(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TokenResponseDto? tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        tokenResponse.Should().NotBeNull();
        tokenResponse!.Token.Should().NotBeNullOrEmpty();
        tokenResponse.ExpiresIn.Should().BeGreaterThan(0);
    }

    protected void SetupUserService(long userId, User user)
    {
        this.MockUserService.Setup(s => s.GetByIdAsync(userId, It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok(user));
    }
}
