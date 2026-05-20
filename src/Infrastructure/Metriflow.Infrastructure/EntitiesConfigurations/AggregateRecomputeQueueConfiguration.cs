using Metriflow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Metriflow.Infrastructure.EntitiesConfigurations;

public class AggregateRecomputeQueueConfiguration
    : IEntityTypeConfiguration<AggregateRecomputeQueue>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<AggregateRecomputeQueue> builder
    )
    {
        builder.HasKey(aggregateRecomputeQueue => new
        {
            aggregateRecomputeQueue.PageId,
            aggregateRecomputeQueue.Date,
            aggregateRecomputeQueue.Interval,
        });
    }
}
