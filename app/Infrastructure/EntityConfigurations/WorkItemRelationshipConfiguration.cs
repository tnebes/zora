#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.EntityConfigurations;

public class WorkItemRelationshipConfiguration : IEntityTypeConfiguration<WorkItemRelationship>
{
    public void Configure(EntityTypeBuilder<WorkItemRelationship> builder)
    {
        builder.ToTable("zora_work_item_relationships");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.RelationshipType)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne<WorkItem>()
            .WithMany()
            .HasForeignKey(r => r.SourceItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_WorkItemRelationship_SourceWorkItem");

        builder.HasOne<WorkItem>()
            .WithMany()
            .HasForeignKey(r => r.TargetItemId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_WorkItemRelationship_TargetWorkItem");

        builder.Ignore(r => r.SourceItem);
        builder.Ignore(r => r.TargetItem);
    }
}
