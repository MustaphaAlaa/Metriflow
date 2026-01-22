using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;

namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Default implementation of <see cref="IConsumerMessageHandler"/>. Stores incoming records in Redis,
/// tracks the maximum received date, and triggers matching/combining logic when appropriate.
/// </summary>
// [ServiceRegistration(ServiceLifetime.Scoped, typeof(IConsumerMessageHandler<>))]
public class ConsumerMessageHandler<T>(
    ILogger<ConsumerMessageHandler<T>> logger,
    IProducer producer,
    IBaseRepository<T> repository
)
    : IConsumerMessageHandler<T> where T : class, IAnalyticRecord
{
    /// <inheritdoc />
    public async Task HandleIncomingRecordAsync(enTypesKey type, List<T> records)
    {
        if (records is null || records.Count == 0)
        {
            logger.LogDebug("HandleIncomingRecordAsync called with no items; nothing to handle.");
            return;
        }

        try
        {
            logger.LogInformation(
                "Start Handling incoming Request, for type: {type}",
                type
            );
            if (records.Count >= 1000)
            {
                foreach (var analyticRecords in records.Chunk(1000))
                {
                    
                    await repository.CreateRangeAsync(analyticRecords);
                    await repository.SaveChangesAsync();
                    repository.ClearTracking();
                }
            }
            else
            {
                await repository.CreateRangeAsync(records);
                await repository.SaveChangesAsync();
            }

            logger.LogInformation(
                "Saved records for type '{type}' with key.",
                type
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling incoming records.");
            throw;
        }
    }
}