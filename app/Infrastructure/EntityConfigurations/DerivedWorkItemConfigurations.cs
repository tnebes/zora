#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.EntityConfigurations;

public class ProgramConfiguration : IEntityTypeConfiguration<ZoraProgram>
{
    public void Configure(EntityTypeBuilder<ZoraProgram> builder)
    {
        builder.Property<string>(p => p.Description)
            .HasColumnType("text");
    }
}

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasOne(p => p.Program)
            .WithMany()
            .HasForeignKey(p => p.ProgramId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ProjectManager)
            .WithMany()
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TaskConfiguration : IEntityTypeConfiguration<ZoraTask>
{
    public void Configure(EntityTypeBuilder<ZoraTask> builder)
    {
        builder.HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.ParentTask)
            .WithMany()
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Priority)
            .HasMaxLength(50);
    }
}
