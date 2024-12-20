#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.Data.Configurations;

public class ZoraProgramConfiguration : IEntityTypeConfiguration<ZoraProgram>
{
    public void Configure(EntityTypeBuilder<ZoraProgram> builder)
    {
        builder.ToTable("zora_programs");

        builder.Property(p => p.Id)
            .HasColumnName("work_item_id");

        builder.Property(zp => zp.Description)
            .HasColumnName("description")
            .IsRequired(false)
            .IsUnicode();

        builder.Property(zp => zp.Deleted)
            .HasColumnName("deleted")
            .HasDefaultValue(false);

        builder.HasMany(zp => zp.Projects)
            .WithOne(p => p.Program)
            .HasForeignKey(p => p.ProgramId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
