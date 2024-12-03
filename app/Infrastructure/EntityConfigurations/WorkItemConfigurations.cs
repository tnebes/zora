#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.EntityConfigurations;

public class WorkItemConfigurations : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.ToTable("zora_work_items");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Type)
            .IsRequired()
            .HasMaxLength(50);

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

        builder.HasDiscriminator(w => w.Type)
            .HasValue<ZoraProgram>("Program")
            .HasValue<Project>("Project")
            .HasValue<ZoraTask>("Task");

        builder.HasMany(w => w.Permissions)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "zora_permission_work_items",
                j => j
                    .HasOne<Permission>()
                    .WithMany()
                    .HasForeignKey("permission_id")
                    .OnDelete(DeleteBehavior.Cascade),
                j => j
                    .HasOne<WorkItem>()
                    .WithMany()
                    .HasForeignKey("work_item_id")
                    .OnDelete(DeleteBehavior.Cascade),
                j =>
                {
                    j.HasKey("permission_id", "work_item_id");
                    j.ToTable("zora_permission_work_items");
                }
            );

        builder.HasMany(w => w.Assets)
            .WithMany()
            .UsingEntity(
                "zora_work_item_assets",
                l => l.HasOne(typeof(Asset)).WithMany().HasForeignKey("asset_id"),
                r => r.HasOne(typeof(WorkItem)).WithMany().HasForeignKey("work_item_id")
            );

        builder.Ignore(w => w.SourceRelationships);
        builder.Ignore(w => w.TargetRelationships);
    }
}
