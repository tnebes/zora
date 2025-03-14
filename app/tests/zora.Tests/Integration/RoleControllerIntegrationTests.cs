#region

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Roles;
using zora.Core.Interfaces.Services;
using zora.Tests.TestFixtures.v2;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.Integration;

public sealed class ExceptionThrowingWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            Mock<IRoleService> mockRoleService = new Mock<IRoleService>();

            mockRoleService
                .Setup(service => service.FindAsync(It.IsAny<QueryParamsDto>()))
                .ThrowsAsync(new Exception("Simulated exception for testing"));

            mockRoleService
                .Setup(service => service.SearchAsync(It.IsAny<DynamicQueryRoleParamsDto>()))
                .ThrowsAsync(new Exception("Simulated exception for search testing"));

            mockRoleService
                .Setup(service => service.IsAdmin(It.IsAny<ClaimsPrincipal>()))
                .Returns(true);

            services.Replace(ServiceDescriptor.Scoped(_ => mockRoleService.Object));

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

    private sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            Claim[] claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim("UserType", "Admin")
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims, "Test");
            ClaimsPrincipal principal = new ClaimsPrincipal(identity);
            AuthenticationTicket ticket = new AuthenticationTicket(principal, "TestScheme");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}

[Collection("TestCollectionV2")]
public sealed class RoleControllerIntegrationTests : BaseIntegrationTest
{
    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto and an admin user WHEN GetRoles() is invoked THEN the controller returns an OK result with the expected paginated role list")]
    public async Task GetRoles_WithValidQueryParamsAndAdminUser_ReturnsOkWithPaginatedRoleList()
    {
        await this.ClearDatabase();

        List<Role> roles = RoleUtils.GetValidRoles().ToList();
        List<RoleDto> expectedRoles = this.Mapper.Map<List<RoleDto>>(roles);

        await this.SeedRoles(roles);
        this.SetupAdminAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();

        HttpResponseMessage response = await this.GetRoles(queryParams);

        RoleResponseDto roleResponse = await this.AssertSuccessfulRoleListResponse(response, roles);
        roleResponse.Should().NotBeNull();
        roleResponse.Items.Should().NotBeNull();
        roleResponse.Items.Should().HaveCount(expectedRoles.Count);
        roleResponse.Items.Should().BeEquivalentTo(expectedRoles);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Get method is called THEN return a 200 OK with paginated roles")]
    public async Task GetRoles_WithAdminUser_ReturnsOkWithPaginatedRoles()
    {
        await this.ClearDatabase();

        List<Role> roles = RoleUtils.GetValidRoles().ToList();
        List<RoleDto> expectedRoles = this.Mapper.Map<List<RoleDto>>(roles);

        await this.SeedRoles(roles);
        this.SetupAdminAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();

        HttpResponseMessage response = await this.GetRoles(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RoleResponseDto? content = await response.Content.ReadFromJsonAsync<RoleResponseDto>();
        content.Should().NotBeNull();
        content!.Total.Should().Be(expectedRoles.Count);
        content.Items.Should().HaveCount(Math.Min(roles.Count, content.PageSize));
        content.Items.Should().BeEquivalentTo(expectedRoles);
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Get method is called THEN return a 401 Unauthorized")]
    public async Task GetRoles_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabase();

        List<Role> roles = RoleUtils.GetValidRoles().ToList();
        await this.SeedRoles(roles);

        this.SetupRegularUserAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetValidQueryParams();

        HttpResponseMessage response = await this.GetRoles(queryParams);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Get method is called with query parameters THEN normalize the query parameters and return filtered results")]
    public async Task GetRoles_WithQueryParameters_NormalizeQueryParametersAndReturnFilteredResults()
    {
        await this.ClearDatabase();

        List<Role> roles = RoleUtils.GetValidRoles().ToList();
        List<RoleDto> expectedRoles = this.Mapper.Map<List<RoleDto>>(roles);

        await this.SeedRoles(roles);
        this.SetupAdminAuthentication();

        QueryParamsDto queryParams = new QueryParamsDto
        {
            Page = 0,
            PageSize = 0,
            SearchTerm = "",
            SortColumn = "invalid",
            SortDirection = "invalid"
        };

        HttpResponseMessage response = await this.GetRoles(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RoleResponseDto? content = await response.Content.ReadFromJsonAsync<RoleResponseDto>();
        content.Should().NotBeNull();
        content!.Page.Should().BeGreaterThan(0);
        content.PageSize.Should().BeGreaterThan(0);
        content.Total.Should().Be(expectedRoles.Count);
        content.Items.Should().BeEquivalentTo(expectedRoles);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Create method is called with valid role data THEN return a 201 Created with the new role")]
    public async Task CreateRole_WithAdminUserAndValidData_ReturnsCreatedWithNewRole()
    {
        await this.ClearDatabase();

        this.SetupAdminAuthentication();

        CreateRoleDto createRoleDto = new CreateRoleDto
        {
            Name = "TestRole",
            PermissionIds = new List<long>()
        };

        HttpResponseMessage response = await this.CreateRole(createRoleDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        Role? createdRole = await response.Content.ReadFromJsonAsync<Role>();
        createdRole.Should().NotBeNull();
        createdRole!.Name.Should().Be("TestRole");

        Role? dbRole = await this.DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "TestRole");
        dbRole.Should().NotBeNull();
        dbRole!.Name.Should().Be("TestRole");
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Create method is called THEN return a 401 Unauthorized")]
    public async Task CreateRole_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabase();

        this.SetupRegularUserAuthentication();

        CreateRoleDto createRoleDto = new CreateRoleDto
        {
            Name = "TestRole",
            PermissionIds = new List<long>()
        };

        HttpResponseMessage response = await this.CreateRole(createRoleDto);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

        Role? dbRole = await this.DbContext.Roles.FirstOrDefaultAsync(r => r.Name == "TestRole");
        dbRole.Should().BeNull();
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Create method is called with invalid role data THEN return a 400 Bad Request")]
    public async Task CreateRole_WithAdminUserAndInvalidData_ReturnsBadRequest()
    {
        await this.ClearDatabase();

        this.SetupAdminAuthentication();

        CreateRoleDto createRoleDto = new CreateRoleDto
        {
            Name = null!,
            PermissionIds = new List<long>()
        };

        HttpResponseMessage response = await this.CreateRole(createRoleDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        int roleCount = await this.DbContext.Roles.CountAsync();
        roleCount.Should().Be(0);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Create method is called with whitespace-only role name THEN return a 400 Bad Request")]
    public async Task CreateRole_WithWhitespaceRoleName_ReturnsBadRequest()
    {
        await this.ClearDatabase();

        this.SetupAdminAuthentication();

        CreateRoleDto createRoleDto = new CreateRoleDto
        {
            Name = "   ",
            PermissionIds = new List<long>()
        };

        HttpResponseMessage response = await this.CreateRole(createRoleDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        int roleCount = await this.DbContext.Roles.CountAsync();
        roleCount.Should().Be(0);
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Update method is called THEN return a 401 Unauthorized")]
    public async Task UpdateRole_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabase();

        Role role = new Role { Name = "TestRole" };
        await this.SeedRoles(new List<Role> { role });

        this.SetupRegularUserAuthentication();

        UpdateRoleDto updateRoleDto = new UpdateRoleDto
        {
            Name = "UpdatedRole",
            PermissionIds = new List<long>()
        };

        HttpResponseMessage response = await this.UpdateRole(role.Id, updateRoleDto);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

        Role? dbRole = await this.DbContext.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);
        dbRole.Should().NotBeNull();
        dbRole!.Name.Should().Be("TestRole"); // Ensure the role name wasn't updated
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Update method is called with a non-existent role ID THEN return a 404 Not Found")]
    public async Task UpdateRole_WithNonExistentRoleId_ReturnsNotFound()
    {
        await this.ClearDatabase();

        this.SetupAdminAuthentication();

        long nonExistentRoleId = 9999;

        UpdateRoleDto updateRoleDto = new UpdateRoleDto
        {
            Name = "UpdatedRole",
            PermissionIds = new List<long>()
        };

        HttpResponseMessage response = await this.UpdateRole(nonExistentRoleId, updateRoleDto);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Update method is called with whitespace-only role name THEN return a 400 Bad Request")]
    public async Task UpdateRole_WithWhitespaceRoleName_ReturnsBadRequest()
    {
        await this.ClearDatabase();

        Role role = new Role { Name = "TestRole" };
        await this.SeedRoles(new List<Role> { role });

        this.SetupAdminAuthentication();

        UpdateRoleDto updateRoleDto = new UpdateRoleDto
        {
            Name = "   ",
            PermissionIds = new List<long>()
        };

        HttpResponseMessage response = await this.UpdateRole(role.Id, updateRoleDto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        Role? dbRole = await this.DbContext.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);
        dbRole.Should().NotBeNull();
        dbRole!.Name.Should().Be("TestRole");
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Delete method is called with a valid role ID THEN return a 204 No Content")]
    public async Task DeleteRole_WithAdminUserAndValidRoleId_ReturnsNoContent()
    {
        await this.ClearDatabase();

        Role role = new Role { Name = "RoleToDelete" };
        await this.SeedRoles(new List<Role> { role });

        long roleId = role.Id;

        this.SetupAdminAuthentication();

        Role? beforeRole = await this.DbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
        beforeRole.Should().NotBeNull();
        beforeRole!.Deleted.Should().BeFalse();

        HttpResponseMessage response = await this.DeleteRole(roleId);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        this.DbContext.ChangeTracker.Clear();

        Role? dbRole = await this.DbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
        dbRole.Should().NotBeNull();
        dbRole!.Deleted.Should().BeTrue();
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Delete method is called THEN return a 401 Unauthorized")]
    public async Task DeleteRole_WithNonAdminUser_ReturnsUnauthorized()
    {
        await this.ClearDatabase();

        Role role = new Role { Name = "RoleToNotDelete" };
        await this.SeedRoles(new List<Role> { role });

        long roleId = role.Id;

        this.SetupRegularUserAuthentication();

        HttpResponseMessage response = await this.DeleteRole(roleId);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);

        Role? dbRole = await this.DbContext.Roles.AsNoTracking().FirstOrDefaultAsync(r => r.Id == roleId);
        dbRole.Should().NotBeNull();
        dbRole!.Name.Should().Be("RoleToNotDelete");
        dbRole.Deleted.Should().BeFalse();
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Delete method is called with a non-existent role ID THEN return a 404 Not Found")]
    public async Task DeleteRole_WithNonExistentRoleId_ReturnsNotFound()
    {
        await this.ClearDatabase();

        this.SetupAdminAuthentication();

        long nonExistentRoleId = 9999;

        HttpResponseMessage response = await this.DeleteRole(nonExistentRoleId);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Find method is called with valid search parameters THEN return a 200 OK with matching roles")]
    public async Task FindRoles_WithAdminUserAndValidParams_ReturnsOkWithMatchingRoles()
    {
        await this.ClearDatabase();

        List<Role> roles = RoleUtils.GetFindTestRoles().ToList();
        await this.SeedRoles(roles);
        this.SetupAdminAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetSearchQueryParamsForAdminUser();

        HttpResponseMessage response = await this.FindRoles(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        RoleResponseDto? responseContent = await this.ReadResponseContent<RoleResponseDto>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Items.Should().NotBeNull();
        responseContent.Items.Should().HaveCount(1);
        responseContent.Items.First().Name.Should().Be("Admin Role");
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Find method is called THEN return a 200 OK with normalized query parameters")]
    public async Task FindRoles_WithNonAdminUser_ReturnsOkWithNormalizedParams()
    {
        await this.ClearDatabase();

        List<Role> roles = RoleUtils.GetFindTestRoles().ToList();
        await this.SeedRoles(roles);

        this.SetupRegularUserAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetInvalidQueryParams();

        HttpResponseMessage response = await this.FindRoles(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        RoleResponseDto? responseContent = await this.ReadResponseContent<RoleResponseDto>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Items.Should().NotBeNull();
        responseContent.PageSize.Should().Be(50);
        responseContent.Page.Should().Be(1);
    }

    [Fact(DisplayName =
        "GIVEN a logged in user WHEN the Find method is called with invalid search parameters THEN return a 200 OK with normalized query parameters")]
    public async Task FindRoles_WithInvalidParams_ReturnsBadRequest()
    {
        await this.ClearDatabase();

        this.SetupAdminAuthentication();

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetInvalidQueryParams();

        queryParams.SortColumn = "NonExistentColumn";

        HttpResponseMessage response = await this.FindRoles(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName =
        "GIVEN a service that throws an exception WHEN the Find method is called THEN return a 500 Internal Server Error")]
    public async Task FindRoles_WithServiceException_ReturnsInternalServerError()
    {
        await using ExceptionThrowingWebApplicationFactory factory = new ExceptionThrowingWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

        QueryParamsDto queryParams = QueryUtils.QueryParamUtils.GetSearchQueryParams();

        HttpResponseMessage response = await client.GetAsync(
            $"/api/v1/roles/find?page={queryParams.Page}&pageSize={queryParams.PageSize}&sortColumn={queryParams.SortColumn}&sortDirection={queryParams.SortDirection}&searchTerm={Uri.EscapeDataString(queryParams.SearchTerm ?? string.Empty)}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName =
        "GIVEN a logged in admin user WHEN the Search method is called with valid search parameters THEN return a 200 OK with matching roles")]
    public async Task SearchRoles_WithAdminUserAndValidParams_ReturnsOkWithMatchingRoles()
    {
        await this.ClearDatabase();

        List<Role> roles = RoleUtils.GetFindTestRoles().ToList();
        await this.SeedRoles(roles);
        this.SetupAdminAuthentication();

        DynamicQueryRoleParamsDto queryParams = QueryUtils.DynamicQueryParamsUtils.GetDynamicRoleSearchQueryParams();

        HttpResponseMessage response = await this.SearchRoles(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        RoleResponseDto? responseContent = await this.ReadResponseContent<RoleResponseDto>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Items.Should().NotBeNull();
        responseContent.Items.Should().HaveCount(1);
        responseContent.Items.First().Name.Should().Be("Admin Role");
    }

    [Fact(DisplayName =
        "GIVEN a logged in non-admin user WHEN the Search method is called THEN return a 200 OK with normalized query parameters")]
    public async Task SearchRoles_WithNonAdminUser_ReturnsOkWithNormalizedParams()
    {
        await this.ClearDatabase();

        List<Role> roles = RoleUtils.GetFindTestRoles().ToList();
        await this.SeedRoles(roles);

        this.SetupRegularUserAuthentication();

        DynamicQueryRoleParamsDto queryParams = QueryUtils.DynamicQueryParamsUtils.GetInvalidDynamicRoleQueryParams();

        HttpResponseMessage response = await this.SearchRoles(queryParams);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        RoleResponseDto? responseContent = await this.ReadResponseContent<RoleResponseDto>(response);
        responseContent.Should().NotBeNull();
        responseContent!.Items.Should().NotBeNull();
        responseContent.PageSize.Should().Be(50);
        responseContent.Page.Should().Be(1);
    }

    [Fact(DisplayName =
        "GIVEN a service that throws an exception WHEN the Search method is called THEN return a 500 Internal Server Error")]
    public async Task SearchRoles_WithServiceException_ReturnsInternalServerError()
    {
        await using ExceptionThrowingWebApplicationFactory factory = new ExceptionThrowingWebApplicationFactory();
        HttpClient client = factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

        DynamicQueryRoleParamsDto queryParams = QueryUtils.DynamicQueryParamsUtils.GetDynamicRoleSearchQueryParams();

        HttpResponseMessage response = await client.GetAsync(
            $"/api/v1/roles/search?{queryParams.ToQueryString()}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
