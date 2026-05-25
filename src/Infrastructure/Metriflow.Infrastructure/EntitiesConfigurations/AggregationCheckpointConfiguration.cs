using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class AggregationCheckpointConfiguration : IEntityTypeConfiguration<AggregationCheckpoint>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AggregationCheckpoint> builder
    )
    {
        builder.HasData(this.AggregationCheckpointData());
    }

    private List<AggregationCheckpoint> AggregationCheckpointData()
    {
        var aggregationCheckpointList = new List<AggregationCheckpoint>()
        {
            new()
            {
                PipelineName = "TimeIntervalAggregation",
                LastProcessedAt = new DateTime()
            },
            new()
            {
                PipelineName = "DailyAggregation", LastProcessedAt = new DateTime()
            },
            new()
            {
                PipelineName = "MonthlyAggregation", LastProcessedAt = new DateTime()
            },
            new()
            {
                PipelineName = "YearlyAggregation", LastProcessedAt = new DateTime()
            },
        };
        return aggregationCheckpointList;
    }
}