#region

using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using zora.Core.Domain;
using zora.Core.DTOs.Permissions;
using zora.Core.DTOs.Requests;
using zora.Core.Interfaces.Services;
using zora.Tests.TestFixtures.v2;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.Integration;

public sealed class PermissionExceptionThrowingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            Mock<IPermissionService> mockPermissionService = new Mock<IPermissionService>();

            mockPermissionService
                .Setup(service => service.FindAsync(It.IsAny<QueryParamsDto>()))
                .ThrowsAsync(new Exception("Simulated exception for testing"));

            mockPermissionService
                .Setup(service => service.SearchAsync(It.IsAny<DynamicQueryPermissionParamsDto>()))
                .ThrowsAsync(new Exception("Simulated exception for search testing"));

            services.Replace(ServiceDescriptor.Scoped(_ => mockPermissionService.Object));

            services.Configure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
                options.DefaultScheme = "TestScheme";
            });

            services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
        });
    }
}

[Collection("TestCollectionV2")]
public sealed class PermissionControllerIntegrationTests : BaseIntegrationTest
{
    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto and an admin user WHEN GetPermissions() is invoked THEN the controller returns an OK result with the expected paginated permission list")]
    public async Task GetPermissions_WithValidQueryParamsAndAdminUser_ReturnsOkWithPaginatedPermissionList()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        List<Permission> permissions = PermissionUtils.GetValidPermissions().ToList();
        await this.SeedPermissions(permissions);

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();
        HttpResponseMessage response = await this.GetPermissions(queryParams);

        await this.AssertSuccessfulPermissionListResponse(response, permissions);
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Get method is called THEN return a 401 Unauthorized")]
    public async Task GetPermissions_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabaseAsync();
        this.SetupRegularUserAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();
        HttpResponseMessage response = await this.GetPermissions(queryParams);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Get method is called with query parameters THEN normalize the query parameters and return filtered results")]
    public async Task GetPermissions_WithQueryParameters_NormalizeQueryParametersAndReturnFilteredResults()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        List<Permission> permissions = PermissionUtils.GetValidPermissions().ToList();
        await this.SeedPermissions(permissions);

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetLargePageSizeQueryParams();
        HttpResponseMessage response = await this.GetPermissions(queryParams);

        PermissionResponseDto result = await this.AssertSuccessfulPermissionListResponse(response, permissions);

        // Additional assertions for the sorted results
        if (result.Items.Count() >= 2)
        {
            string firstName = result.Items.First().Name;
            string secondName = result.Items.ElementAt(1).Name;
            firstName.CompareTo(secondName).Should().BeLessThan(0); // Alphabetical order check
        }
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Create method is called with valid permission data THEN return a 201 Created with the new permission")]
    public async Task CreatePermission_WithAdminUserAndValidData_ReturnsCreatedWithNewPermission()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        CreatePermissionDto createPermissionDto = QueryUtils.QueryParamUtils.GetValidCreatePermissionDto();

        HttpResponseMessage response = await this.CreatePermission(createPermissionDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Created);

        PermissionDto? createdPermission = await this.ReadResponseContent<PermissionDto>(response);
        createdPermission.Should().NotBeNull();
        createdPermission!.Name.Should().Be("TestPermission");

        Permission? dbPermission =
            await this.DbContext.Permissions.FirstOrDefaultAsync(p => p.Name == "TestPermission");
        dbPermission.Should().NotBeNull();
        dbPermission!.Name.Should().Be("TestPermission");
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Create method is called THEN return a 401 Unauthorized")]
    public async Task CreatePermission_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabaseAsync();
        this.SetupRegularUserAuthentication();

        CreatePermissionDto createPermissionDto = new()
        {
            Name = "TestPermission",
            Description = "Test Description",
            PermissionString = "test.permission"
        };

        HttpResponseMessage response = await this.CreatePermission(createPermissionDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Create method is called with invalid permission data THEN return a 400 Bad Request")]
    public async Task CreatePermission_WithAdminUserAndInvalidData_ReturnsBadRequest()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        CreatePermissionDto createPermissionDto = new()
        {
            Name = string.Empty,
            Description = "Test Description",
            PermissionString = "test.permission"
        };

        HttpResponseMessage response = await this.CreatePermission(createPermissionDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Update method is called with valid permission data THEN return a 200 OK with the updated permission")]
    public async Task UpdatePermission_WithAdminUserAndValidData_ReturnsOkWithUpdatedPermission()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        Permission permission = PermissionUtils.GetValidPermissions().First();
        await this.SeedPermissions(new List<Permission> { permission });

        Permission? beforePermission =
            await this.DbContext.Permissions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == permission.Id);
        beforePermission.Should().NotBeNull();
        beforePermission!.Name.Should().Be(permission.Name);
        beforePermission.Description.Should().Be(permission.Description);
        beforePermission.PermissionString.Should().Be(permission.PermissionString);

        UpdatePermissionDto updatePermissionDto = PermissionUtils.GetValidUpdatePermissionDto();
        HttpResponseMessage response = await this.UpdatePermission(permission.Id, updatePermissionDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);

        PermissionDto? updatedPermission = await this.ReadResponseContent<PermissionDto>(response);
        updatedPermission.Should().NotBeNull();
        updatedPermission!.Name.Should().Be(updatePermissionDto.Name);
        updatedPermission.Description.Should().Be(updatePermissionDto.Description);
        updatedPermission.PermissionString.Should().Be(updatePermissionDto.PermissionString);

        Permission? dbPermission =
            await this.DbContext.Permissions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == permission.Id);
        dbPermission.Should().NotBeNull();
        dbPermission!.Name.Should().Be(updatePermissionDto.Name);
        dbPermission.Description.Should().Be(updatePermissionDto.Description);
        dbPermission.PermissionString.Should().Be(updatePermissionDto.PermissionString);
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Update method is called THEN return a 401 Unauthorized")]
    public async Task UpdatePermission_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabaseAsync();
        this.SetupRegularUserAuthentication();

