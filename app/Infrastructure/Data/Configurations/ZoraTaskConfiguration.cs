#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public class ZoraTaskConfiguration : IEntityTypeConfiguration<ZoraTask>
{
    public void Configure(EntityTypeBuilder<ZoraTask> builder)
    {
        builder.ToTable("zora_tasks");

        builder.Property(t => t.Id)
            .HasColumnName("work_item_id");

        builder.Property(zt => zt.ProjectId)
            .HasColumnName("project_id")
            .IsRequired(false);

        builder.Property(zt => zt.Priority)
            .HasColumnName("priority")
            .HasMaxLength(50)
            .IsRequired(false)
            .IsUnicode();

        builder.Property(zt => zt.ParentTaskId)
            .HasColumnName("parent_task_id")
            .IsRequired(false);

        builder.HasOne(zt => zt.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(zt => zt.ProjectId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(zt => zt.ParentTask)
            .WithMany(zt => zt.SubTasks)
            .HasForeignKey(zt => zt.ParentTaskId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(zt => zt.Priority)
            .HasDatabaseName("IX_Task_Priorities");
    }
}
