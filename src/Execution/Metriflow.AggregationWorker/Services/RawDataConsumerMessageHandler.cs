using System.Runtime.CompilerServices;
using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces.Correlation;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Entities.Workers;
using Metriflow.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace Metriflow.AggregationWorker.Services;

/// <summary>
/// Default implementation of <see cref="IRawDataConsumerMessageHandler{T}"/>. Stores incoming records in Redis,
/// tracks the maximum received date, and triggers matching/combining logic when appropriate.
/// </summary>
public class RawDataConsumerMessageHandler<T>(
    ILogger<RawDataConsumerMessageHandler<T>> logger,
    IOptions<RabbitMqSettings> options,
    IAggregationProgressRepository aggregationProgressRepository,
    IBaseRepository<T> repository
)
    : IRawDataConsumerMessageHandler<T> where T : class, IAnalyticRecord
{
    private readonly RabbitMqSettings _rabbitMqSettings = options.Value;

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
                $"@@@@@@RawDataConsumerMessageHandler<{type}>Start Handling incoming Request, for type: {type}"
            );
            if (records.Count >= 1000)
            {
                foreach (var analyticRecords in records.Chunk(1000))
                {
                    // var keys = analyticRecords.Select(r =>
                    //     new AggregationKey()
                    //     {
                    //         Date = new DateTime(r.Ticks, DateTimeKind.Utc),
                    //         PageId = r.Page
                    //     });
                   
                    await repository.CreateRangeAsync(analyticRecords);
                    await this.CreateAggregationProgress(type, records);

                    await aggregationProgressRepository.SaveChangesAsync();
                    repository.ClearTracking();
                }
            }
            else
            {
                await repository.CreateRangeAsync(records);
                await this.CreateAggregationProgress(type, records);
                await repository.SaveChangesAsync();
                repository.ClearTracking();
            }

            logger.LogInformation($"@@@@@@@{type}records have been created.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "@@@@@@@@@@@@@@@@@@@@@@@@Error handling incoming records.");
            throw;
        }
    }

    private async Task CreateAggregationProgress(enTypesKey type, List<T> records)
    {
        var keys = records.Select(r =>
            new AggregationKey()
            {
                Date = new DateTime(r.Ticks, DateTimeKind.Utc),
                PageId = r.Page
            });
        logger.LogDebug("#@@@Keyssssssssss");
        logger.LogDebug("#@@@Keyssssssssss");
        logger.LogDebug(string.Join(", ", keys));
        await aggregationProgressRepository.CreateRangeWithKeysAsync(keys);
        logger.LogInformation($"@@@@@@@{type}, AggregationProgresses have been created.");
    }
}