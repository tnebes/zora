#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("zora_permissions");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .IsUnicode();

        builder.Property(p => p.PermissionString)
            .HasColumnName("permission_string")
            .IsRequired()
            .HasMaxLength(5)
            .IsFixedLength()
            .IsUnicode(false);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(p => p.Deleted)
            .HasColumnName("deleted")
            .HasDefaultValue(false);

        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.PermissionWorkItems)
            .WithOne(pwi => pwi.Permission)
            .HasForeignKey(pwi => pwi.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasCheckConstraint(
            "CHK_Permission_String",
            "permission_string LIKE '[0-1][0-1][0-1][0-1][0-1]'"
        );
    }
}
