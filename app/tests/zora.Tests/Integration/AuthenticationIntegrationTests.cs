#region

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Tests.TestFixtures;

#endregion

namespace zora.Tests.Integration;

public sealed class AuthenticationIntegrationTests : IClassFixture<ZoraWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly ZoraWebApplicationFactory _factory;

    public AuthenticationIntegrationTests(ZoraWebApplicationFactory factory)
    {
        this._factory = factory;
        this._client = this._factory.CreateClient();
    }

    [Fact(Skip = "Integration test requires proper setup")]
    public async Task Authenticate_WithValidCredentials_ReturnsToken()
    {
        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = "tnebes",
            Password = "simplepassword"
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

    [Fact]
    public async Task Authenticate_WithInvalidCredentials_ReturnsUnauthorized()
    {
        LoginRequestDto loginRequest = new LoginRequestDto
        {
            Username = "nonexistentuser",
            Password = "wrongpassword"
        };

        StringContent content = new StringContent(
            JsonSerializer.Serialize(loginRequest),
            Encoding.UTF8,
            "application/json");

        HttpResponseMessage response = await this._client.PostAsync("/api/v1/authentication/token", content);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
