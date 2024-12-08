#region

using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using zora.Core;
using zora.Core.Attributes;
using zora.Core.Domain;
using zora.Core.Interfaces;
using zora.Infrastructure.Data.Configurations;
using zora.Services.Configuration;

#endregion

namespace zora.Infrastructure.Data;

[ServiceLifetime(ServiceLifetime.Scoped)]
public class ApplicationDbContext : DbContext, IDbContext, IZoraService
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

        modelBuilder.Entity<WorkItem>().UseTptMappingStrategy();

        ApplicationDbContext.ConfigureTableNames(modelBuilder);

        modelBuilder.Entity<PermissionWorkItem>()
            .HasKey(p => new { p.PermissionId, p.WorkItemId });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        this.AddEntityConfigurations(modelBuilder);

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

    private void AddEntityConfigurations(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserRoleConfiguration());
        modelBuilder.ApplyConfiguration(new RolePermissionConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionConfiguration());
        modelBuilder.ApplyConfiguration(new WorkItemAssetConfiguration());
        modelBuilder.ApplyConfiguration(new AssetConfiguration());
        modelBuilder.ApplyConfiguration(new WorkItemConfiguration());
        modelBuilder.ApplyConfiguration(new PermissionWorkItemConfiguration());
        modelBuilder.ApplyConfiguration(new ProjectConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new WorkItemRelationshipConfiguration());
        modelBuilder.ApplyConfiguration(new ZoraProgramConfiguration());
        modelBuilder.ApplyConfiguration(new ZoraTaskConfiguration());
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .HasDatabaseName("IX_User_Username");

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .HasDatabaseName("IX_User_Email");

        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .HasDatabaseName("IX_Role_Name");

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.Name)
            .HasDatabaseName("IX_Permission_Name");

        modelBuilder.Entity<Permission>()
            .HasIndex(p => p.PermissionString)
            .HasDatabaseName("IX_Permission_String");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.Type)
            .HasDatabaseName("IX_WorkItem_Types");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.Status)
            .HasDatabaseName("IX_WorkItem_Statuses");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.AssigneeId)
            .HasDatabaseName("IX_WorkItem_AssigneeIds");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.Name)
            .HasDatabaseName("IX_WorkItem_Name");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.CreatedById)
            .HasDatabaseName("IX_WorkItem_CreatedBy");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.UpdatedById)
            .HasDatabaseName("IX_WorkItem_UpdatedBy");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.DueDate)
            .HasDatabaseName("IX_WorkItem_DueDates");

        modelBuilder.Entity<WorkItem>()
            .HasIndex(w => w.StartDate)
            .HasDatabaseName("IX_WorkItem_StartDates");

        modelBuilder.Entity<Project>()
            .HasIndex(p => p.ProjectManagerId)
            .HasDatabaseName("IX_Project_ProjectManager");

        modelBuilder.Entity<Project>()
            .HasIndex(p => p.ProgramId)
            .HasDatabaseName("IX_Project_Program");

        modelBuilder.Entity<ZoraTask>()
            .HasIndex(t => t.ProjectId)
            .HasDatabaseName("IX_Task_Project");

        modelBuilder.Entity<ZoraTask>()
            .HasIndex(t => t.Priority)
            .HasDatabaseName("IX_Task_Priorities");

        modelBuilder.Entity<ZoraTask>()
            .HasIndex(t => t.ParentTaskId)
            .HasDatabaseName("IX_Task_ParentTask");

        modelBuilder.Entity<WorkItemRelationship>()
            .HasIndex(w => w.RelationshipType)
            .HasDatabaseName("IX_WorkItemRelationship_RelationshipTypes");

        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.Name)
            .HasDatabaseName("IX_Asset_Name");

        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.CreatedById)
            .HasDatabaseName("IX_Asset_CreatedBy");

        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.UpdatedById)
            .HasDatabaseName("IX_Asset_UpdatedBy");

        modelBuilder.Entity<WorkItemAsset>()
            .HasIndex(w => w.AssetId)
            .HasDatabaseName("IX_WorkItemAsset_AssetIds");

        modelBuilder.Entity<WorkItemAsset>()
            .HasIndex(w => w.WorkItemId)
            .HasDatabaseName("IX_WorkItemAsset_WorkItemIds");
    }
}
