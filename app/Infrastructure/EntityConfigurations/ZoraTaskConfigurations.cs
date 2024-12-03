#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.EntityConfigurations;

public class ZoraTaskConfigurations : IEntityTypeConfiguration<ZoraTask>
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

        builder.Ignore(t => t.SourceRelationships);
        builder.Ignore(t => t.TargetRelationships);
    }
}
