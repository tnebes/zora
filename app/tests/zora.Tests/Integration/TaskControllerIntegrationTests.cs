#region

using System.Net;
using FluentAssertions;
using zora.Core.Domain;
using zora.Core.DTOs.Tasks;
using zora.Core.Enums;
using zora.Core.Utilities;
using zora.Tests.TestFixtures.v2;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.Integration;

[Collection("TestCollectionV2")]
public sealed partial class TaskControllerIntegrationTests : BaseIntegrationTest
{
    private readonly TaskUtils _taskUtils;

    public TaskControllerIntegrationTests() => this._taskUtils = new TaskUtils(this.DbContext);

    [Fact(DisplayName = "GIVEN a task and an admin user WHEN AssignTask() is called THEN the task is assigned to the specified user")]
    public async Task AssignTask_AdminUser_TaskIsAssigned()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role adminRole = RoleUtils.GetValidRole();
        adminRole.Name = "Admin";

        ZoraTask task = this._taskUtils.GetValidTask();
        task.AssigneeId = null;

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [adminRole]);
        await this.DbContext.Tasks.AddAsync(task);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id, Role = adminRole, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        AssignTaskDto assignTaskDto = new() { AssigneeId = user.Id };

        HttpResponseMessage response = await this.AssignTask(task.Id, assignTaskDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.AssigneeId.Should().Be(user.Id);
    }

    [Fact(DisplayName = "GIVEN a task and a user with write permission WHEN AssignTask() is called THEN the task is assigned to the specified user")]
    public async Task AssignTask_UserWithWritePermission_TaskIsAssigned()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Write);
        ZoraTask task = this._taskUtils.GetValidTask();
        task.AssigneeId = null;

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

        AssignTaskDto assignTaskDto = new() { AssigneeId = user.Id };

        HttpResponseMessage response = await this.AssignTask(task.Id, assignTaskDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.AssigneeId.Should().Be(user.Id);
    }

    [Fact(DisplayName = "GIVEN a task and a user with read permission WHEN AssignTask() is called THEN forbidden")]
    public async Task AssignTask_UserWithReadPermission_ReturnsForbidden()
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

        AssignTaskDto assignTaskDto = new() { AssigneeId = user.Id };

        HttpResponseMessage response = await this.AssignTask(task.Id, assignTaskDto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "GIVEN a task assigned to a user WHEN CompleteTask() is called by the assignee THEN the task is completed")]
    public async Task CompleteTask_AssignedUser_TaskIsCompleted()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Read);
        ZoraTask task = this._taskUtils.GetValidTask();
        task.AssigneeId = user.Id;
        task.Status = "In Progress";
        task.CompletionPercentage = 75;

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

        CompleteTaskDto completeTaskDto = new() { Completed = true };

        HttpResponseMessage response = await this.CompleteTask(task.Id, completeTaskDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Status.Should().Be("Completed");
        result.CompletionPercentage.Should().Be(100);
    }

    [Fact(DisplayName = "GIVEN a task not assigned to a user WHEN CompleteTask() is called by a non-admin THEN forbidden")]
    public async Task CompleteTask_NonAssignedNonAdminUser_ReturnsForbidden()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        User otherUser = new() { Id = 2, Username = "other.user", Email = "other@user.com", Password = "password" };
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Read);
        ZoraTask task = this._taskUtils.GetValidTask();
        task.AssigneeId = otherUser.Id;

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user, otherUser]);
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

        CompleteTaskDto completeTaskDto = new() { Completed = true };

        HttpResponseMessage response = await this.CompleteTask(task.Id, completeTaskDto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "GIVEN a task assigned to a user WHEN CompleteTask() is called by an admin THEN the task is completed")]
    public async Task CompleteTask_AdminUser_TaskIsCompleted()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        User otherUser = new() { Id = 2, Username = "other.user", Email = "other@user.com", Password = "password" };
        Role adminRole = RoleUtils.GetValidRole();
        adminRole.Name = "Admin";

        ZoraTask task = this._taskUtils.GetValidTask();
        task.AssigneeId = otherUser.Id;
        task.Status = "In Progress";
        task.CompletionPercentage = 50;

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user, otherUser]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [adminRole]);
        await this.DbContext.Tasks.AddAsync(task);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id, Role = adminRole, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        CompleteTaskDto completeTaskDto = new() { Completed = true };

        HttpResponseMessage response = await this.CompleteTask(task.Id, completeTaskDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Status.Should().Be("Completed");
        result.CompletionPercentage.Should().Be(100);
    }

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

    [Fact(DisplayName = "GIVEN a task and a user with delete permission WHEN Delete() is called THEN the task is deleted")]
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

    [Fact(DisplayName = "GIVEN a user with permission 01001 (Delete and Read) WHEN Delete() is called THEN the task is deleted")]
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