        UpdatePermissionDto updatePermissionDto = PermissionUtils.GetValidUpdatePermissionDto();

        HttpResponseMessage response = await this.UpdatePermission(1, updatePermissionDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Update method is called with a non-existent permission ID THEN return a 404 Not Found")]
    public async Task UpdatePermission_WithNonExistentPermissionId_ReturnsNotFound()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        UpdatePermissionDto updatePermissionDto = PermissionUtils.GetValidUpdatePermissionDto();

        HttpResponseMessage response = await this.UpdatePermission(99999, updatePermissionDto);

        await this.AssertResponseStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Delete method is called with a valid permission ID THEN return a 204 No Content")]
    public async Task DeletePermission_WithAdminUserAndValidPermissionId_ReturnsNoContent()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        Permission permission = PermissionUtils.GetValidPermissions().First();
        await this.SeedPermissions(new List<Permission> { permission });

        Permission? beforePermission =
            await this.DbContext.Permissions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == permission.Id);
        beforePermission.Should().NotBeNull();
        beforePermission!.Deleted.Should().BeFalse();

        HttpResponseMessage response = await this.DeletePermission(permission.Id);

        await this.AssertResponseStatusCode(response, HttpStatusCode.NoContent);

        this.DbContext.ChangeTracker.Clear();

        Permission? dbPermission = await this.DbContext.Permissions.FindAsync(permission.Id);
        dbPermission.Should().NotBeNull();
        dbPermission!.Deleted.Should().BeTrue();
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Delete method is called THEN return a 401 Unauthorized")]
    public async Task DeletePermission_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabaseAsync();
        this.SetupRegularUserAuthentication();

        HttpResponseMessage response = await this.DeletePermission(1);

        await this.AssertResponseStatusCode(response, HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Delete method is called with a non-existent permission ID THEN return a 404 Not Found")]
    public async Task DeletePermission_WithNonExistentPermissionId_ReturnsNotFound()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        HttpResponseMessage response = await this.DeletePermission(99999);

        await this.AssertResponseStatusCode(response, HttpStatusCode.NotFound);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Find method is called with valid search parameters THEN return a 200 OK with matching permissions")]
    public async Task FindPermissions_WithAdminUserAndValidParams_ReturnsOkWithMatchingPermissions()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        List<Permission> permissions = PermissionUtils.GetValidPermissions().ToList();
        await this.SeedPermissions(permissions);

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetSearchQueryParams("Create");
        HttpResponseMessage response = await this.FindPermissions(queryParams);

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);

        PermissionResponseDto? result = await this.ReadResponseContent<PermissionResponseDto>(response);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName =
        "GIVEN a service that throws an exception WHEN the Find method is called THEN return a 500 Internal Server Error")]
    public async Task FindPermissions_WithServiceException_ReturnsInternalServerError()
    {
        using PermissionExceptionThrowingWebApplicationFactory exceptionFactory = new();
        HttpClient exceptionClient = exceptionFactory.CreateClient();

        DynamicQueryPermissionParamsDto queryParams = new()
        {
            Page = 1,
            PageSize = 10,
            Name = "first"
        };

        HttpResponseMessage response =
            await exceptionClient.GetAsync($"/api/v1/permissions/find?{queryParams.ToQueryString()}");

        await this.AssertResponseStatusCode(response, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Search method is called with valid search parameters THEN return a 200 OK with matching permissions")]
    public async Task SearchPermissions_WithAdminUserAndValidParams_ReturnsOkWithMatchingPermissions()
    {
        await this.ClearDatabaseAsync();
        this.SetupAdminAuthentication();

        List<Permission> permissions = PermissionUtils.GetValidPermissions().ToList();
        await this.SeedPermissions(permissions);

        DynamicQueryPermissionParamsDto queryParams = new()
        {
            Page = 1,
            PageSize = 10,
            PermissionString = permissions.First().PermissionString
        };

        HttpResponseMessage response = await this.SearchPermissions(queryParams);

        await this.AssertResponseStatusCode(response, HttpStatusCode.OK);

        PermissionResponseDto? result = await this.ReadResponseContent<PermissionResponseDto>(response);
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user and a service that throws an exception WHEN the Search method is called THEN return a 500 Internal Server Error")]
    public async Task SearchPermissions_WithServiceException_ReturnsInternalServerError()
    {
        using PermissionExceptionThrowingWebApplicationFactory exceptionFactory = new();
        this.SetupAdminAuthentication();
        HttpClient exceptionClient = exceptionFactory.CreateClient();
        exceptionClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test", "Admin");

        DynamicQueryPermissionParamsDto queryParams = new()
        {
            Page = 1,
            PageSize = 10,
            Name = "first"
        };

        HttpResponseMessage response =
            await exceptionClient.GetAsync($"/api/v1/permissions/search{queryParams.ToQueryString()}");

        await this.AssertResponseStatusCode(response, HttpStatusCode.InternalServerError);
    }
}
