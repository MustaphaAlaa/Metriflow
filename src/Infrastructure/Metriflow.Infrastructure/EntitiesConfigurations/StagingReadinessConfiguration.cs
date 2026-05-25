using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class StagingReadinessConfiguration : IEntityTypeConfiguration<StagingReadiness>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<StagingReadiness> builder
    )
    {
        builder.Property(sr => sr.Source).IsRequired();
        builder.Property(sr => sr.BatchId).IsRequired();
        builder.Property(sr => sr.Consumed).HasDefaultValue(false);
        builder.Property(sr => sr.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        builder.HasKey(sr => new { sr.Source, sr.BatchId });
    }
}