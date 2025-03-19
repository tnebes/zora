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
using zora.Core.DTOs.Assets;
using zora.Core.DTOs.Permissions;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Roles;
using zora.Core.DTOs.Users;
using zora.Infrastructure.Data;

#endregion

namespace zora.Tests.TestFixtures.v2;

public abstract class BaseIntegrationTest : IAsyncLifetime, IDisposable
{
    private readonly IServiceScope _serviceScope;
    private readonly WebApplicationFactory<Program> Factory;
    private HttpClient Client;
    protected ApplicationDbContext DbContext;
    protected IMapper Mapper;

    protected BaseIntegrationTest()
    {
        this.Factory = new ZoraWebApplicationFactory();
        this.Client = this.Factory.CreateClient();

        this._serviceScope = this.Factory.Services.CreateScope();
        this.DbContext = this._serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        this.InitializeMapper();
    }

    public async Task InitializeAsync()
    {
        await this.ClearDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await this.ClearDatabaseAsync();
        this.Dispose();
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

    # region Task API Methods

    protected async Task<HttpResponseMessage> GetTasks(QueryParamsDto queryParams) =>
        await this.Client.GetAsync($"/api/v1/tasks{queryParams.ToQueryString()}");

    # endregion

    #region API Methods

    protected async Task<HttpResponseMessage> GetUsers(QueryParamsDto queryParams) =>
        await this.Client.GetAsync($"/api/v1/users{queryParams.ToQueryString()}");

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
        await this.Client.GetAsync($"/api/v1/users/find{queryParams.ToQueryString()}");

    #endregion

    #region Role API Methods

    protected async Task<HttpResponseMessage> GetRoles(QueryParamsDto queryParams) =>
        await this.Client.GetAsync($"/api/v1/roles{queryParams.ToQueryString()}");

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
        await this.Client.GetAsync($"/api/v1/roles/find{queryParams.ToQueryString()}");

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

    #region Permission API Methods

    protected async Task<HttpResponseMessage> GetPermissions(QueryParamsDto queryParams) =>
        await this.Client.GetAsync($"/api/v1/permissions{queryParams.ToQueryString()}");

    protected async Task<HttpResponseMessage> GetPermissionById(long permissionId) =>
        await this.Client.GetAsync($"/api/v1/permissions/{permissionId}");

    protected async Task<HttpResponseMessage> CreatePermission(CreatePermissionDto dto) =>
        await this.Client.PostAsJsonAsync("/api/v1/permissions", dto);

    protected async Task<HttpResponseMessage> UpdatePermission(long id, UpdatePermissionDto dto) =>
        await this.Client.PutAsJsonAsync($"/api/v1/permissions/{id}", dto);

    protected async Task<HttpResponseMessage> DeletePermission(long id) =>
        await this.Client.DeleteAsync($"/api/v1/permissions/{id}");

    protected async Task<HttpResponseMessage> SearchPermissions(DynamicQueryParamsDto queryParamsDto) =>
        await this.Client.GetAsync($"/api/v1/permissions/search?{queryParamsDto.ToQueryString()}");

    protected async Task<HttpResponseMessage> FindPermissions(QueryParamsDto queryParams) =>
        await this.Client.GetAsync($"/api/v1/permissions/find{queryParams.ToQueryString()}");

    protected async Task<PermissionResponseDto> AssertSuccessfulPermissionListResponse(
        HttpResponseMessage response,
        List<Permission> expectedPermissions)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        PermissionResponseDto? content = await response.Content.ReadFromJsonAsync<PermissionResponseDto>();
        content.Should().NotBeNull();
        content!.Total.Should().Be(expectedPermissions.Count);
        content.Items.Should().HaveCount(Math.Min(expectedPermissions.Count, content.PageSize));
        return content;
    }

    #endregion

    #region Asset API Methods

    protected async Task<HttpResponseMessage> GetAssets(QueryParamsDto queryParams) =>
        await this.Client.GetAsync($"/api/v1/assets{queryParams.ToQueryString()}");

