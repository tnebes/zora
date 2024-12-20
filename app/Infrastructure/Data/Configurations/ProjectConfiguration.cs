#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("zora_projects");

        builder.Property(p => p.Id)
            .HasColumnName("work_item_id");

        builder.Property(p => p.ProgramId)
            .HasColumnName("program_id")
            .IsRequired(false);

        builder.Property(p => p.ProjectManagerId)
            .HasColumnName("project_manager_id")
            .IsRequired(false);

        builder.Property(p => p.Deleted)
            .HasColumnName("deleted")
            .HasDefaultValue(false);

        builder.HasOne(p => p.Program)
            .WithMany(zp => zp.Projects)
            .HasForeignKey(p => p.ProgramId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.ProjectManager)
            .WithMany(u => u.ManagedProjects)
            .HasForeignKey(p => p.ProjectManagerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
