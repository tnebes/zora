#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public sealed class WorkItemAssetConfiguration : IEntityTypeConfiguration<WorkItemAsset>
{
    public void Configure(EntityTypeBuilder<WorkItemAsset> builder)
    {
        builder.ToTable("zora_work_item_assets");

        builder.HasKey(wia => new { wia.WorkItemId, wia.AssetId });

        builder.Property(wia => wia.WorkItemId)
            .HasColumnName("work_item_id")
            .IsRequired();

        builder.Property(wia => wia.AssetId)
            .HasColumnName("asset_id")
            .IsRequired();

        builder.HasOne(wia => wia.WorkItem)
            .WithMany(wi => wi.WorkItemAssets)
            .HasForeignKey(wia => wia.WorkItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(wia => wia.Asset)
            .WithMany(a => a.WorkItemAssets)
            .HasForeignKey(wia => wia.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(wia => wia.AssetId)
            .HasDatabaseName("IX_WorkItemAsset_AssetIds");

        builder.HasIndex(wia => wia.WorkItemId)
            .HasDatabaseName("IX_WorkItemAsset_WorkItemIds");
    }
}
