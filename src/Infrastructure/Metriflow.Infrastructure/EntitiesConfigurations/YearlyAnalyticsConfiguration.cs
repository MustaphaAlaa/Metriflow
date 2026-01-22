using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class YearlyAnalyticsConfiguration : IEntityTypeConfiguration<YearlyAnalytics>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<YearlyAnalytics> builder
    )
    {
        try
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(ds => new { ds.PageId, ds.Year })
                .IsUnique();
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception In YearlyAnalyticsConfiguration");
            Console.WriteLine(ex.Message);
            Console.WriteLine();
        }
    }
}