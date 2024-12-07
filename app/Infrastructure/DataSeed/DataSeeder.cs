#region

using Bogus;
using zora.Core.Domain;
using zora.Core.Interfaces;
using zora.Infrastructure.Data;

#endregion

namespace zora.Infrastructure.DataSeed;

public class DataSeeder : IZoraService, IDataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataSeeder> _logger;
    private readonly Faker<ZoraProgram> _programFaker;
    private readonly Faker<Project> _projectFaker;
    private readonly Faker<ZoraTask> _taskFaker;

    private readonly Faker<User> _userFaker;
    private readonly Faker<WorkItem> _workItemFaker;

    public DataSeeder(ApplicationDbContext context, ILogger<DataSeeder> logger)
    {
        this._context = context;
        this._logger = logger;

        this._userFaker = new Faker<User>()
            .RuleFor(u => u.Username, f => f.Internet.UserName())
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Password, f => BCrypt.Net.BCrypt.HashPassword("Password123!"));

        this._workItemFaker = new Faker<WorkItem>()
            .RuleFor(w => w.Name, f => f.Commerce.ProductName())
            .RuleFor(w => w.Description, f => f.Lorem.Paragraph())
            .RuleFor(w => w.Status, f => f.PickRandom("New", "InProgress", "Completed"))
            .RuleFor(w => w.StartDate, f => f.Date.Future())
            .RuleFor(w => w.DueDate, (f, w) => f.Date.Future(1, w.StartDate))
            .RuleFor(w => w.CompletionPercentage, f => f.Random.Decimal(0, 100))
            .RuleFor(w => w.EstimatedHours, f => f.Random.Decimal(1, 100))
            .RuleFor(w => w.ActualHours, f => f.Random.Decimal(1, 100));

        this._programFaker = new Faker<ZoraProgram>();
        this._projectFaker = new Faker<Project>();
        this._taskFaker = new Faker<ZoraTask>()
            .RuleFor(t => t.Priority, f => f.PickRandom("Low", "Medium", "High"));
    }

    public async Task SeedAsync()
    {
        this._logger.LogInformation("Starting database seeding");

        List<User> users = this._userFaker.Generate(1000);
        await this._context.Users.AddRangeAsync(users);
        this._logger.LogInformation("Seeding users");
        await this._context.SaveChangesAsync();

        List<WorkItem> programWorkItems = this._workItemFaker
            .RuleFor(w => w.Type, f => "Program")
            .RuleFor(w => w.AssigneeId, f => f.PickRandom(users).Id)
            .RuleFor(w => w.CreatedById, f => f.PickRandom(users).Id)
            .Generate(10);

        List<ZoraProgram> programs = programWorkItems.Select(w => new ZoraProgram()
        {
            WorkItem = w,
            Description = new Faker().Lorem.Paragraph()
        }).ToList();

        await this._context.WorkItems.AddRangeAsync(programWorkItems);
        await this._context.Programs.AddRangeAsync(programs);
        this._logger.LogInformation("Seeding programs");
        await this._context.SaveChangesAsync();

        foreach (ZoraProgram program in programs)
        {
            List<WorkItem> projectWorkItems = this._workItemFaker
                .RuleFor(w => w.Type, f => "Project")
                .RuleFor(w => w.AssigneeId, f => f.PickRandom(users).Id)
                .RuleFor(w => w.CreatedById, f => f.PickRandom(users).Id)
                .Generate(10);

            List<Project> projects = projectWorkItems.Select(w => new Project
            {
                WorkItem = w,
                ProgramId = program.WorkItemId,
                ProjectManagerId = new Faker().PickRandom(users).Id
            }).ToList();

            await this._context.WorkItems.AddRangeAsync(projectWorkItems);
            await this._context.Projects.AddRangeAsync(projects);
            this._logger.LogInformation("Seeding projects");
            await this._context.SaveChangesAsync();

            foreach (Project project in projects)
            {
                List<WorkItem> taskWorkItems = this._workItemFaker
                    .RuleFor(w => w.Type, f => "Task")
                    .RuleFor(w => w.AssigneeId, f => f.PickRandom(users).Id)
                    .RuleFor(w => w.CreatedById, f => f.PickRandom(users).Id)
                    .Generate(20);

                List<ZoraTask> tasks = taskWorkItems.Select(w => new ZoraTask()
                {
                    WorkItem = w,
                    ProjectId = project.WorkItemId,
                    Priority = new Faker().PickRandom("Low", "Medium", "High")
                }).ToList();

                await this._context.WorkItems.AddRangeAsync(taskWorkItems);
                await this._context.Tasks.AddRangeAsync(tasks);
            }

            this._logger.LogInformation("Seeding tasks");
            await this._context.SaveChangesAsync();
        }

        this._logger.LogInformation("Database seeding completed");
    }
}
