#region

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using zora.API.Mapping;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.TestFixtures;

public abstract class BaseIntegrationTest
{
    protected readonly MockedRepositoryFixture Fixture;
    protected HttpClient Client;
    protected IMapper Mapper;

    protected BaseIntegrationTest(MockedRepositoryFixture fixture)
    {
        this.Fixture = fixture;
        this.Client = fixture.CreateClient();
        this.InitializeMapper();
    }

    private void InitializeMapper()
    {
        MapperConfiguration config = new(cfg =>
        {
            cfg.AddProfile<UserMappingProfile>();
            cfg.AddProfile<AssetMappingProfile>();
            cfg.AddProfile<AuthenticationMappingProfile>();
            cfg.AddProfile<PermissionMappingProfile>();
            cfg.AddProfile<RoleMappingProfile>();
        });

        this.Mapper = config.CreateMapper();
    }

    protected List<TDto> MapUsersToDto<TDto>(List<User> users)
    {
        return users.Select(user => this.Mapper.Map<TDto>(user)).ToList();
    }

    protected void SetupUsers(List<User> users)
    {
        this.Fixture.Users = users;
    }

    protected void SetupAdminAuthentication()
    {
        this.Fixture.Claims = AuthenticationUtils.AdminClaims;
        this.Fixture.SetupAuthenticationForClient(this.Client);
    }

    protected void SetupRegularUserAuthentication()
    {
        this.Fixture.Claims = AuthenticationUtils.RegularUserClaims;
        this.Fixture.SetupAuthenticationForClient(this.Client);
    }

    protected async Task<HttpResponseMessage> GetUsers(QueryParamsDto queryParams) =>
        await this.Client.GetAsync($"/api/v1/users{queryParams.ToQueryString()}");

    protected async Task<UserResponseDto<TUserDto>> AssertSuccessfulUserListResponse<TUserDto>(
        HttpResponseMessage response,
        List<User> expectedUsers) where TUserDto : MinimumUserDto
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        string content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        UserResponseDto<TUserDto>? userResponse = await response.Content.ReadFromJsonAsync<UserResponseDto<TUserDto>>();

        userResponse.Should().NotBeNull();
        userResponse!.PageSize.Should().Be(50);
        userResponse.Page.Should().Be(1);
        userResponse.Total.Should().Be(expectedUsers.Count);
        userResponse.Items.Should().NotBeNullOrEmpty();

        return userResponse;
    }

    protected async Task<HttpResponseMessage> DeleteUser(long userId) =>
        await this.Client.DeleteAsync($"/api/v1/users/{userId}");

    protected async Task<HttpResponseMessage> SearchUsers(DynamicQueryParamsDto queryParamsDto)
    {
        return await this.Client.GetAsync($"/api/v1/users/search{queryParamsDto.ToQueryString()}");
    }

    protected async Task AssertResponseStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode) =>
        response.StatusCode.Should().Be(expectedStatusCode);

    protected async Task<TResponse?> ReadResponseContent<TResponse>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<TResponse>();

    protected async Task<HttpResponseMessage> UpdateUser(long userId, UpdateUserDto updateUserDto)
    {
        string json = JsonSerializer.Serialize(updateUserDto);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        return await this.Client.PutAsync($"/api/v1/users/{userId}", content);
    }

    protected async Task<HttpResponseMessage> CreateUser(CreateMinimumUserDto createUserDto)
    {
        string json = JsonSerializer.Serialize(createUserDto);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        return await this.Client.PostAsync("/api/v1/users", content);
    }

    protected async Task<HttpResponseMessage> FindUsers(QueryParamsDto queryParams) =>
        await this.Client.GetAsync($"/api/v1/users/find{queryParams.ToQueryString()}");
}
