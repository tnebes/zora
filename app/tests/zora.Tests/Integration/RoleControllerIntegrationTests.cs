#region

using zora.Core.Domain;
using zora.Core.DTOs.Requests;
using zora.Core.DTOs.Responses;
using zora.Tests.TestFixtures.v2;

#endregion

namespace zora.Tests.Integration;

[Collection("TestCollectionV2")]
public sealed class RoleControllerIntegrationTests : BaseIntegrationTest
{
    [Fact(DisplayName =
        "GIVEN a valid QueryParamsDto and an admin user WHEN GetRoles() is invoked THEN the controller returns an OK result with the expected paginated role list")]
    public async Task GetRoles_WithValidQueryParamsAndAdminUser_ReturnsOkWithPaginatedRoleList()
    {
        await this.ClearDatabase();

        List<Role> roles = new List<Role>
        {
            new()
            {
                Id = 1,
                Name = "Admin",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                Name = "User",
                CreatedAt = DateTime.UtcNow
            }
        };

        await this.SeedRoles(roles);
        this.SetupAdminAuthentication();

        QueryParamsDto queryParams = new QueryParamsDto
        {
            Page = 1,
            PageSize = 10,
            SortColumn = "Name",
            SortDirection = "asc"
        };

        HttpResponseMessage response = await this.GetRoles(queryParams);

        RoleResponseDto roleResponse = await this.AssertSuccessfulRoleListResponse(response, roles);
    }
}
