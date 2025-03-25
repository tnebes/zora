using System.Net;
using FluentAssertions;
using zora.Core.Domain;
using zora.Core.Enums;
using zora.Tests.Utils;

namespace zora.Tests.Integration;

public sealed partial class TaskControllerIntegrationTests
{
    [Fact(DisplayName = "GIVEN a task and an admin user WHEN Delete() is called THEN the task is deleted")]
    public async Task Delete_AdminUser_TaskIsDeleted()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role adminRole = RoleUtils.GetValidRole();
        adminRole.Name = "Admin";

        ZoraTask task = this._taskUtils.GetValidTask();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [adminRole]);
        await this.DbContext.Tasks.AddAsync(task);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id, Role = adminRole, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        HttpResponseMessage response = await this.DeleteTask(task.Id);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        bool result = await this.ReadResponseContent<bool>(response);
        result.Should().BeTrue();

        this.DbContext.ChangeTracker.Clear();
        ZoraTask? deletedTask = await this.DbContext.Tasks.FindAsync(task.Id);
        deletedTask.Deleted.Should().BeTrue();
    }

    [Fact(DisplayName =
        "GIVEN a task and a user with delete permission WHEN Delete() is called THEN the task is deleted")]
    public async Task Delete_UserWithDeletePermission_TaskIsDeleted()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Delete);
        ZoraTask task = this._taskUtils.GetValidTask();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);
        await this.DbContext.Tasks.AddAsync(task);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        PermissionWorkItem permissionWorkItem = new PermissionWorkItem
        {
            PermissionId = permission.Id,
            WorkItemId = task.Id
        };
        await this.DbContext.PermissionWorkItems.AddAsync(permissionWorkItem);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.DeleteTask(task.Id);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        bool result = await this.ReadResponseContent<bool>(response);
        result.Should().BeTrue();

        this.DbContext.ChangeTracker.Clear();
        ZoraTask? deletedTask = await this.DbContext.Tasks.FindAsync(task.Id);
        deletedTask.Deleted.Should().BeTrue();
    }

    [Fact(DisplayName = "GIVEN a task and a user with read permission WHEN Delete() is called THEN forbidden")]
    public async Task Delete_UserWithReadPermission_ReturnsForbidden()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Read);
        ZoraTask task = this._taskUtils.GetValidTask();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);
        await this.DbContext.Tasks.AddAsync(task);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        PermissionWorkItem permissionWorkItem = new PermissionWorkItem
        {
            PermissionId = permission.Id,
            WorkItemId = task.Id
        };
        await this.DbContext.PermissionWorkItems.AddAsync(permissionWorkItem);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.DeleteTask(task.Id);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        ZoraTask? taskAfterAttempt = await this.DbContext.Tasks.FindAsync(task.Id);
        taskAfterAttempt.Deleted.Should().BeFalse();
    }

    [Fact(DisplayName =
        "GIVEN a user with permission 01001 (Delete and Read) WHEN Delete() is called THEN the task is deleted")]
    public async Task Delete_UserWithReadAndReadPermission_TaskIsDeleted()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Delete | PermissionFlag.Read);
        ZoraTask task = this._taskUtils.GetValidTask();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);
        await this.DbContext.Tasks.AddAsync(task);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        PermissionWorkItem permissionWorkItem = new PermissionWorkItem
        {
            PermissionId = permission.Id,
            WorkItemId = task.Id
        };
        await this.DbContext.PermissionWorkItems.AddAsync(permissionWorkItem);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.DeleteTask(task.Id);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        bool result = await this.ReadResponseContent<bool>(response);
        result.Should().BeTrue();

        this.DbContext.ChangeTracker.Clear();
        ZoraTask? deletedTask = await this.DbContext.Tasks.FindAsync(task.Id);
        deletedTask.Deleted.Should().BeTrue();
    }

    [Fact(DisplayName = "GIVEN a user with permission 00011 (Write and Read) WHEN Delete() is called THEN forbidden")]
    public async Task Delete_UserWithWriteAndReadPermission_ReturnsForbidden()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Write | PermissionFlag.Read);
        ZoraTask task = this._taskUtils.GetValidTask();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);
        await this.DbContext.Tasks.AddAsync(task);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        PermissionWorkItem permissionWorkItem = new PermissionWorkItem
        {
            PermissionId = permission.Id,
            WorkItemId = task.Id
        };
        await this.DbContext.PermissionWorkItems.AddAsync(permissionWorkItem);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.DeleteTask(task.Id);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        ZoraTask? taskAfterAttempt = await this.DbContext.Tasks.FindAsync(task.Id);
        taskAfterAttempt.Deleted.Should().BeFalse();
    }
}
