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
            await connection.OpenAsync();
        }

        return connection;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string? connectionString = this._secretsManagerService.GetSecret(Constants.ConnectionStringKey);

            if (string.IsNullOrEmpty(connectionString))
            {
                this._logger.LogError("Database connection string {KeyName} not found in secrets.",
                    Constants.ConnectionStringKey);
                throw new InvalidOperationException(
                    $"Database connection string {Constants.ConnectionStringKey} not found in secrets.");
            }

            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToTable("zora_users");
        modelBuilder.Entity<Role>().ToTable("zora_roles");
        modelBuilder.Entity<Permission>().ToTable("zora_permissions");
        modelBuilder.Entity<WorkItem>().ToTable("zora_work_items");
        modelBuilder.Entity<ZoraProgram>().ToTable("zora_programs");
        modelBuilder.Entity<Project>().ToTable("zora_projects");
        modelBuilder.Entity<ZoraTask>().ToTable("zora_tasks");
        modelBuilder.Entity<Asset>().ToTable("assets");
        modelBuilder.Entity<WorkItemRelationship>().ToTable("zora_work_item_relationships");

        modelBuilder.Entity<WorkItem>()
            .HasDiscriminator(w => w.Type)
            .HasValue<ZoraProgram>("Program")
            .HasValue<Project>("Project")
            .HasValue<ZoraTask>("Task");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
