#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("zora_users");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .UseIdentityColumn();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode();

        builder.Property(u => u.Password)
            .HasColumnName("password")
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(255)
            .IsUnicode();

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .HasDefaultValueSql("GETDATE()");

        builder.Property(u => u.Deleted)
            .HasColumnName("deleted")
            .HasDefaultValue(false);

        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("UQ_User_Username");

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("UQ_User_Email");

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.AssignedWorkItems)
            .WithOne(wi => wi.Assignee)
            .HasForeignKey(wi => wi.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.CreatedWorkItems)
            .WithOne(wi => wi.CreatedBy)
            .HasForeignKey(wi => wi.CreatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.UpdatedWorkItems)
            .WithOne(wi => wi.UpdatedBy)
            .HasForeignKey(wi => wi.UpdatedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.ManagedProjects)
            .WithOne(p => p.ProjectManager)
            .HasForeignKey(p => p.ProjectManagerId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
