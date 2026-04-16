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
           new TableRowsCount[]
           {
               new TableRowsCount{Id = 1, RowsCount= 0, TableName = "GARecords"},
               new TableRowsCount{Id = 2, RowsCount= 0, TableName = "PSIRecords"},
               new TableRowsCount{Id = 3, RowsCount= 0, TableName = "AggregationProgresses"},
               new TableRowsCount{Id = 4, RowsCount= 0, TableName = "PageAnalytics"},
               new TableRowsCount{Id = 5, RowsCount= 0, TableName = "TimeIntervalsAnalytics"},
               new TableRowsCount{Id = 6, RowsCount= 0, TableName = "DailyAnalytics"},
               new TableRowsCount{Id = 7, RowsCount= 0, TableName = "MonthlyAnalytics"},
               new TableRowsCount{Id = 8, RowsCount= 0, TableName = "YearlyAnalytics"},
           }
        );

    }
}