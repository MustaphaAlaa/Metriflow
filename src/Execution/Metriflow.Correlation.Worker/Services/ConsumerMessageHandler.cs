using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Metriflow.Application.Interfaces.Caches;
using Metriflow.Application.Models.Enums;
using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Interfaces;

namespace Metriflow.Correlation.Worker;

/// <summary>
/// Default implementation of <see cref="IConsumerMessageHandler"/>. Stores incoming records in Redis,
/// tracks the maximum received date, and triggers matching/combining logic when appropriate.
/// </summary>
public class ConsumerMessageHandler : IConsumerMessageHandler
{
    private readonly ILogger<CorrelationWorker> _logger;
    private readonly ICacheService _redis;

    /// <summary>
    /// Creates a new <see cref="ConsumerMessageHandler"/>.
    /// </summary>
    public ConsumerMessageHandler(ILogger<CorrelationWorker> logger, ICacheService redis)
    {
        _logger = logger;
        _redis = redis;
    }

    /// <inheritdoc />
    public async Task HandleIncomingRecordAsync<T>(enTypesKey type, IList<T> record)
        where T : IAnalyticRecord
    {
        if (record is null || record.Count == 0)
        {
            _logger.LogDebug("HandleIncomingRecordAsync called with no items; nothing to handle.");
            return;
        }

        _logger.LogInformation(
            "Start Handling incoming Request, for type: {type}",
            type
        );

        var date = await SaveNewRecordAsync(type, record);
    }

    private long GetTheDayOfTicks<T>(T record)
        where T : IAnalyticRecord
    {
        var ticks = record.Ticks % TimeSpan.TicksPerDay;
        if (ticks == 0)
            return record.Ticks;
        return record.Ticks - ticks;
    }

    private async Task<DateTime> SaveNewRecordAsync<T>(enTypesKey type, IList<T> record)
        where T : IAnalyticRecord
    {
        //!!! SOLID Doesn't Applied Correctly Here !!!
        //!!note:
        // I will let it as it, and I'll comeback later and I'll refactor it.
        // I need to move forward for now.
        DateTime date = default;
        foreach (var rec in record)
        {
            //the completed lists will take the shared key instead of the full key to be easier and faster in th combination
            var sharedListName = $"{GetTheDayOfTicks(rec)}|{rec.Page}";
            var listName = $"{type}|{sharedListName}";

            // Alternative of Json could be used to lighter in serialization and deserialization.
            // But I'll keep Json because I don't have time.

            var recordJson = JsonSerializer.Serialize(rec);
            var listLength = await _redis.AddLastAsync(listName, recordJson);
        
            if (listLength != 24)
                continue;

            var completedList = type switch
            {
                enTypesKey.GA  => enCompletedListsNames.CompletedListGA,
                enTypesKey.PSI => enCompletedListsNames.CompletedListPSI,
                _ => throw new ArgumentOutOfRangeException(nameof(type))
            };

            await _redis.AddLastAsync(completedList.ToString(), sharedListName);

            _logger.LogInformation(
                "Saved record for type '{type}' with key '{fieldKey}'.",
                type,
                listName
            );
        }

        return date;
    }
}