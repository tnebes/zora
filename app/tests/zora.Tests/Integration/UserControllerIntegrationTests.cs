#region

using System.Net;
using FluentAssertions;
using Moq;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Tests.TestFixtures;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.Integration;

[Collection("TestCollection")]
public sealed class UserControllerIntegrationTests : BaseIntegrationTest
{
    public UserControllerIntegrationTests(MockedRepositoryFixture fixture) : base(fixture)
    {
    }

    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto and an admin user WHEN Get() is invoked THEN the controller returns an OK result with the expected paginated user list")]
    public async Task Get_WithValidQueryParamsAndAdminUser_ReturnsOkWithPaginatedUserList()
    {
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.SetupUsers(expectedUsers);
        this.SetupAdminAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();
        HttpResponseMessage response = await this.GetUsers(queryParams);

        UserResponseDto<FullUserDto> userResponse =
            await this.AssertSuccessfulUserListResponse<FullUserDto>(response, expectedUsers);

        List<FullUserDto> expectedUsersDto = this.MapUsersToDto<FullUserDto>(expectedUsers);
        List<FullUserDto> actualUsers = userResponse.Items.ToList();

        actualUsers.Should().HaveCount(3);
        actualUsers.Should().BeEquivalentTo(expectedUsersDto);
    }

    [Fact(DisplayName =
        "GIVEN an invalid QueryParamsDto WHEN Get() is called by an admin THEN a OK is returned with results mimicking the results of a default query")]
    public async Task Get_WithInvalidQueryParams_ReturnsOkWithDefaultResults()
    {
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.SetupUsers(expectedUsers);
        this.SetupAdminAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetInvalidQueryParams();
        HttpResponseMessage response = await this.GetUsers(queryParams);

        UserResponseDto<FullUserDto> userResponse =
            await this.AssertSuccessfulUserListResponse<FullUserDto>(response, expectedUsers);

        List<FullUserDto> expectedUsersDto = this.MapUsersToDto<FullUserDto>(expectedUsers);
        List<FullUserDto> actualUsers = userResponse.Items.ToList();

        actualUsers.Should().HaveCount(3);
        actualUsers.Should().BeEquivalentTo(expectedUsersDto);
    }

    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto with extra large page size and non-admin user WHEN Get() is invoked THEN the controller returns an OK result with the maximum allowed page size")]
    public async Task Get_WithValidQueryParamsAndNonAdminUser_ReturnsOkWithPaginatedUserList()
    {
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.SetupUsers(expectedUsers);
        this.SetupRegularUserAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetLargePageSizeQueryParams();
        HttpResponseMessage response = await this.GetUsers(queryParams);

        UserResponseDto<FullUserDto> userResponse =
            await this.AssertSuccessfulUserListResponse<FullUserDto>(response, expectedUsers);

        List<FullUserDto> expectedUsersDto = this.MapUsersToDto<FullUserDto>(expectedUsers);
        List<FullUserDto> actualUsers = userResponse.Items.ToList();

        actualUsers.Should().HaveCount(3);
        actualUsers.Should().BeEquivalentTo(expectedUsersDto);
    }

    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto that triggers an exception at the repository level WHEN Get() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Get_WithExceptionAtRepositoryLevel_Returns500InternalServerError()
    {
        this.SetupAdminAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();

        this.Fixture.MockUserRepository.Setup(repo => repo.GetUsersAsync(It.IsAny<QueryParamsDto>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Something went wrong"));

        HttpResponseMessage response = await this.GetUsers(queryParams);

        await this.AssertResponseStatusCode(response, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName =
        "GIVEN a valid DynamicQueryUserParamsDto and an admin user WHEN Search() is invoked THEN the controller returns an OK result with the expected user search response")]
    public async Task Search_WithValidParamsAndAdminUser_ReturnsOkWithSearchResults()
    {
        this.SetupAdminAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;

        DynamicQueryUserParamsDto queryParams = QueryUtils.DynamicQueryParamsUtils.GetValidDynamicQueryUserParams();
        HttpResponseMessage response = await this.SearchUsers(queryParams);
        UserResponseDto<FullUserDto> userResponse =
            await this.AssertSuccessfulUserListResponse<FullUserDto>(response, expectedUsers);
        List<FullUserDto> actualUsers = userResponse.Items.ToList();
        List<FullUserDto> expectedUsersDto = this.MapUsersToDto<FullUserDto>(expectedUsers);

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);
        actualUsers.Should().BeEquivalentTo(expectedUsersDto);
        userResponse.Page.Should().Be(queryParams.Page);
        userResponse.PageSize.Should().Be(queryParams.PageSize);
        userResponse.Total.Should().Be(expectedUsers.Count);
        userResponse.Items.Should().NotBeNullOrEmpty();
    }

    [Fact(DisplayName =
        "GIVEN a valid DynamicQueryUserParamsDto and a non-admin user WHEN Search() is invoked THEN the controller normalises the query parameters and returns an OK result with the expected response")]
    public async Task Search_WithValidParamsAndNonAdminUser_NormalizesParamsAndReturnsOk()
    {
        this.SetupRegularUserAuthentication();

        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        expectedUsers.RemoveAt(2);
        this.Fixture.Users = expectedUsers;

        DynamicQueryUserParamsDto queryParams = QueryUtils.DynamicQueryParamsUtils.GetValidDynamicQueryUserParams();
        HttpResponseMessage response = await this.SearchUsers(queryParams);

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);
        UserResponseDto<FullUserDto> userResponse =
            await this.AssertSuccessfulUserListResponse<FullUserDto>(response, expectedUsers);
        List<FullUserDto> actualUsers = userResponse.Items.ToList();
        List<FullUserDto> expectedUsersDto = this.MapUsersToDto<FullUserDto>(expectedUsers);

        actualUsers.Should().BeEquivalentTo(expectedUsersDto);
    }

