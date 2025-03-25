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
    [Fact(DisplayName = "GIVEN an admin user WHEN Create() is called THEN return the created task")]
    public async Task CreateTask_AdminUser_ReturnsCreatedTask()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role adminRole = RoleUtils.GetValidRole();
        adminRole.Name = "Admin";

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [adminRole]);

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = adminRole.Id, Role = adminRole, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        CreateTaskDto createDto = new CreateTaskDto
        {
            Name = "New Task",
            Description = "New Description",
            Status = "Not Started",
            StartDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            CompletionPercentage = 0,
            EstimatedHours = 10,
            ActualHours = 0,
            Priority = "Medium",
            ProjectId = 1
        };

        HttpResponseMessage response = await this.CreateTask(createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.Description.Should().Be(createDto.Description);
        result.Status.Should().Be(createDto.Status);
        result.StartDate.Should().Be(createDto.StartDate);
        result.DueDate.Should().Be(createDto.DueDate);
        result.CompletionPercentage.Should().Be(createDto.CompletionPercentage);
        result.EstimatedHours.Should().Be(createDto.EstimatedHours);
        result.ActualHours.Should().Be(createDto.ActualHours);
        result.Priority.Should().Be(createDto.Priority);
        result.ProjectId.Should().Be(createDto.ProjectId);
    }

    [Fact(DisplayName = "GIVEN a user with write permission WHEN Create() is called THEN return the created task",
        Skip = "Not implemented yet")]
    public async Task CreateTask_UserWithWritePermission_ReturnsCreatedTask()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Write);

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        CreateTaskDto createDto = new CreateTaskDto
        {
            Name = "New Task with Write Permission",
            Description = "New Description with Write Permission",
            Status = "Not Started",
            StartDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            CompletionPercentage = 0,
            EstimatedHours = 10,
            ActualHours = 0,
            Priority = "Medium",
            ProjectId = 1
        };

        HttpResponseMessage response = await this.CreateTask(createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Name.Should().Be(createDto.Name);
        result.Description.Should().Be(createDto.Description);
        result.Status.Should().Be(createDto.Status);
        result.StartDate.Should().Be(createDto.StartDate);
        result.DueDate.Should().Be(createDto.DueDate);
        result.CompletionPercentage.Should().Be(createDto.CompletionPercentage);
        result.EstimatedHours.Should().Be(createDto.EstimatedHours);
        result.ActualHours.Should().Be(createDto.ActualHours);
        result.Priority.Should().Be(createDto.Priority);
        result.ProjectId.Should().Be(createDto.ProjectId);
    }

    [Fact(DisplayName = "GIVEN a user with read permission WHEN Create() is called THEN forbidden",
        Skip = "Not implemented yet")]
    public async Task CreateTask_UserWithReadPermission_ReturnsForbidden()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Read);

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        CreateTaskDto createDto = new CreateTaskDto
        {
            Name = "New Task with Read Permission",
            Description = "New Description with Read Permission",
            Status = "Not Started",
            StartDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            CompletionPercentage = 0,
            EstimatedHours = 10,
            ActualHours = 0,
            Priority = "Medium",
            ProjectId = 1
        };

        HttpResponseMessage response = await this.CreateTask(createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // TODO temporary, remove skip once implemented
    [Fact(DisplayName = "GIVEN a regular user with create permission WHEN Create() is called THEN forbidden")]
    public async Task CreateTask_UserWithCreatePermission_ReturnsForbidden()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Create);

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        CreateTaskDto createDto = new CreateTaskDto
        {
            Name = "New Task with Create Permission",
            Description = "New Description with Create Permission",
            Status = "Not Started",
            StartDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(7),
            CompletionPercentage = 0,
            EstimatedHours = 10,
            ActualHours = 0,
            Priority = "Medium",
            ProjectId = 1
        };

        HttpResponseMessage response = await this.CreateTask(createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
