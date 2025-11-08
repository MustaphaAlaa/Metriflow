using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class YearlyStatConfiguration : IEntityTypeConfiguration<YearlyStat>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<YearlyStat> builder
    )
    {
        builder
            .HasAlternateKey(ds => (new { ds.PageId, ds.Year }))
            .HasName("AK_YearlyStats_PageYear");

        builder.HasIndex(ds => ds.Year);
    }
}