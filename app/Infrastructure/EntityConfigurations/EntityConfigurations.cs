#region

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using zora.Core.Domain;

#endregion

namespace zora.Infrastructure.EntityConfigurations;

public class ZoraProgramConfiguration : IEntityTypeConfiguration<ZoraProgram>
{
    public void Configure(EntityTypeBuilder<ZoraProgram> builder)
    {
        builder.Property(p => p.Description)
            .HasColumnType("text");

        builder.Ignore(p => p.SourceRelationships);
        builder.Ignore(p => p.TargetRelationships);
    }
}
