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

public static class TestHelpers
{
    public static async Task<HttpResponseMessage> ExecuteGetRequest(HttpClient client, string endpoint,
        QueryParamsDto queryParams) => await client.GetAsync($"{endpoint}{queryParams.ToQueryString()}");

    public static async Task<HttpResponseMessage> ExecutePostRequest<T>(HttpClient client, string endpoint, T content)
    {
        StringContent stringContent = new(
            JsonSerializer.Serialize(content),
            Encoding.UTF8,
            "application/json");

        return await client.PostAsync(endpoint, stringContent);
    }

    public static async Task<HttpResponseMessage> ExecuteDeleteRequest(HttpClient client, string endpoint) =>
        await client.DeleteAsync(endpoint);

    public static async Task AssertSuccessfulResponse(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        string content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    public static async Task<UserResponseDto<TUserDto>> AssertSuccessfulUserResponse<TUserDto>(
        HttpResponseMessage response,
        List<User> expectedUsers,
        int expectedPage = 1,
        int expectedPageSize = 50,
        int expectedTotal = 3) where TUserDto : MinimumUserDto
    {
        await AssertSuccessfulResponse(response);

        UserResponseDto<TUserDto>? userResponse = await response.Content.ReadFromJsonAsync<UserResponseDto<TUserDto>>();

        userResponse.Should().NotBeNull();
        userResponse!.PageSize.Should().Be(expectedPageSize);
        userResponse.Page.Should().Be(expectedPage);
        userResponse.Total.Should().Be(expectedTotal);
        userResponse.Items.Should().NotBeNullOrEmpty();

        return userResponse;
    }

    public static List<TDto> MapUsersToDto<TDto>(List<User> users)
    {
        return users.Select(user =>
                new MapperConfiguration(cfg => cfg.AddProfile<UserMappingProfile>())
                    .CreateMapper()
                    .Map<TDto>(user))
            .ToList();
    }

    public static void SetupTestFixture(
        MockedRepositoryFixture fixture,
        HttpClient client,
        List<User> users,
        bool isAdmin = true)
    {
        fixture.Users = users;
        fixture.Claims = isAdmin ? AuthenticationUtils.AdminClaims : AuthenticationUtils.RegularUserClaims;
        fixture.SetupAuthenticationForClient(client);
    }
}
