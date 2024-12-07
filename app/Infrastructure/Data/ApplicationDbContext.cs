#region

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Interfaces;
using zora.Services.Configuration;

#endregion

namespace zora.Infrastructure.Data;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class ApplicationDbContext : DbContext, IDbContext
{
    private readonly ILogger<ApplicationDbContext> _logger;
    private readonly ISecretsManagerService _secretsManagerService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ISecretsManagerService secretsManagerService,
        ILogger<ApplicationDbContext> logger)
        : base(options)
    {
        this._secretsManagerService = secretsManagerService;
        this._logger = logger;
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<WorkItem> WorkItems { get; set; } = null!;
    public DbSet<ZoraProgram> Programs { get; set; } = null!;
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<ZoraTask> Tasks { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<WorkItemRelationship> WorkItemRelationships { get; set; } = null!;

    public async Task<SqlConnection> CreateConnectionAsync()
    {
        SqlConnection connection = (SqlConnection)this.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            try
            {
                await connection.OpenAsync();
            }
            catch (SqlException ex)
            {
                this._logger.LogError(ex, "Failed to open database connection");
                throw;
            }
        }

        return connection;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string? connectionString = this._secretsManagerService.GetSecret(Constants.CONNECTION_STRING_KEY);

            if (string.IsNullOrEmpty(connectionString))
            {
                const string errorMessage = "Database connection string {KeyName} not found in secrets.";
                this._logger.LogError(errorMessage, Constants.CONNECTION_STRING_KEY);
                throw new InvalidOperationException(string.Format(errorMessage, Constants.CONNECTION_STRING_KEY));
            }

            optionsBuilder
                .UseSqlServer(connectionString)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        ApplicationDbContext.ConfigureTableNames(modelBuilder);
        ApplicationDbContext.ConfigureIndexes(modelBuilder);
    }

    private static void ConfigureTableNames(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("zora_users");
        modelBuilder.Entity<Role>().ToTable("zora_roles");
        modelBuilder.Entity<Permission>().ToTable("zora_permissions");
        modelBuilder.Entity<WorkItem>().ToTable("zora_work_items");
        modelBuilder.Entity<ZoraProgram>().ToTable("zora_programs");
        modelBuilder.Entity<Project>().ToTable("zora_projects");
        modelBuilder.Entity<ZoraTask>().ToTable("zora_tasks");
        modelBuilder.Entity<Asset>().ToTable("assets");
        modelBuilder.Entity<WorkItemRelationship>().ToTable("zora_work_item_relationships");
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.Type)
            .HasDatabaseName("IX_WorkItem_Types");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.Status)
            .HasDatabaseName("IX_WorkItem_Statuses");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.AssigneeId)
            .HasDatabaseName("IX_WorkItem_AssigneeIds");

        // TODO add other indices
    }
}
