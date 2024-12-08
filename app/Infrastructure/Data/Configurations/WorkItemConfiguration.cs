#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public class WorkItemConfiguration : IEntityTypeConfiguration<WorkItem>
{
    public void Configure(EntityTypeBuilder<WorkItem> builder)
    {
        builder.ToTable("zora_work_items");

        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id)
            .HasColumnName("id")
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
            .IsUnicode();

        builder.Property(w => w.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(50)
            .IsUnicode();

        builder.Property(w => w.StartDate)
            .HasColumnName("start_date");

        builder.Property(w => w.DueDate)
            .HasColumnName("due_date");

        builder.Property(w => w.CompletionPercentage)
            .HasColumnName("completion_percentage")
            .HasPrecision(5, 2);

        builder.Property(w => w.EstimatedHours)
            .HasColumnName("estimated_hours")
            .HasPrecision(10, 2);

        builder.Property(w => w.ActualHours)
            .HasColumnName("actual_hours")
            .HasPrecision(10, 2);

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(w => w.CreatedById)
            .HasColumnName("created_by");

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(w => w.UpdatedById)
            .HasColumnName("updated_by");

        builder.Property(w => w.AssigneeId)
            .HasColumnName("assignee_id");

        builder.HasOne(w => w.Assignee)
            .WithMany(u => u.AssignedWorkItems)
            .HasForeignKey(w => w.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.CreatedBy)
            .WithMany(u => u.CreatedWorkItems)
            .HasForeignKey(w => w.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.UpdatedBy)
            .WithMany(u => u.UpdatedWorkItems)
            .HasForeignKey(w => w.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Program)
            .WithOne()
            .HasForeignKey<ZoraProgram>(p => p.WorkItemId);

        builder.HasOne(w => w.Project)
            .WithOne()
            .HasForeignKey<Project>(p => p.WorkItemId);

        builder.HasOne(w => w.Task)
            .WithOne()
            .HasForeignKey<ZoraTask>(t => t.WorkItemId);

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
            .HasForeignKey(wia => wia.WorkItemId);

        builder.HasMany(w => w.PermissionWorkItems)
            .WithOne(pw => pw.WorkItem)
            .HasForeignKey(pw => pw.WorkItemId);
    }
}
