#region

using Bogus;
using zora.Core.Domain;
using zora.Core.Interfaces.Services;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.DataSeed;

public sealed class DataSeeder : IZoraService, IDataSeeder
{
    private const int NUM_USERS = 1000;
    private const int NUM_PROGRAMS = 10;
    private const int NUM_PROJECTS_PER_PROGRAM = 10;
    private const int NUM_TASKS_PER_PROJECT = 20;
    private const int NUM_ROLES = 150;
    private const int NUM_PERMISSIONS = 150;
    private const int NUM_ASSETS = 50;
    private const int MIN_PERMISSIONS_PER_ROLE = 2;
    private const int MAX_PERMISSIONS_PER_ROLE = 10;
    private const int MIN_ROLES_PER_USER = 1;
    private const int MAX_ROLES_PER_USER = 5;
    private readonly Faker<Asset> _assetFaker;

    private readonly ApplicationDbContext _context;
    private readonly Faker _faker;
    private readonly ILogger<DataSeeder> _logger;
    private readonly Faker<Permission> _permissionFaker;
    private readonly Faker<ZoraProgram> _programFaker;
    private readonly Faker<Project> _projectFaker;
    private readonly Faker<Role> _roleFaker;
    private readonly Faker<ZoraTask> _taskFaker;
    private readonly Faker<User> _userFaker;

    public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        this._context = context;
        this._logger = logger;
        this._faker = new Faker();

        this._userFaker = new Faker<User>()
            .RuleFor(u => u.Username, f => $"{f.Commerce.Color()}{f.Internet.UserName()}")
            .RuleFor(u => u.Email, f => f.Internet.Email(uniqueSuffix: f.Random.Guid().ToString()))
            .RuleFor(u => u.Password, _ => BCrypt.Net.BCrypt.HashPassword("Password123!"));

        this._programFaker = new Faker<ZoraProgram>()
            .RuleFor(w => w.Name, f => f.Commerce.Department())
            .RuleFor(w => w.Description, f => f.Lorem.Paragraph())
            .RuleFor(w => w.Status, f => f.PickRandom("New", "In Progress", "Completed"))
            .RuleFor(w => w.StartDate, f => f.Date.Future())
            .RuleFor(w => w.DueDate, (f, w) => f.Date.Future(1, w.StartDate))
            .RuleFor(w => w.CompletionPercentage, f => f.Random.Decimal(0, 100))
            .RuleFor(w => w.EstimatedHours, f => f.Random.Decimal(1, 100))
            .RuleFor(w => w.ActualHours, f => f.Random.Decimal(1, 100))
            .RuleFor(w => w.Type, _ => "Program");

        this._projectFaker = new Faker<Project>()
            .RuleFor(w => w.Name, f => f.Commerce.ProductName())
            .RuleFor(w => w.Description, f => f.Lorem.Paragraph())
            .RuleFor(w => w.Status, f => f.PickRandom("New", "In Progress", "Completed"))
            .RuleFor(w => w.StartDate, f => f.Date.Future())
            .RuleFor(w => w.DueDate, (f, w) => f.Date.Future(1, w.StartDate))
            .RuleFor(w => w.CompletionPercentage, f => f.Random.Decimal(0, 100))
            .RuleFor(w => w.EstimatedHours, f => f.Random.Decimal(1, 100))
            .RuleFor(w => w.ActualHours, f => f.Random.Decimal(1, 100))
            .RuleFor(w => w.Type, _ => "Project");

        this._taskFaker = new Faker<ZoraTask>()
            .RuleFor(w => w.Name, f => f.Commerce.Product())
            .RuleFor(w => w.Description, f => f.Lorem.Paragraph())
            .RuleFor(w => w.Status, f => f.PickRandom("New", "In Progress", "Completed"))
            .RuleFor(w => w.StartDate, f => f.Date.Future())
            .RuleFor(w => w.DueDate, (f, w) => f.Date.Future(1, w.StartDate))
            .RuleFor(w => w.CompletionPercentage, f => f.Random.Decimal(0, 100))
            .RuleFor(w => w.EstimatedHours, f => f.Random.Decimal(1, 100))
            .RuleFor(w => w.ActualHours, f => f.Random.Decimal(1, 100))
            .RuleFor(w => w.Type, _ => "Task")
            .RuleFor(w => w.Priority, f => f.PickRandom("Low", "Medium", "High"));

        this._roleFaker = new Faker<Role>()
            .RuleFor(r => r.Name, f => $"{f.Commerce.Department()} {f.Lorem.Word()} {f.Lorem.Word()}");

        this._permissionFaker = new Faker<Permission>()
            .RuleFor(p => p.Name, f => $"{f.Lorem.Word()} {f.Lorem.Word()} {f.Lorem.Word()}")
            .RuleFor(p => p.PermissionString, _ => GeneratePermissionString());

