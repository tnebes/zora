#region

using System.Net;
using FluentAssertions;
using zora.Core.Domain;
using zora.Core.DTOs.Tasks;
using zora.Core.Enums;
using zora.Tests.TestFixtures.v2;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.Integration;

public sealed class TaskControllerIntegrationTests : BaseIntegrationTest
{
    private readonly TaskUtils _taskUtils;

    public TaskControllerIntegrationTests() => this._taskUtils = new TaskUtils(this.DbContext);

    [Fact(DisplayName =
        "GIVEN 4 tasks of which 3 are visible to the user WHEN Get() is called THEN return 3 tasks for which the user has at least READ permissions with default page size.")]
    public async Task GetTasks_LoggedInUserWith3CreatedTasks_Returns3Tasks()
    {
        await DatabaseSeeder.ClearDatabaseAsync(this.DbContext);

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Read);
        List<ZoraTask> tasks = this._taskUtils.GetValidTasks();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);

        await this.DbContext.Tasks.AddRangeAsync(tasks);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        List<PermissionWorkItem> permissionWorkItems = tasks.Take(3).Select(task => new PermissionWorkItem
            { PermissionId = permission.Id, WorkItemId = task.Id }).ToList();
        await this.DbContext.PermissionWorkItems.AddRangeAsync(permissionWorkItems);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.GetTasks(QueryUtils.QueryParamUtils.GetValidQueryParams());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TaskResponseDto? result = await this.ReadResponseContent<TaskResponseDto>(response);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(3);
    }
}
