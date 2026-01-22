using Metriflow.Domain.Entities.Workers;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class GARecordConfiguration : IEntityTypeConfiguration<GARecord>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<GARecord> builder
    )
    {
        builder.HasKey(x=>x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
       
        builder.Property(x => x.Ticks)
            .HasConversion(ticks => new DateTime(ticks,DateTimeKind.Utc),
                date => date.Ticks)
            .HasJsonPropertyName("Date");


        builder.HasIndex(x => new { x.Ticks, x.Page, })
            .IsUnique(true);
    }
}
