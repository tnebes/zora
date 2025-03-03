#region

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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
using zora.Tests.TestFixtures;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.Integration;

public sealed class AuthenticationControllerIntegrationTests : IClassFixture<MockedRepositoryFixture>
{
    private readonly HttpClient _client;
    private readonly MockedRepositoryFixture _fixture;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IUserService> _mockUserService;

    public AuthenticationControllerIntegrationTests(MockedRepositoryFixture fixture)
    {
        this._fixture = new FixtureBuilder()
            .WithUserRepository(UserUtils.GetValidUsers().ToList())
            .Build();

        this._mockJwtService = new Mock<IJwtService>();
        this._mockUserService = new Mock<IUserService>();
        this._mockAuthService = new Mock<IAuthenticationService>();

        this._mockJwtService.Setup(s => s.GenerateToken(It.IsAny<User>()))
            .Returns("test-token");
        this._mockJwtService.Setup(s => s.GetTokenExpiration())
            .Returns(3600);

        HttpClient client = fixture.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                RemoveExistingServices<IJwtService>(services);
                RemoveExistingServices<IUserService>(services);
                RemoveExistingServices<IAuthenticationService>(services);

                services.AddSingleton(this._mockJwtService.Object);
                services.AddSingleton(this._mockUserService.Object);
                services.AddSingleton(this._mockAuthService.Object);
            });
        }).CreateClient();

        this._client = client;
    }

    private static void RemoveExistingServices<T>(IServiceCollection services)
    {
        List<ServiceDescriptor> descriptors = services.Where(d => d.ServiceType == typeof(T)).ToList();
        foreach (ServiceDescriptor descriptor in descriptors)
        {
            services.Remove(descriptor);
        }
    }

    [Fact(DisplayName = "GIVEN a valid login request WHEN the token is requested THEN a token is returned")]
    public async Task Authenticate_WithValidCredentials_ReturnsToken()
    {
        string username = "testuser";
        string password = "password123";

        User user = new User
        {
            Id = 1,
            Username = username,
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword(password)
        };

        this._mockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(true);

        this._mockAuthService.Setup(s => s.AuthenticateUser(It.Is<LoginRequestDto>(r =>
                r.Username == username && r.Password == password)))
            .ReturnsAsync(Result.Ok(user));

        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = username,
            Password = password
        };

        StringContent content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await this._client.PostAsync("/api/v1/authentication/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TokenResponseDto? tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        tokenResponse.Should().NotBeNull();
        tokenResponse!.Token.Should().NotBeNullOrEmpty();
        tokenResponse.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact(DisplayName =
        "GIVEN an invalid login request with nonexistent user WHEN the token is requested THEN an unauthorized error is returned")]
    public async Task Authenticate_WithNonexistentUser_ReturnsUnauthorized()
    {
        string username = "nonexistentuser";
        string password = "wrongpassword";

        this._mockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(true);

        this._mockAuthService.Setup(s => s.AuthenticateUser(It.Is<LoginRequestDto>(r =>
                r.Username == username)))
            .ReturnsAsync(Result.Fail<User>(new Error("User not found")
                .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.InvalidCredentials)));

        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = username,
            Password = password
        };

        StringContent content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await this._client.PostAsync("/api/v1/authentication/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN an invalid login request with incorrect password WHEN the token is requested THEN an unauthorized error is returned")]
    public async Task Authenticate_WithIncorrectPassword_ReturnsUnauthorized()
    {
        string username = "testuser";
        string wrongPassword = "wrongpassword";

        this._mockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(true);

        this._mockAuthService.Setup(s => s.AuthenticateUser(It.Is<LoginRequestDto>(r =>
                r.Username == username && r.Password == wrongPassword)))
            .ReturnsAsync(Result.Fail<User>(new Error("Invalid credentials")
                .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.InvalidCredentials)));

        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = username,
            Password = wrongPassword
        };

        StringContent content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await this._client.PostAsync("/api/v1/authentication/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a login request with empty credentials WHEN the token is requested THEN bad request is returned")]
    public async Task Authenticate_WithEmptyCredentials_ReturnsBadRequest()
    {
        this._mockAuthService.Setup(s => s.IsValidLoginRequest(It.Is<LoginRequestDto>(r =>
                string.IsNullOrEmpty(r.Username) || string.IsNullOrEmpty(r.Password))))
            .Returns(false);

        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = "",
            Password = "password123"
        };

        StringContent content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await this._client.PostAsync("/api/v1/authentication/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName =
        "GIVEN a login request for a deleted user WHEN the token is requested THEN unauthorized is returned")]
    public async Task Authenticate_WithDeletedUser_ReturnsUnauthorized()
    {
        string username = "deleteduser";
        string password = "password123";

        this._mockAuthService.Setup(s => s.IsValidLoginRequest(It.IsAny<LoginRequestDto>()))
            .Returns(true);

        this._mockAuthService.Setup(s => s.AuthenticateUser(It.Is<LoginRequestDto>(r =>
                r.Username == username)))
            .ReturnsAsync(Result.Fail<User>(new Error("User is deleted")
                .WithMetadata(Constants.ERROR_TYPE, AuthenticationErrorType.UserDeleted)));

        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = username,
            Password = password
        };

        StringContent content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await this._client.PostAsync("/api/v1/authentication/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a logged in user WHEN getcurrentuser is called THEN valid information about the user is returned")]
    public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsUserInfo()
    {
        this._fixture.SetupAuthenticationForClient(this._client);

        this._mockUserService.Setup(s => s.GetByIdAsync(1, It.IsAny<bool>()))
            .ReturnsAsync(Result.Ok(new User
            {
                Id = 1,
                Username = "testuser",
                Email = "testuser@example.com"
            }));

        HttpResponseMessage response = await this._client.GetAsync("/api/v1/authentication/current-user");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        MinimumUserDto? userInfo = await response.Content.ReadFromJsonAsync<MinimumUserDto>();
        userInfo.Should().NotBeNull();
        userInfo!.Username.Should().Be("testuser");
    }

    [Fact(DisplayName = "GIVEN an anonymous user WHEN getcurrentuser is called THEN unauthorized is returned")]
    public async Task GetCurrentUser_WithAnonymousUser_ReturnsUnauthorized()
    {
        this._client.DefaultRequestHeaders.Remove("Authorization");

        HttpResponseMessage response = await this._client.GetAsync("/api/v1/authentication/current-user");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
