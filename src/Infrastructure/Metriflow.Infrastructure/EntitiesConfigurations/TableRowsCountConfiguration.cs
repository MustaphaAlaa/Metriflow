using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class TableRowsCountConfiguration : IEntityTypeConfiguration<TableRowsCount>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<TableRowsCount> builder
    )
    {
        builder.HasKey(e => e.Id);

        builder.HasData(
            new List<TableRowsCount>()
            {
                new() { Id = 1, RowsCount = 0, TableName = "GARecords" },
                new() { Id = 2, RowsCount = 0, TableName = "PSARecords" },
                new() { Id = 3, RowsCount = 0, TableName = "AggregationProgresses" },
                new() { Id = 4, RowsCount = 0, TableName = "PageAnalytics" },
                new() { Id = 5, RowsCount = 0, TableName = "TimeIntervalsAnalytics" },
                new() { Id = 6, RowsCount = 0, TableName = "DailyAnalytics" },
                new() { Id = 7, RowsCount = 0, TableName = "MonthlyAnalytics" },
                new() { Id = 8, RowsCount = 0, TableName = "YearlyAnalytics" },
            }
        );
    }
}