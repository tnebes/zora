#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.EntityConfigurations;

public class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(w => w.Description)
            .HasColumnType("text");

        builder.Property(w => w.Status)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.CompletionPercentage)
            .HasColumnType("decimal(5,2)");

        builder.Property(w => w.EstimatedHours)
            .HasColumnType("decimal(10,2)");

        builder.Property(w => w.ActualHours)
            .HasColumnType("decimal(10,2)");

        builder.HasOne(w => w.Assignee)
            .WithMany()
            .HasForeignKey(w => w.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.CreatedBy)
            .WithMany()
            .HasForeignKey(w => w.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(w => w.UpdatedBy)
            .WithMany()
            .HasForeignKey(w => w.UpdatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(w => w.Permissions)
            .WithMany()
            .UsingEntity(
                "zora_permission_work_items",
                l => l.HasOne(typeof(Permission)).WithMany().HasForeignKey("permission_id"),
                r => r.HasOne(typeof(WorkItem)).WithMany().HasForeignKey("work_item_id")
            );

        builder.HasMany(w => w.Assets)
            .WithMany()
            .UsingEntity(
                "zora_work_item_assets",
                l => l.HasOne(typeof(Asset)).WithMany().HasForeignKey("asset_id"),
                r => r.HasOne(typeof(WorkItem)).WithMany().HasForeignKey("work_item_id")
            );

        builder.HasMany(w => w.SourceRelationships)
            .WithOne(r => r.SourceItem)
            .HasForeignKey(r => r.SourceItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.TargetRelationships)
            .WithOne(r => r.TargetItem)
            .HasForeignKey(r => r.TargetItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
