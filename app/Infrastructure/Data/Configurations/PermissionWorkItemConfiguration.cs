#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public class PermissionWorkItemConfiguration : IEntityTypeConfiguration<PermissionWorkItem>
{
    public void Configure(EntityTypeBuilder<PermissionWorkItem> builder)
    {
        builder.ToTable("zora_permission_work_items");

        builder.HasKey(pwi => new { pwi.PermissionId, pwi.WorkItemId });

        builder.Property(pwi => pwi.PermissionId)
            .HasColumnName("permission_id")
            .IsRequired();

        builder.Property(pwi => pwi.WorkItemId)
            .HasColumnName("work_item_id")
            .IsRequired();

        builder.HasOne(pwi => pwi.Permission)
            .WithMany(p => p.PermissionWorkItems)
            .HasForeignKey(pwi => pwi.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pwi => pwi.WorkItem)
            .WithMany(wi => wi.PermissionWorkItems)
            .HasForeignKey(pwi => pwi.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
