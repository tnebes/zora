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
public sealed class TaskControllerIntegrationTests : BaseIntegrationTest
{
    private readonly TaskUtils _taskUtils;

    public TaskControllerIntegrationTests() => this._taskUtils = new TaskUtils(this.DbContext);

    [Fact(DisplayName =
        "GIVEN different PermissionFlag values WHEN converted to string and back THEN original values are preserved")]
    public void PermissionFlag_ConversionBetweenStringAndEnum_WorksCorrectly()
    {
        Dictionary<PermissionFlag, string> flagToMaskMapping = new Dictionary<PermissionFlag, string>
        {
            { PermissionFlag.None, "00000" },
            { PermissionFlag.Read, "00001" },
            { PermissionFlag.Write, "00010" },
            { PermissionFlag.Create, "00100" },
            { PermissionFlag.Delete, "01000" },
            { PermissionFlag.Admin, "10000" }
        };

        foreach (KeyValuePair<PermissionFlag, string> mapping in flagToMaskMapping)
        {
            PermissionFlag flag = mapping.Key;
            string expectedMask = mapping.Value;

            string actualMask = flag.GetPermissionMask();
            actualMask.Should().Be(expectedMask);

            PermissionFlag convertedFlag = PermissionUtilities.StringToPermissionFlag(actualMask);
            convertedFlag.Should().Be(flag);

            string binaryString = PermissionUtilities.PermissionFlagToString(flag);
            PermissionFlag fromBinaryString = PermissionUtilities.StringToPermissionFlag(binaryString);
            fromBinaryString.Should().Be(flag);
        }

        PermissionFlag combined = PermissionFlag.Read | PermissionFlag.Write;
        string combinedString = PermissionUtilities.PermissionFlagToString(combined);
        combinedString.Should().Be("00011");

        PermissionFlag reconstructed = PermissionUtilities.StringToPermissionFlag(combinedString);
        reconstructed.Should().Be(combined);

        PermissionFlag allFlags = PermissionFlag.Read | PermissionFlag.Write | PermissionFlag.Create |
                                  PermissionFlag.Delete | PermissionFlag.Admin;
        string allFlagsString = PermissionUtilities.PermissionFlagToString(allFlags);
        allFlagsString.Should().Be("11111");

        PermissionFlag reconstructedAllFlags = PermissionUtilities.StringToPermissionFlag(allFlagsString);
        reconstructedAllFlags.Should().Be(allFlags);
    }

