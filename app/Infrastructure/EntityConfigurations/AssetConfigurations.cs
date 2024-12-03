#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.EntityConfigurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Description)
            .HasColumnType("text");

        builder.Property(a => a.AssetPath)
            .IsRequired()
            .HasColumnType("text");

        builder.HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(a => a.UpdatedBy)
            .WithMany()
            .HasForeignKey(a => a.UpdatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(a => a.WorkItems)
            .WithMany(w => w.Assets)
            .UsingEntity(
                "zora_work_item_assets",
                l => l.HasOne(typeof(WorkItem)).WithMany().HasForeignKey("work_item_id"),
                r => r.HasOne(typeof(Asset)).WithMany().HasForeignKey("asset_id"),
                j => j.HasKey("work_item_id", "asset_id")
            );
    }
}