    protected async Task<HttpResponseMessage> CreateAsset(CreateAssetDto createDto)
    {
        MultipartFormDataContent formContent = new MultipartFormDataContent();

        if (createDto.Asset != null)
        {
            StreamContent fileContent = new(createDto.Asset.OpenReadStream());
            formContent.Add(fileContent, "Asset", createDto.Asset.FileName);
        }

        formContent.Add(new StringContent(createDto.Name), "Name");

        if (!string.IsNullOrEmpty(createDto.Description))
        {
            formContent.Add(new StringContent(createDto.Description), "Description");
        }

        if (createDto.WorkAssetId.HasValue)
        {
            formContent.Add(new StringContent(createDto.WorkAssetId.Value.ToString()), "WorkAssetId");
        }

        if (createDto.CreatedById.HasValue)
        {
            formContent.Add(new StringContent(createDto.CreatedById.Value.ToString()), "CreatedById");
        }

        return await this.Client.PostAsync("/api/v1/assets", formContent);
    }

    protected async Task<HttpResponseMessage> UpdateAsset(long id, UpdateAssetDto updateDto)
    {
        MultipartFormDataContent formContent = new MultipartFormDataContent();

        if (updateDto.File != null)
        {
            StreamContent fileContent = new(updateDto.File.OpenReadStream());
            formContent.Add(fileContent, "File", updateDto.File.FileName);
        }

        formContent.Add(new StringContent(updateDto.Name), "Name");

        if (!string.IsNullOrEmpty(updateDto.Description))
        {
            formContent.Add(new StringContent(updateDto.Description), "Description");
        }

        formContent.Add(new StringContent(updateDto.WorkItemId.ToString()), "WorkItemId");

        return await this.Client.PutAsync($"/api/v1/assets/{id}", formContent);
    }

    protected async Task<HttpResponseMessage> DeleteAsset(long id) =>
        await this.Client.DeleteAsync($"/api/v1/assets/{id}");

    protected async Task<HttpResponseMessage> FindAssets(QueryParamsDto findParams) =>
        await this.Client.GetAsync($"/api/v1/assets/find{findParams.ToQueryString()}");

    protected async Task<HttpResponseMessage> SearchAssets(DynamicQueryAssetParamsDto searchParams) =>
        await this.Client.GetAsync($"/api/v1/assets/search{searchParams.ToQueryString()}");

    #endregion

    #region Helper Methods

    protected async Task AssertResponseStatusCode(HttpResponseMessage response, HttpStatusCode expectedStatusCode) =>
        response.StatusCode.Should().Be(expectedStatusCode);

    protected async Task<TResponse?> ReadResponseContent<TResponse>(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<TResponse>();

    protected async Task<AssetResponseDto> AssertSuccessfulAssetListResponse(HttpResponseMessage response,
        List<Asset> expectedAssets)
    {
        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);
        AssetResponseDto result = await this.ReadResponseContent<AssetResponseDto>(response);
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        return result;
    }

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

    protected async Task SeedAssets(List<Asset> assets)
    {
        await this.DbContext.Assets.AddRangeAsync(assets);
        await this.DbContext.SaveChangesAsync();
    }

    protected async Task ClearDatabaseAsync()
    {
        this.DbContext.PermissionWorkItems.RemoveRange(await this.DbContext.PermissionWorkItems.ToListAsync());
        this.DbContext.RolePermissions.RemoveRange(await this.DbContext.RolePermissions.ToListAsync());
        this.DbContext.UserRoles.RemoveRange(await this.DbContext.UserRoles.ToListAsync());
        this.DbContext.Tasks.RemoveRange(await this.DbContext.Tasks.ToListAsync());
        this.DbContext.Permissions.RemoveRange(await this.DbContext.Permissions.ToListAsync());
        this.DbContext.Roles.RemoveRange(await this.DbContext.Roles.ToListAsync());
        this.DbContext.Users.RemoveRange(await this.DbContext.Users.ToListAsync());
        this.DbContext.Assets.RemoveRange(await this.DbContext.Assets.ToListAsync());
        await this.DbContext.SaveChangesAsync();
    }

    #endregion
}
