#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public sealed class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.UseTptMappingStrategy();

        builder.ToTable("zora_work_items");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasColumnName("work_item_id")
            .UseIdentityColumn();

        builder.Property(w => w.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode();

        builder.Property(w => w.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode();

        builder.Property(w => w.Description)
            .HasColumnName("description")
            .IsRequired(false)
            .IsUnicode();

        builder.Property(w => w.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode();

        builder.Property(w => w.StartDate)
            .HasColumnName("start_date")
            .IsRequired(false);

        builder.Property(w => w.DueDate)
            .HasColumnName("due_date")
            .IsRequired(false);

        builder.Property(w => w.CompletionPercentage)
            .HasColumnName("completion_percentage")
            .HasPrecision(5, 2)
            .IsRequired(false);

        builder.Property(w => w.EstimatedHours)
            .HasColumnName("estimated_hours")
            .HasPrecision(10, 2)
            .IsRequired(false);

        builder.Property(w => w.ActualHours)
            .HasColumnName("actual_hours")
            .HasPrecision(10, 2)
            .IsRequired(false);

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(w => w.CreatedById)
            .HasColumnName("created_by")
            .IsRequired(false);

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired(false);

        builder.Property(w => w.UpdatedById)
            .HasColumnName("updated_by")
            .IsRequired(false);

        builder.Property(w => w.AssigneeId)
            .HasColumnName("assignee_id")
            .IsRequired(false);

        builder.Property(w => w.Deleted)
            .HasColumnName("deleted")
            .HasDefaultValue(false);

        builder.HasOne(w => w.Assignee)
            .WithMany(u => u.AssignedWorkItems)
            .HasForeignKey(w => w.AssigneeId)
            .HasConstraintName("FK_WorkItem_User_Assignee")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.CreatedBy)
            .WithMany(u => u.CreatedWorkItems)
            .HasForeignKey(w => w.CreatedById)
            .HasConstraintName("FK_WorkItem_User_CreatedBy")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.UpdatedBy)
            .WithMany(u => u.UpdatedWorkItems)
            .HasForeignKey(w => w.UpdatedById)
            .HasConstraintName("FK_WorkItem_User_UpdatedBy")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.SourceRelationships)
            .WithOne(r => r.SourceItem)
            .HasForeignKey(r => r.SourceItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.TargetRelationships)
            .WithOne(r => r.TargetItem)
            .HasForeignKey(r => r.TargetItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.WorkItemAssets)
            .WithOne(wia => wia.WorkItem)
            .HasForeignKey(wia => wia.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(w => w.PermissionWorkItems)
            .WithOne(pw => pw.WorkItem)
            .HasForeignKey(pw => pw.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(w => w.Type)
            .HasDatabaseName("IX_WorkItem_Types");

        builder.HasIndex(w => w.Status)
            .HasDatabaseName("IX_WorkItem_Statuses");

        builder.HasIndex(w => w.AssigneeId)
            .HasDatabaseName("IX_WorkItem_AssigneeIds");
    }
}