        this._assetFaker = new Faker<Asset>()
            .RuleFor(a => a.Name, f => f.Commerce.ProductName())
            .RuleFor(a => a.Description, f => f.Lorem.Sentence())
            .RuleFor(a => a.AssetPath, f => f.System.DirectoryPath())
            .RuleFor(a => a.CreatedAt, f => f.Date.Past(2))
            .RuleFor(a => a.UpdatedAt, f => f.Date.Recent(90));
    }

    public async Task SeedAsync()
    {
        try
        {
            this._logger.LogInformation("Starting database seeding");

            List<User> users = await this.SeedUsers();
            List<ZoraProgram> programs = await this.SeedPrograms(users);
            await this.SeedProjectsAndTasks(programs, users);
            List<Role> roles = await this.SeedRoles();
            List<Permission> permissions = await this.SeedPermissions();
            await this.SeedRolePermissions(roles, permissions);
            await this.SeedUserRoles(users, roles);
            await this.SeedAssets(users);

            this._logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            this._logger.LogCritical(ex, "Error occurred during database seeding");
            throw;
        }
    }

    private async Task<List<User>> SeedUsers()
    {
        this._logger.LogInformation("Seeding {Count} users", NUM_USERS);
        List<User>? users = this._userFaker.Generate(NUM_USERS);
        await this._context.Users.AddRangeAsync(users);
        await this._context.SaveChangesAsync();
        return users;
    }

    private async Task<List<ZoraProgram>> SeedPrograms(List<User> users)
    {
        List<ZoraProgram>? programs = this._programFaker
            .RuleFor(w => w.AssigneeId, f => f.PickRandom(users).Id)
            .RuleFor(w => w.CreatedById, f => f.PickRandom(users).Id)
            .Generate(NUM_PROGRAMS);

        await this._context.Programs.AddRangeAsync(programs);
        this._logger.LogInformation("Seeding {Count} programs", NUM_PROGRAMS);
        await this._context.SaveChangesAsync();
        return programs;
    }

    private async Task SeedProjectsAndTasks(List<ZoraProgram> programs, List<User> users)
    {
        foreach (ZoraProgram program in programs)
        {
            List<Project>? projects = this._projectFaker
                .RuleFor(w => w.AssigneeId, f => f.PickRandom(users).Id)
                .RuleFor(w => w.CreatedById, f => f.PickRandom(users).Id)
                .RuleFor(p => p.ProgramId, _ => program.Id)
                .RuleFor(p => p.ProjectManagerId, f => f.PickRandom(users).Id)
                .Generate(NUM_PROJECTS_PER_PROGRAM);

            this._logger.LogInformation("Seeding {Count} projects for program {ProgramId}",
                NUM_PROJECTS_PER_PROGRAM, program.Id);
            await this._context.Projects.AddRangeAsync(projects);
            await this._context.SaveChangesAsync();

            foreach (Project project in projects)
            {
                List<ZoraTask>? tasks = this._taskFaker
                    .RuleFor(w => w.AssigneeId, f => f.PickRandom(users).Id)
                    .RuleFor(w => w.CreatedById, f => f.PickRandom(users).Id)
                    .RuleFor(t => t.ProjectId, _ => project.Id)
                    .Generate(NUM_TASKS_PER_PROJECT);

                this._logger.LogInformation("Seeding {Count} tasks for project {ProjectId}",
                    NUM_TASKS_PER_PROJECT, project.Id);
                await this._context.Tasks.AddRangeAsync(tasks);
                await this._context.SaveChangesAsync();
            }
        }
    }

    private async Task<List<Role>> SeedRoles()
    {
        List<Role> roles = this._roleFaker.Generate(NUM_ROLES);
        this._logger.LogInformation("Seeding {Count} roles", NUM_ROLES);
        await this._context.Roles.AddRangeAsync(roles);
        await this._context.SaveChangesAsync();
        return roles;
    }

    private async Task<List<Permission>> SeedPermissions()
    {
        List<Permission> permissions = this._permissionFaker.Generate(NUM_PERMISSIONS);
        this._logger.LogInformation("Seeding {Count} permissions", NUM_PERMISSIONS);
        await this._context.Permissions.AddRangeAsync(permissions);
        await this._context.SaveChangesAsync();
        return permissions;
    }

    private async Task SeedRolePermissions(List<Role> roles, List<Permission> permissions)
    {
        this._logger.LogInformation("Seeding role permissions");
        Faker faker = new Faker();
        foreach (Role role in roles)
        {
            int numPermissions =
                faker.Random.Int(MIN_PERMISSIONS_PER_ROLE, MAX_PERMISSIONS_PER_ROLE);
            IEnumerable<RolePermission> rolePermissions = faker.PickRandom(permissions, numPermissions)
                .Select(p => new RolePermission { RoleId = role.Id, PermissionId = p.Id });

            await this._context.RolePermissions.AddRangeAsync(rolePermissions);
        }

        await this._context.SaveChangesAsync();
    }

    private async Task SeedUserRoles(List<User> users, List<Role> roles)
    {
        this._logger.LogInformation("Seeding user roles");
        Faker faker = new Faker();
        foreach (User user in users)
        {
            int numRoles = faker.Random.Int(MIN_ROLES_PER_USER, MAX_ROLES_PER_USER);
            IEnumerable<UserRole> userRoles = faker.PickRandom(roles, numRoles)
                .Select(r => new UserRole { UserId = user.Id, RoleId = r.Id });

            await this._context.UserRoles.AddRangeAsync(userRoles);
        }

        await this._context.SaveChangesAsync();
    }

    private async Task SeedAssets(List<User> users)
    {
        List<Asset> assets = this._assetFaker
            .RuleFor(a => a.CreatedById, f => f.PickRandom(users).Id)
            .RuleFor(a => a.UpdatedById, f => f.PickRandom(users).Id)
            .Generate(NUM_ASSETS);

        this._logger.LogInformation("Seeding {Count} assets", NUM_ASSETS);
        await this._context.Assets.AddRangeAsync(assets);
        await this._context.SaveChangesAsync();
    }

    private static string GeneratePermissionString()
    {
        return new string(Enumerable.Range(0, 5)
            .Select(_ => new Faker().Random.Bool() ? '1' : '0')
            .ToArray());
    }
}
