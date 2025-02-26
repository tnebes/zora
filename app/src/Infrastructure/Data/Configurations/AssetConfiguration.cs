#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public sealed class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode();

        builder.Property(a => a.Description)
            .HasColumnName("description")
            .IsUnicode();

        builder.Property(a => a.AssetPath)
            .HasColumnName("asset_path")
            .IsRequired()
            .IsUnicode();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(a => a.CreatedById)
            .HasColumnName("created_by");

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(a => a.UpdatedById)
            .HasColumnName("updated_by");

        builder.Property(a => a.Deleted)
            .HasColumnName("deleted")
            .HasDefaultValue(false);

        builder.HasOne(a => a.CreatedBy)
            .WithMany()
            .HasForeignKey(a => a.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.UpdatedBy)
            .WithMany()
            .HasForeignKey(a => a.UpdatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.WorkItemAssets)
            .WithOne(wia => wia.Asset)
            .HasForeignKey(wia => wia.AssetId);
    }
}
