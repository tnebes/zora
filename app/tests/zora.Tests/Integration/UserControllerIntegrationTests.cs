#region

using System.Net;
using FluentAssertions;
using Moq;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Users;
using zora.Tests.TestFixtures.v1;
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
        this.Fixture.MockUserRepository.Setup(repo => repo.SoftDelete(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Something went wrong"));
        HttpResponseMessage response = await this.DeleteUser(userId);
        this.Fixture.Users.Should().HaveCount(3);

        await this.AssertResponseStatusCode(response, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN updating a user THEN the user should be updated")]
    public async Task Update_WithAdminUser_UpdatesUser()
    {
        this.SetupAdminAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;

        long userId = 1;
        User userToUpdate = expectedUsers.First(u => u.Id == userId);

        UpdateUserDto updateUserDto = new()
        {
            Id = userId,
            Username = userToUpdate.Username,
            Email = "updated.email@example.com",
            RoleIds = new List<long> { 1, 2 }
        };

        HttpResponseMessage response = await this.UpdateUser(userId, updateUserDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);
        FullUserDto? updatedUser = await this.ReadResponseContent<FullUserDto>(response);

        updatedUser.Should().NotBeNull();
        updatedUser!.Email.Should().Be(updateUserDto.Email);
    }

    [Fact(DisplayName =
        "GIVEN a logged in regular user WHEN updating another user THEN the user should not be updated")]
    public async Task Update_WithRegularUserUpdatingAnotherUser_ReturnsUnauthorized()
    {
        this.SetupRegularUserAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;

        long userId = 3; // Different user than the authenticated one
        User userToUpdate = expectedUsers.First(u => u.Id == userId);

        UpdateUserDto updateUserDto = new()
        {
            Id = userId,
            Username = userToUpdate.Username,
            Email = "updated.email@example.com",
            RoleIds = new List<long> { 1, 2 }
        };

        HttpResponseMessage response = await this.UpdateUser(userId, updateUserDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a logged in regular user WHEN updating itself THEN the user should be updated")]
    public async Task Update_WithRegularUserUpdatingItself_UpdatesUser()
    {
        this.SetupRegularUserAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;

        long userId = 2; // Same user as the authenticated one
        User userToUpdate = expectedUsers.First(u => u.Id == userId);

        UpdateUserDto updateUserDto = new()
        {
            Id = userId,
            Username = userToUpdate.Username,
            Email = "updated.email@example.com",
            RoleIds = new List<long> { 2 }
        };

        HttpResponseMessage response = await this.UpdateUser(userId, updateUserDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);
        FullUserDto? updatedUser = await this.ReadResponseContent<FullUserDto>(response);

        updatedUser.Should().NotBeNull();
        updatedUser!.Email.Should().Be(updateUserDto.Email);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN finding a user with some parameters THEN that user should be returned")]
    public async Task Find_WithAdminUser_ReturnsMatchingUsers()
    {
        this.SetupAdminAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;

        QueryParamsDto queryParams = new()
        {
            Page = 1,
            PageSize = 10,
            SearchTerm = "user"
        };

        HttpResponseMessage response = await this.FindUsers(queryParams);

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);
        UserResponseDto<FullUserDto>? userResponse =
            await this.ReadResponseContent<UserResponseDto<FullUserDto>>(response);

        userResponse.Should().NotBeNull();
        userResponse!.Items.Should().NotBeEmpty();
    }

    /*
    [Fact(DisplayName =
        "GIVEN a logged in regular user WHEN finding a user with some parameters THEN that user should be returned")]
    public async Task Find_WithRegularUser_ReturnsMatchingUsers()
    {
        this.SetupRegularUserAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;

        QueryParamsDto queryParams = new()
        {
            Page = 1,
            PageSize = 10,
            SearchTerm = "user"
        };

        HttpResponseMessage response = await this.FindUsers(queryParams);

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);
        UserResponseDto<FullUserDto>? userResponse =
            await this.ReadResponseContent<UserResponseDto<FullUserDto>>(response);

        userResponse.Should().NotBeNull();
        userResponse!.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN finding a user with query parameters that is search term that is 2 characters long THEN invalid request should be returned")]
    public async Task Find_WithShortSearchTerm_ReturnsBadRequest()
    {
        this.SetupAdminAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;

        QueryParamsDto queryParams = new()
        {
            Page = 1,
            PageSize = 10,
            SearchTerm = "ab" // 2 characters long
        };

        this.Fixture.MockUserRepository
            .Setup(repo => repo.FindUsersAsync(It.IsAny<QueryParamsDto>(), It.IsAny<bool>()))
            .ThrowsAsync(new Exception("Search term too short"));

        HttpResponseMessage response = await this.FindUsers(queryParams);

        await this.AssertResponseStatusCode(response, HttpStatusCode.BadRequest);
    }
    */

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN creating a new user THEN that new user should be created")]
    public async Task Create_WithAdminUser_CreatesNewUser()
    {
        this.SetupAdminAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;

        CreateMinimumUserDto createUserDto = new()
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "Password123!",
            RoleIds = new List<long> { 1 }
        };

        HttpResponseMessage response = await this.CreateUser(createUserDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Created);
        FullUserDto? createdUser = await this.ReadResponseContent<FullUserDto>(response);

        createdUser.Should().NotBeNull();
        createdUser!.Username.Should().Be(createUserDto.Username);
        createdUser.Email.Should().Be(createUserDto.Email);
    }

    [Fact(DisplayName =
        "GIVEN a logged in regular user WHEN creating a new user THEN that new user should not be created")]
    public async Task Create_WithRegularUser_ReturnsUnauthorized()
    {
        this.SetupRegularUserAuthentication();
        List<User> expectedUsers = UserUtils.GetValidUsers().ToList();
        this.Fixture.Users = expectedUsers;

        CreateMinimumUserDto createUserDto = new()
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "Password123!",
            RoleIds = new List<long> { 1 }
        };

        HttpResponseMessage response = await this.CreateUser(createUserDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }
}
