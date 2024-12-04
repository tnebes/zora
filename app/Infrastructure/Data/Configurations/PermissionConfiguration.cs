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

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.PermissionString)
            .IsRequired()
            .HasMaxLength(5);

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
