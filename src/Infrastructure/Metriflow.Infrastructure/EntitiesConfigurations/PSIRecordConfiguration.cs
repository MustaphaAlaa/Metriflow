using Metriflow.Domain.Entities.Workers;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class PSIRecordConfiguration : IEntityTypeConfiguration<PSIRecord>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<PSIRecord> builder
    )
    {
        builder.HasNoKey();

        builder.Property(x => x.IsCorrelation).HasDefaultValue(false);

        builder.HasIndex(x => new { PageId = x.PageId, Date = x.Ticks }).IsClustered(false);

        builder.HasIndex(x => x.IsCorrelation).HasFilter("[IsCorrelation] = 1").IsClustered(false);

        builder
            .Property(x => x.Ticks)
            .HasConversion(ticks => new DateTime(ticks, DateTimeKind.Utc), date => date.Ticks)
            .HasJsonPropertyName("Date")
            .HasColumnName("Date");
    }
}
