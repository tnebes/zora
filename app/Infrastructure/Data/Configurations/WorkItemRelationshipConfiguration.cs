#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public class WorkItemRelationshipConfiguration : IEntityTypeConfiguration<WorkItemRelationship>
{
    public void Configure(EntityTypeBuilder<WorkItemRelationship> builder)
    {
        builder.ToTable("zora_work_item_relationships");

        builder.HasKey(wir => wir.Id);

        builder.Property(wir => wir.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(wir => wir.SourceItemId)
            .HasColumnName("source_item_id")
            .IsRequired();

        builder.Property(wir => wir.TargetItemId)
            .HasColumnName("target_item_id")
            .IsRequired();

        builder.Property(wir => wir.RelationshipType)
            .HasColumnName("relationship_type")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode();

        builder.Property(wir => wir.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(wir => wir.Deleted)
            .HasColumnName("deleted")
            .HasDefaultValue(false);

        builder.HasOne(wir => wir.SourceItem)
            .WithMany(wi => wi.SourceRelationships)
            .HasForeignKey(wir => wir.SourceItemId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasOne(wir => wir.TargetItem)
            .WithMany(wi => wi.TargetRelationships)
            .HasForeignKey(wir => wir.TargetItemId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        builder.HasIndex(wir => wir.RelationshipType)
            .HasDatabaseName("IX_WorkItemRelationship_RelationshipTypes");
    }
}