    [Fact(DisplayName =
        "GIVEN 4 tasks of which 3 are visible to the user WHEN Get() is called THEN return 3 tasks for which the user has at least READ permissions with default page size.")]
    public async Task GetTasks_LoggedInUserWith3CreatedTasks_Returns3Tasks()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Read);
        List<ZoraTask> tasks = this._taskUtils.GetValidTasks();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);

        await this.DbContext.Tasks.AddRangeAsync(tasks);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
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

    [Fact(DisplayName =
        "GIVEN a task for which the user has a permission set to none WHEN Get() is called THEN return 0 tasks")]
    public async Task GetTasks_LoggedInUserWithNoPermissions_Returns0Tasks()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.None);
        List<ZoraTask> tasks = this._taskUtils.GetValidTasks();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);

        await this.DbContext.Tasks.AddRangeAsync(tasks);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        List<PermissionWorkItem> permissionWorkItems = tasks
            .Select(task => new PermissionWorkItem
            {
                PermissionId = permission.Id,
                WorkItemId = task.Id
            }).ToList();
        await this.DbContext.PermissionWorkItems.AddRangeAsync(permissionWorkItems);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.GetTasks(QueryUtils.QueryParamUtils.GetValidQueryParams());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TaskResponseDto? result = await this.ReadResponseContent<TaskResponseDto>(response);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(0);
    }

    [Fact(DisplayName =
        "GIVEN a task for which the user has a write permission WHEN Get() is called THEN return that task")]
    public async Task GetTasks_LoggedInUserWithWritePermissions_ReturnsTask()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Write);
        List<ZoraTask> tasks = this._taskUtils.GetValidTasks();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);

        await this.DbContext.Tasks.AddRangeAsync(tasks);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        PermissionWorkItem permissionWorkItem = new PermissionWorkItem
        {
            PermissionId = permission.Id,
            WorkItemId = tasks.First().Id
        };
        await this.DbContext.PermissionWorkItems.AddAsync(permissionWorkItem);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.GetTasks(QueryUtils.QueryParamUtils.GetValidQueryParams());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TaskResponseDto? result = await this.ReadResponseContent<TaskResponseDto>(response);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName =
        "GIVEN a task for which the user has a create permission WHEN Get() is called THEN return that task")]
    public async Task GetTasks_LoggedInUserWithCreatePermissions_ReturnsTask()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Create);
        List<ZoraTask> tasks = this._taskUtils.GetValidTasks();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);

        await this.DbContext.Tasks.AddRangeAsync(tasks);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        PermissionWorkItem permissionWorkItem = new PermissionWorkItem
        {
            PermissionId = permission.Id,
            WorkItemId = tasks.First().Id
        };
        await this.DbContext.PermissionWorkItems.AddAsync(permissionWorkItem);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.GetTasks(QueryUtils.QueryParamUtils.GetValidQueryParams());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TaskResponseDto? result = await this.ReadResponseContent<TaskResponseDto>(response);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName =
        "GIVEN a task for which the user has a delete permission WHEN Get() is called THEN return that task")]
    public async Task GetTasks_LoggedInUserWithDeletePermissions_ReturnsTask()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        Permission permission = PermissionUtils.GetValidPermission(PermissionFlag.Delete);
        List<ZoraTask> tasks = this._taskUtils.GetValidTasks();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [permission]);

        await this.DbContext.Tasks.AddRangeAsync(tasks);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        RolePermission rolePermission = new RolePermission { RoleId = role.Id, PermissionId = permission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        PermissionWorkItem permissionWorkItem = new PermissionWorkItem
        {
            PermissionId = permission.Id,
            WorkItemId = tasks.First().Id
        };
        await this.DbContext.PermissionWorkItems.AddAsync(permissionWorkItem);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.GetTasks(QueryUtils.QueryParamUtils.GetValidQueryParams());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TaskResponseDto? result = await this.ReadResponseContent<TaskResponseDto>(response);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
    }

    [Fact(DisplayName = "GIVEN user is admin WHEN Get() is called THEN return all tasks")]
    public async Task GetTasks_LoggedInAdminUser_ReturnsAllTasks()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role role = RoleUtils.GetValidRole();
        role.Name = "Admin";
        List<ZoraTask> tasks = this._taskUtils.GetValidTasks();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [role]);

        await this.DbContext.Tasks.AddRangeAsync(tasks);
        await this.DbContext.SaveChangesAsync();

        UserRole userRole = new UserRole { UserId = user.Id, RoleId = role.Id, Role = role, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRole]);

        HttpResponseMessage response = await this.GetTasks(QueryUtils.QueryParamUtils.GetValidQueryParams());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TaskResponseDto? result = await this.ReadResponseContent<TaskResponseDto>(response);
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(tasks.Count);
    }

    [Fact(DisplayName =
        "GIVEN 7 tasks with different permissions WHEN Get() is called THEN return only tasks with appropriate permissions")]
    public async Task GetTasks_WithSevenTasksAndDifferentPermissions_ReturnsOnlyAccessibleTasks()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role userRole = RoleUtils.GetValidRole();

        Permission nonePermission = PermissionUtils.GetValidPermission(PermissionFlag.None);
        Permission readPermission = PermissionUtils.GetValidPermission(PermissionFlag.Read);
        Permission writePermission = PermissionUtils.GetValidPermission(PermissionFlag.Write);
        Permission createPermission = PermissionUtils.GetValidPermission(PermissionFlag.Create);
        Permission deletePermission = PermissionUtils.GetValidPermission(PermissionFlag.Delete);
        Permission adminPermission = PermissionUtils.GetValidPermission(PermissionFlag.Admin);

        List<ZoraTask> tasks = new List<ZoraTask>();
        for (int i = 1; i <= 7; i++)
        {
            tasks.Add(new ZoraTask
            {
                Id = i,
                Name = $"Task {i}",
                Description = $"Description for Task {i}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [userRole]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext,
            [nonePermission, readPermission, writePermission, createPermission, deletePermission, adminPermission]);
        await this.DbContext.Tasks.AddRangeAsync(tasks);
        await this.DbContext.SaveChangesAsync();

        UserRole userRoleAssociation = new UserRole
            { UserId = user.Id, RoleId = userRole.Id, Role = userRole, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRoleAssociation]);

        List<RolePermission> rolePermissions = new List<RolePermission>
        {
            new() { RoleId = userRole.Id, PermissionId = nonePermission.Id },
            new() { RoleId = userRole.Id, PermissionId = readPermission.Id },
            new() { RoleId = userRole.Id, PermissionId = writePermission.Id },
            new() { RoleId = userRole.Id, PermissionId = createPermission.Id },
            new() { RoleId = userRole.Id, PermissionId = deletePermission.Id },
            new() { RoleId = userRole.Id, PermissionId = adminPermission.Id }
        };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, rolePermissions);

        List<PermissionWorkItem> permissionWorkItems = new List<PermissionWorkItem>
        {
            new() { PermissionId = nonePermission.Id, WorkItemId = 2 },
            new() { PermissionId = readPermission.Id, WorkItemId = 3 },
            new() { PermissionId = writePermission.Id, WorkItemId = 4 },
            new() { PermissionId = createPermission.Id, WorkItemId = 5 },
            new() { PermissionId = deletePermission.Id, WorkItemId = 6 },
            new() { PermissionId = adminPermission.Id, WorkItemId = 7 }
        };
        await this.DbContext.PermissionWorkItems.AddRangeAsync(permissionWorkItems);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.GetTasks(QueryUtils.QueryParamUtils.GetValidQueryParams());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TaskResponseDto? result = await this.ReadResponseContent<TaskResponseDto>(response);
        result.Should().NotBeNull();

        result.Items.Should().HaveCount(5);

        List<long> returnedTaskIds = result.Items.Select(t => t.Id).ToList();
        returnedTaskIds.Should().Contain(new[] { 3L, 4L, 5L, 6L, 7L });
        returnedTaskIds.Should().NotContain(new[] { 1L, 2L });
    }

    [Fact(DisplayName =
        "GIVEN an admin user and 10 tasks with various permissions WHEN Get() is called THEN return all 10 tasks")]
    public async Task GetTasks_AdminUserWithTenTasksWithVariousPermissions_ReturnsAllTasks()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role adminRole = RoleUtils.GetValidRole();
        adminRole.Name = "Admin";

        Permission nonePermission = PermissionUtils.GetValidPermission(PermissionFlag.None);
        Permission readPermission = PermissionUtils.GetValidPermission(PermissionFlag.Read);
        Permission writePermission = PermissionUtils.GetValidPermission(PermissionFlag.Write);
        Permission createPermission = PermissionUtils.GetValidPermission(PermissionFlag.Create);
        Permission deletePermission = PermissionUtils.GetValidPermission(PermissionFlag.Delete);
        Permission adminPermission = PermissionUtils.GetValidPermission(PermissionFlag.Admin);

        List<ZoraTask> tasks = new List<ZoraTask>();
        for (int i = 1; i <= 10; i++)
        {
            tasks.Add(new ZoraTask
            {
                Id = i,
                Name = $"Task {i}",
                Description = $"Description for Task {i}",
                CreatedAt = DateTime.UtcNow
            });
        }

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [adminRole]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext,
            [nonePermission, readPermission, writePermission, createPermission, deletePermission, adminPermission]);
        await this.DbContext.Tasks.AddRangeAsync(tasks);
        await this.DbContext.SaveChangesAsync();

        UserRole userRoleAssociation = new UserRole
            { UserId = user.Id, RoleId = adminRole.Id, Role = adminRole, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRoleAssociation]);

        List<PermissionWorkItem> permissionWorkItems = new List<PermissionWorkItem>
        {
            new() { PermissionId = nonePermission.Id, WorkItemId = 3 },
            new() { PermissionId = nonePermission.Id, WorkItemId = 4 },
            new() { PermissionId = readPermission.Id, WorkItemId = 5 },
            new() { PermissionId = readPermission.Id, WorkItemId = 6 },
            new() { PermissionId = writePermission.Id, WorkItemId = 7 },
            new() { PermissionId = createPermission.Id, WorkItemId = 8 },
            new() { PermissionId = deletePermission.Id, WorkItemId = 9 },
            new() { PermissionId = adminPermission.Id, WorkItemId = 10 }
        };
        await this.DbContext.PermissionWorkItems.AddRangeAsync(permissionWorkItems);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.GetTasks(QueryUtils.QueryParamUtils.GetValidQueryParams());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        TaskResponseDto? result = await this.ReadResponseContent<TaskResponseDto>(response);
        result.Should().NotBeNull();

        result.Items.Should().HaveCount(10);

        List<long> returnedTaskIds = result.Items.Select(t => t.Id).ToList();
        returnedTaskIds.Should().BeEquivalentTo(Enumerable.Range(1, 10).Select(i => (long)i));
    }

    [Fact(DisplayName =
        "GIVEN a user with no permissions for a task WHEN Get() is called THEN return 403")]
    public async Task GetTasks_UserWithNoPermissions_Returns403()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role userRole = RoleUtils.GetValidRole();

        ZoraTask task = this._taskUtils.GetValidTask();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [userRole]);
        await this.DbContext.Tasks.AddAsync(task);
        await this.DbContext.SaveChangesAsync();

        HttpResponseMessage response = await this.GetIndividualTask(task.Id);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName =
        "GIVEN a user with permission NONE for a task WHEN Get() is called THEN return 403")]
    public async Task GetTasks_UserWithNonePermission_Returns403()
    {
        await this.ClearDatabaseAsync();

        User user = UserUtils.GetValidUser();
        Role userRole = RoleUtils.GetValidRole();

        Permission nonePermission = PermissionUtils.GetValidPermission(PermissionFlag.None);

        ZoraTask task = this._taskUtils.GetValidTask();

        await DatabaseSeeder.SeedUsersAsync(this.DbContext, [user]);
        await DatabaseSeeder.SeedRolesAsync(this.DbContext, [userRole]);
        await DatabaseSeeder.SeedPermissionsAsync(this.DbContext, [nonePermission]);
        await this.DbContext.Tasks.AddAsync(task);
        await this.DbContext.SaveChangesAsync();

        UserRole userRoleAssociation = new UserRole
            { UserId = user.Id, RoleId = userRole.Id, Role = userRole, User = user };
        await DatabaseSeeder.SeedUserRolesAsync(this.DbContext, [userRoleAssociation]);

        RolePermission rolePermission = new RolePermission { RoleId = userRole.Id, PermissionId = nonePermission.Id };
        await DatabaseSeeder.SeedRolePermissionsAsync(this.DbContext, [rolePermission]);

        HttpResponseMessage response = await this.GetIndividualTask(task.Id);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName =
        "GIVEN a user with permission DELETE for a task WHEN Get() is called THEN return the task")]
    public async Task GetTasks_UserWithDeletePermission_ReturnsTask()
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

        HttpResponseMessage response = await this.GetIndividualTask(task.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
    }

    [Fact(DisplayName =
        "GIVEN an admin user without permissions for a task WHEN Get() is called THEN return the task")]
    public async Task GetTasks_AdminUserWithoutPermissions_ReturnsTask()
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

        HttpResponseMessage response = await this.GetIndividualTask(task.Id);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
    }

    [Fact(DisplayName =
        "GIVEN a user with MANAGE/ADMIN permissions for a task WHEN Get() is called THEN return the task")]
    public async Task GetTasks_UserWithManageAdminPermission_ReturnsTask()
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

        HttpResponseMessage response = await this.GetIndividualTask(task.Id);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        ReadTaskDto? result = await this.ReadResponseContent<ReadTaskDto>(response);
        result.Should().NotBeNull();
        result.Id.Should().Be(task.Id);
    }

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
}
