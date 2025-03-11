#region

using System.Net;
using FluentAssertions;
using zora.Core.Domain;
using zora.Core.DTOs.Responses;
using zora.Core.Enums;
using zora.Tests.TestFixtures.v1;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.Integration;

[Collection("TestCollection")]
public sealed class AuthenticationControllerIntegrationTests : AuthenticationTestBase
{
    public AuthenticationControllerIntegrationTests(MockedRepositoryFixture fixture) : base(fixture)
    {
    }

    [Fact(DisplayName = "GIVEN a valid login request WHEN the token is requested THEN a token is returned")]
    public async Task Authenticate_WithValidCredentials_ReturnsToken()
    {
        string username = "testuser";
        string password = "password123";

        User user = new()
        {
            Id = 1,
            Username = username,
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword(password)
        };

        this.SetupValidAuthentication(username, password, user);

        HttpResponseMessage response = await this.RequestToken(username, password);

        await this.AssertSuccessfulTokenResponse(response);
    }

    [Fact(DisplayName =
        "GIVEN an invalid login request with nonexistent user WHEN the token is requested THEN an unauthorized error is returned")]
    public async Task Authenticate_WithNonexistentUser_ReturnsUnauthorized()
    {
        string username = "nonexistentuser";
        string password = "wrongpassword";

        this.SetupInvalidAuthentication(username, password, AuthenticationErrorType.InvalidCredentials,
            "User not found");

        HttpResponseMessage response = await this.RequestToken(username, password);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN an invalid login request with incorrect password WHEN the token is requested THEN an unauthorized error is returned")]
    public async Task Authenticate_WithIncorrectPassword_ReturnsUnauthorized()
    {
        string username = "testuser";
        string wrongPassword = "wrongpassword";

        this.SetupInvalidAuthentication(username, wrongPassword, AuthenticationErrorType.InvalidCredentials,
            "Invalid credentials");

        HttpResponseMessage response = await this.RequestToken(username, wrongPassword);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a login request with empty credentials WHEN the token is requested THEN bad request is returned")]
    public async Task Authenticate_WithEmptyCredentials_ReturnsBadRequest()
    {
        this.SetupInvalidLoginRequest();

        HttpResponseMessage response = await this.RequestToken("", "password123");

        await this.AssertResponseStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName =
        "GIVEN a login request for a deleted user WHEN the token is requested THEN unauthorized is returned")]
    public async Task Authenticate_WithDeletedUser_ReturnsUnauthorized()
    {
        string username = "deleteduser";
        string password = "password123";

        this.SetupInvalidAuthentication(username, password, AuthenticationErrorType.UserDeleted, "User is deleted");

        HttpResponseMessage response = await this.RequestToken(username, password);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a logged in user WHEN getcurrentuser is called THEN valid information about the user is returned")]
    public async Task GetCurrentUser_WithAuthenticatedUser_ReturnsUserInfo()
    {
        this.Fixture.Claims = AuthenticationUtils.RegularUserClaims;
        this.Fixture.SetupAuthenticationForClient(this.Client);

        this.SetupUserService(2, new User
        {
            Id = 2,
            Username = "testuser",
            Email = "testuser@example.com"
        });

        HttpResponseMessage response = await this.GetCurrentUser();

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);
        MinimumUserDto? userInfo = await this.ReadResponseContent<MinimumUserDto>(response);
        userInfo.Should().NotBeNull();
        userInfo!.Username.Should().Be("testuser");
    }

    [Fact(DisplayName = "GIVEN an anonymous user WHEN getcurrentuser is called THEN unauthorized is returned")]
    public async Task GetCurrentUser_WithAnonymousUser_ReturnsUnauthorized()
    {
        this.Fixture.Claims = AuthenticationUtils.AnonymousUserClaims;
        this.Fixture.SetupAuthenticationForClient(this.Client);

        HttpResponseMessage response = await this.GetCurrentUser();

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }
}
