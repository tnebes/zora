#region

using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;

#endregion

namespace zora.Tests.TestFixtures;

public abstract class BaseIntegrationTest
{
    protected readonly MockedRepositoryFixture Fixture;
    protected HttpClient Client;

    protected BaseIntegrationTest(MockedRepositoryFixture fixture)
    {
        this.Fixture = fixture;
        this.Client = fixture.CreateClient();
    }

    protected void SetupAdminUser(List<User> users) => TestHelpers.SetupTestFixture(this.Fixture, this.Client, users);

    protected void SetupRegularUser(List<User> users) =>
        TestHelpers.SetupTestFixture(this.Fixture, this.Client, users, false);

    protected async Task<HttpResponseMessage> GetUsers(QueryParamsDto queryParams) =>
        await TestHelpers.ExecuteGetRequest(this.Client, "/api/v1/users", queryParams);

    protected async Task<UserResponseDto<TUserDto>> AssertSuccessfulUserListResponse<TUserDto>(
        HttpResponseMessage response,
        List<User> expectedUsers) where TUserDto : MinimumUserDto =>
        await TestHelpers.AssertSuccessfulUserResponse<TUserDto>(response, expectedUsers);

    protected async Task<HttpResponseMessage> DeleteUser(long userId) =>
        await TestHelpers.ExecuteDeleteRequest(this.Client, $"/api/v1/users/{userId}");

    protected async Task<HttpResponseMessage> SearchUsers<T>(T searchParams) where T : DynamicQueryParamsDto =>
        await TestHelpers.ExecutePostRequest(this.Client, "/api/v1/users/search", searchParams);

    protected async Task AssertResponseStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode) =>
        response.StatusCode.Should().Be(expectedStatusCode);

    protected async Task<TResponse?> ReadResponseContent<TResponse>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<TResponse>();
}
