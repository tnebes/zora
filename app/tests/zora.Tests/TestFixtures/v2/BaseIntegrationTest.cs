#region

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Infrastructure.Data;

#endregion

namespace zora.Tests.TestFixtures.v2;

public abstract class BaseIntegrationV2TestFixed : IDisposable
{
    private readonly IServiceScope _serviceScope;
    protected readonly WebApplicationFactory<Program> Factory;
    protected HttpClient Client;
    protected ApplicationDbContext DbContext;
    protected IMapper Mapper;

    protected BaseIntegrationV2TestFixed()
    {
        this.Factory = new ZoraWebApplicationFactoryV2();
        this.Client = this.Factory.CreateClient();

        this._serviceScope = this.Factory.Services.CreateScope();
        this.DbContext = this._serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        this.InitializeMapper();
    }

    public void Dispose()
    {
        this._serviceScope?.Dispose();
        this.Factory?.Dispose();
        this.Client?.Dispose();
    }

    private void InitializeMapper()
    {
        IServiceProvider serviceProvider = this._serviceScope.ServiceProvider;
        this.Mapper = serviceProvider.GetRequiredService<IMapper>();
    }

    protected void SetupAdminAuthentication()
    {
        this.Client = this.Factory.CreateClient();
        this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Admin");
    }