    [Fact(DisplayName =
        "GIVEN a DynamicQueryUserParamsDto that results in a service failure WHEN Search() is invoked THEN the controller returns a BadRequest result")]
    public async Task Search_WithServiceFailure_ReturnsBadRequest()
    {
        this.SetupAdminAuthentication();
        DynamicQueryUserParamsDto queryParams = QueryUtils.DynamicQueryParamsUtils.GetValidDynamicQueryUserParams();
        this.Fixture.MockUserRepository
            .Setup(repo => repo.SearchAsync(It.IsAny<DynamicQueryUserParamsDto>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Something went wrong"));
        HttpResponseMessage response = await this.SearchUsers(queryParams);

        await this.AssertResponseStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName =
        "GIVEN a valid user ID corresponding to an existing user, and the caller is an admin WHEN Delete() is invoked THEN the controller deletes the user and returns a NoContent result")]
    public async Task Delete_WithExistingUserAndAdminCaller_DeletesUserAndReturnsNoContent()
    {
        this.SetupAdminAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;
        long userId = 1;
        HttpResponseMessage response = await this.DeleteUser(userId);

        await this.AssertResponseStatusCode(response, HttpStatusCode.NoContent);
        expectedUsers.Should().NotContain(user => user.Id == userId);
    }

    [Fact(DisplayName =
        "GIVEN a valid user ID corresponding to an existing user, and the caller is the owner (non-admin) WHEN Delete() is invoked THEN the controller deletes the user and returns a NoContent result")]
    public async Task Delete_WithExistingUserAndOwnerCaller_DeletesUserAndReturnsNoContent()
    {
        this.SetupRegularUserAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;
        long userId = 2;
        HttpResponseMessage response = await this.DeleteUser(userId);

        await this.AssertResponseStatusCode(response, HttpStatusCode.NoContent);
        expectedUsers.Should().NotContain(user => user.Id == userId);
    }

    [Fact(DisplayName =
        "GIVEN a valid user ID corresponding to an existing user, and the caller is not the owner (non-admin) WHEN Delete() is invoked THEN the controller returns an Unauthorized result")]
    public async Task Delete_WithExistingUserAndOwnerNotCaller_ReturnsUnauthorized()
    {
        this.SetupRegularUserAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;
        long userId = 3;
        HttpResponseMessage response = await this.DeleteUser(userId);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a valid user ID that does not correspond to any existing user WHEN Delete() is invoked THEN the controller returns a NotFound result")]
    public async Task Delete_WithNonExistingUser_ReturnsNotFound()
    {
        this.SetupAdminAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;
        long userId = 999;
        HttpResponseMessage response = await this.DeleteUser(userId);

        await this.AssertResponseStatusCode(response, HttpStatusCode.NotFound);
    }


    [Fact(DisplayName =
        "GIVEN a valid user ID that causes an exception at the repository level during deletion WHEN Delete() is invoked THEN the controller returns a 500 Internal Server Error")]
    public async Task Delete_WithRepositoryException_Returns500InternalServerError()
    {
        this.SetupAdminAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;
        long userId = 1;
        this.Fixture.MockUserRepository.Setup(repo => repo.SoftDelete(It.IsAny<User>())).ThrowsAsync(new Exception("Something went wrong"));
        HttpResponseMessage response = await this.DeleteUser(userId);
        this.Fixture.Users.Should().HaveCount(3);

        await this.AssertResponseStatusCode(response, HttpStatusCode.InternalServerError);
    }
}
