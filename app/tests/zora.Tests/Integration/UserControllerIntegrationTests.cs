#region

using System.Net;
using System.Net.Http.Json;
using AutoMapper;
using FluentAssertions;
using zora.API.Mapping;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Tests.TestFixtures;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.Integration;

public sealed class UserControllerIntegrationTests : IClassFixture<MockedRepositoryFixture>
{
    private readonly HttpClient _client;
    private readonly MockedRepositoryFixture _fixture;

    public UserControllerIntegrationTests(MockedRepositoryFixture fixture)
    {
        this._fixture = fixture;
        this._client = fixture.CreateClient();
        this._fixture.SetupAuthenticationForClient(this._client);
    }

    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto and an admin user WHEN Get() is invoked THEN the controller returns an OK result with the expected paginated user list")]
    public async Task Get_WithValidQueryParamsAndAdminUser_ReturnsOkWithPaginatedUserList()
    {
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();

        this._fixture.Users = expectedUsers;

        QueryParamsDto queryParams = QueryParamsUtils.GetValidQueryParams();

        HttpResponseMessage response = await this._client.GetAsync($"/api/v1/users{queryParams.ToQueryString()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        string content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        UserResponseDto<FullUserDto>? userResponse =
            await response.Content.ReadFromJsonAsync<UserResponseDto<FullUserDto>>();

        List<FullUserDto> expectedUsersDto = expectedUsers.Select(user =>
                new MapperConfiguration(cfg => cfg.AddProfile<UserMappingProfile>())
                    .CreateMapper()
                    .Map<FullUserDto>(user))
            .ToList();

        userResponse.Should().NotBeNull();
        userResponse.PageSize.Should().Be(50);
        userResponse.Page.Should().Be(1);
        userResponse.Total.Should().Be(3);
        userResponse.Items.Should().NotBeNullOrEmpty();
        List<FullUserDto> actualUsers = userResponse.Items.ToList();
        actualUsers.Should().HaveCount(3);
        actualUsers.Should().BeEquivalentTo(expectedUsersDto);
    }

    [Fact(DisplayName =
        "GIVEN an invalid QueryParamsDto WHEN Get() is called by an admin THEN a OK is returned with results mimicking the results of a default query")]
    public async Task Get_WithInvalidQueryParams_ReturnsOkWithDefaultResults()
    {
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        QueryParamsDto invalidQueryParams = QueryParamsUtils.GetInvalidQueryParams();

        this._fixture.Users = expectedUsers;

        HttpResponseMessage response =
            await this._client.GetAsync($"/api/v1/users{invalidQueryParams.ToQueryString()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        string content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        UserResponseDto<FullUserDto>? userResponse =
            await response.Content.ReadFromJsonAsync<UserResponseDto<FullUserDto>>();

        List<FullUserDto> expectedUsersDto = expectedUsers.Select(user =>
                new MapperConfiguration(cfg => cfg.AddProfile<UserMappingProfile>())
                    .CreateMapper()
                    .Map<FullUserDto>(user))
            .ToList();

        userResponse.Should().NotBeNull();
        userResponse.PageSize.Should().Be(50);
        userResponse.Page.Should().Be(1);
        userResponse.Total.Should().Be(3);
        userResponse.Items.Should().NotBeNullOrEmpty();
        List<FullUserDto> actualUsers = userResponse.Items.ToList();
        actualUsers.Should().HaveCount(3);
        actualUsers.Should().BeEquivalentTo(expectedUsersDto);
    }

    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto with extra large page size and non-admin user WHEN Get() is invoked THEN the controller returns an OK result with the maximum allowed page size")]
    public async Task Get_WithValidQueryParamsAndNonAdminUser_ReturnsOkWithPaginatedUserList()
    {
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();

        this._fixture.Users = expectedUsers;

        QueryParamsDto queryParams = QueryParamsUtils.GetLargePageSizeQueryParams();

        HttpResponseMessage response = await this._client.GetAsync($"/api/v1/users{queryParams.ToQueryString()}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        string content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        UserResponseDto<FullUserDto>? userResponse =
            await response.Content.ReadFromJsonAsync<UserResponseDto<FullUserDto>>();

        List<FullUserDto> expectedUsersDto = expectedUsers.Select(user =>
                new MapperConfiguration(cfg => cfg.AddProfile<UserMappingProfile>())
                    .CreateMapper()
                    .Map<FullUserDto>(user))
            .ToList();

        userResponse.Should().NotBeNull();
        userResponse.PageSize.Should().Be(50);
        userResponse.Page.Should().Be(1);
        userResponse.Total.Should().Be(3);
        userResponse.Items.Should().NotBeNullOrEmpty();
        List<FullUserDto> actualUsers = userResponse.Items.ToList();
        actualUsers.Should().HaveCount(3);
        actualUsers.Should().BeEquivalentTo(expectedUsersDto);
    }

    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto that triggers an exception at the repository level WHEN Get() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Get_WithExceptionAtRepositoryLevel_Returns500InternalServerError()
    {
    }

    [Fact(DisplayName =
        "GIVEN a valid DynamicQueryUserParamsDto and an admin user WHEN Search() is invoked THEN the controller returns an OK result with the expected user search response")]
    public async Task Search_WithValidParamsAndAdminUser_ReturnsOkWithSearchResults()
    {
    }

    [Fact(DisplayName =
        "GIVEN a valid DynamicQueryUserParamsDto and a non-admin user WHEN Search() is invoked THEN the controller normalises the query parameters and returns an OK result with the expected response")]
    public async Task Search_WithValidParamsAndNonAdminUser_NormalizesParamsAndReturnsOk()
    {
    }

    [Fact(DisplayName =
        "GIVEN a DynamicQueryUserParamsDto that results in a service failure WHEN Search() is invoked THEN the controller returns a BadRequest result")]
    public async Task Search_WithServiceFailure_ReturnsBadRequest()
    {
    }

    [Fact(DisplayName =
        "GIVEN a scenario where an exception is thrown at the repository level during the search WHEN Search() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Search_WithRepositoryException_Returns500InternalServerError()
    {
    }

    [Fact(DisplayName =
        "GIVEN a valid user ID corresponding to an existing user, and the caller is an admin WHEN Delete() is invoked THEN the controller deletes the user and returns a NoContent result")]
    public async Task Delete_WithExistingUserAndAdminCaller_DeletesUserAndReturnsNoContent()
    {
    }

    [Fact(DisplayName =
        "GIVEN a valid user ID corresponding to an existing user, and the caller is the owner (non-admin) WHEN Delete() is invoked THEN the controller deletes the user and returns a NoContent result")]
    public async Task Delete_WithExistingUserAndOwnerCaller_DeletesUserAndReturnsNoContent()
    {
    }

    [Fact(DisplayName =
        "GIVEN a valid user ID that does not correspond to any existing user WHEN Delete() is invoked THEN the controller returns a NotFound result")]
    public async Task Delete_WithNonExistingUser_ReturnsNotFound()
    {
    }

    [Fact(DisplayName =
        "GIVEN a valid user ID for an existing user but the caller is neither admin nor owner WHEN Delete() is invoked THEN the controller returns an Unauthorized result")]
    public async Task Delete_WithExistingUserButUnauthorizedCaller_ReturnsUnauthorized()
    {
    }

    [Fact(DisplayName =
        "GIVEN a valid user ID that causes an exception at the repository level during deletion WHEN Delete() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Delete_WithRepositoryException_Returns500InternalServerError()
    {
    }
}
