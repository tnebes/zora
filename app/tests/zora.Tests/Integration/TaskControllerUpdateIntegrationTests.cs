#region

using System.Net;
using FluentAssertions;
using zora.Core.Domain;
using zora.Core.DTOs.Tasks;
using zora.Core.Enums;
using zora.Tests.Utils;

#endregion

namespace zora.Tests.Integration;

public sealed partial class TaskControllerIntegrationTests
{
    [Fact(DisplayName = "GIVEN an admin user and a task WHEN Update() is called THEN return the task")]
    public async Task UpdateTask_AdminUser_ReturnsTask()
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

        UpdateTaskDto updateDto = this.Mapper.Map<UpdateTaskDto>(task);
        updateDto.Name = "Updated Task";
        updateDto.Description = "Updated Description";

        HttpResponseMessage response = await this.UpdateTask(task.Id, updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Name.Should().Be(updateDto.Name);
        result.Description.Should().Be(updateDto.Description);
        result.Status.Should().Be(task.Status);
        result.StartDate.Should().Be(task.StartDate);
        result.DueDate.Should().Be(task.DueDate);
        result.CompletionPercentage.Should().Be(task.CompletionPercentage);
        result.EstimatedHours.Should().Be(task.EstimatedHours);
        result.ActualHours.Should().Be(task.ActualHours);
        result.Priority.Should().Be(task.Priority);
        result.ProjectId.Should().Be(task.ProjectId);
    }

    [Fact(DisplayName =
        "GIVEN a user with MANAGE/ADMIN permissions for a task WHEN Update() is called THEN return the task")]
    public async Task UpdateTask_UserWithManageAdminPermission_ReturnsTask()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Admin);
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

        UpdateTaskDto updateDto = this.Mapper.Map<UpdateTaskDto>(task);
        updateDto.Name = "Updated Task with Admin Permission";
        updateDto.Description = "Updated Description with Admin Permission";

        HttpResponseMessage response = await this.UpdateTask(task.Id, updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
        result.Name.Should().Be(updateDto.Name);
        result.Description.Should().Be(updateDto.Description);
        result.Status.Should().Be(task.Status);
        result.StartDate.Should().Be(task.StartDate);
        result.DueDate.Should().Be(task.DueDate);
        result.CompletionPercentage.Should().Be(task.CompletionPercentage);
        result.EstimatedHours.Should().Be(task.EstimatedHours);
        result.ActualHours.Should().Be(task.ActualHours);
        result.Priority.Should().Be(task.Priority);
        result.ProjectId.Should().Be(task.ProjectId);
    }

    [Fact(DisplayName = "GIVEN a user with read permissions for a task WHEN Update() is called THEN forbidden")]
    public async Task UpdateTask_UserWithReadPermission_ReturnsForbidden()
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

        UpdateTaskDto updateDto = this.Mapper.Map<UpdateTaskDto>(task);
        updateDto.Name = "Updated Task with Read Permission";
        updateDto.Description = "Updated Description with Read Permission";

        HttpResponseMessage response = await this.UpdateTask(task.Id, updateDto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
