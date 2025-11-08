using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class RawDataConfiguration : IEntityTypeConfiguration<RawData>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<RawData> builder
    )
    {
        builder.HasIndex(rd => rd.PageId);
    }
}