    protected void SetupRegularUserAuthentication(long userId = 1)
    {
        this.Client = this.Factory.CreateClient();
        this.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "User");
    }

    #region API Methods

    protected async Task<HttpResponseMessage> GetUsers(QueryParamsDto queryParams) =>
        await this.Client.GetAsync(
            $"/api/v1/users?page={queryParams.Page}&pageSize={queryParams.PageSize}&sortColumn={queryParams.SortColumn}&sortDirection={queryParams.SortDirection}");

    protected async Task<UserResponseDto<TUserDto>> AssertSuccessfulUserListResponse<TUserDto>(
        HttpResponseMessage response,
        List<User> expectedUsers) where TUserDto : MinimumUserDto
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        UserResponseDto<TUserDto>? content = await response.Content.ReadFromJsonAsync<UserResponseDto<TUserDto>>();
        content.Should().NotBeNull();
        content!.Total.Should().Be(expectedUsers.Count);
        content.Items.Should().HaveCount(Math.Min(expectedUsers.Count, content.PageSize));
        return content;
    }

    protected async Task<HttpResponseMessage> DeleteUser(long userId) =>
        await this.Client.DeleteAsync($"/api/v1/users/{userId}");

    protected async Task<HttpResponseMessage> SearchUsers(DynamicQueryParamsDto queryParamsDto) =>
        await this.Client.GetAsync($"/api/v1/users/search?{queryParamsDto.ToQueryString()}");

    protected async Task<HttpResponseMessage> UpdateUser(long userId, UpdateUserDto updateUserDto)
    {
        string json = JsonSerializer.Serialize(updateUserDto);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        return await this.Client.PutAsync($"/api/v1/users/{userId}", content);
    }

    protected async Task<HttpResponseMessage> CreateUser(CreateMinimumUserDto createUserDto)
    {
        string json = JsonSerializer.Serialize(createUserDto);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        return await this.Client.PostAsync("/api/v1/users", content);
    }

    protected async Task<HttpResponseMessage> FindUsers(QueryParamsDto queryParams) =>
        await this.Client.GetAsync(
            $"/api/v1/users/find?page={queryParams.Page}&pageSize={queryParams.PageSize}&sortColumn={queryParams.SortColumn}&sortDirection={queryParams.SortDirection}");

    #endregion

    #region Role API Methods

    protected async Task<HttpResponseMessage> GetRoles(QueryParamsDto queryParams) =>
        await this.Client.GetAsync(
            $"/api/v1/roles?page={queryParams.Page}&pageSize={queryParams.PageSize}&sortColumn={queryParams.SortColumn}&sortDirection={queryParams.SortDirection}");

    protected async Task<HttpResponseMessage> GetRoleById(long roleId) =>
        await this.Client.GetAsync($"/api/v1/roles/{roleId}");

    protected async Task<HttpResponseMessage> CreateRole(CreateRoleDto createRoleDto)
    {
        string json = JsonSerializer.Serialize(createRoleDto);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        return await this.Client.PostAsync("/api/v1/roles", content);
    }

    protected async Task<HttpResponseMessage> UpdateRole(long roleId, UpdateRoleDto updateRoleDto)
    {
        string json = JsonSerializer.Serialize(updateRoleDto);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
        return await this.Client.PutAsync($"/api/v1/roles/{roleId}", content);
    }

    protected async Task<HttpResponseMessage> DeleteRole(long roleId) =>
        await this.Client.DeleteAsync($"/api/v1/roles/{roleId}");

    protected async Task<HttpResponseMessage> SearchRoles(DynamicQueryRoleParamsDto queryParamsDto) =>
        await this.Client.GetAsync($"/api/v1/roles/search?{queryParamsDto.ToQueryString()}");

    protected async Task<HttpResponseMessage> FindRoles(QueryParamsDto queryParams) =>
        await this.Client.GetAsync(
            $"/api/v1/roles/find?page={queryParams.Page}&pageSize={queryParams.PageSize}&sortColumn={queryParams.SortColumn}&sortDirection={queryParams.SortDirection}");

    protected async Task<RoleResponseDto> AssertSuccessfulRoleListResponse(
        HttpResponseMessage response,
        List<Role> expectedRoles)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RoleResponseDto? content = await response.Content.ReadFromJsonAsync<RoleResponseDto>();
        content.Should().NotBeNull();
        content!.Total.Should().Be(expectedRoles.Count);
        content.Items.Should().HaveCount(Math.Min(expectedRoles.Count, content.PageSize));
        return content;
    }

    #endregion

    #region Helper Methods

    protected async Task AssertResponseStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode) =>
        response.StatusCode.Should().Be(expectedStatusCode);

    protected async Task<TResponse?> ReadResponseContent<TResponse>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<TResponse>();

    #endregion

    #region Database Seeding

    protected async Task SeedUsers(List<User> users)
    {
        await this.DbContext.Users.AddRangeAsync(users);
        await this.DbContext.SaveChangesAsync();
    }

    protected async Task SeedPermissions(List<Permission> permissions)
    {
        await this.DbContext.Permissions.AddRangeAsync(permissions);
        await this.DbContext.SaveChangesAsync();
    }

    protected async Task SeedRoles(List<Role> roles)
    {
        await this.DbContext.Roles.AddRangeAsync(roles);
        await this.DbContext.SaveChangesAsync();
    }

    protected async Task SeedUserRoles(List<UserRole> userRoles)
    {
        await this.DbContext.UserRoles.AddRangeAsync(userRoles);
        await this.DbContext.SaveChangesAsync();
    }

    protected async Task SeedRolePermissions(List<RolePermission> rolePermissions)
    {
        await this.DbContext.RolePermissions.AddRangeAsync(rolePermissions);
        await this.DbContext.SaveChangesAsync();
    }

    protected async Task ClearDatabase()
    {
        this.DbContext.RolePermissions.RemoveRange(await this.DbContext.RolePermissions.ToListAsync());
        this.DbContext.UserRoles.RemoveRange(await this.DbContext.UserRoles.ToListAsync());
        this.DbContext.Permissions.RemoveRange(await this.DbContext.Permissions.ToListAsync());
        this.DbContext.Roles.RemoveRange(await this.DbContext.Roles.ToListAsync());
        this.DbContext.Users.RemoveRange(await this.DbContext.Users.ToListAsync());
        await this.DbContext.SaveChangesAsync();
    }

    #endregion
}
