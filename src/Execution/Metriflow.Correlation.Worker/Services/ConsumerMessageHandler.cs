using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Metriflow.Application.Interfaces.Caches;
using Metriflow.Application.Models.Enums;
using Metriflow.Correlation.Worker.Interfaces;
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
    public async Task HandleIncomingRecordAsync<T>(string type, IList<T> record)
        where T : IAnalyticRecord
    {
        if (record is null || record.Count == 0)
        {
            _logger.LogDebug("HandleIncomingRecordAsync called with no items; nothing to handle.");
            return;
        }
        _logger.LogInformation(
            "Start Handling incoming Request, {type} ==> {record}",
            type,
            record
        );

        var date = await SaveNewRecordAsync(type, record);
    }

    private long GetTheDayOfTicks<T>(T record)
        where T : IAnalyticRecord
    {
        var ticks = record.Date % TimeSpan.TicksPerDay;
        if (ticks == 0)
            return record.Date;
        return record.Date - ticks;
    }

    private async Task<DateTime> SaveNewRecordAsync<T>(string type, IList<T> record)
        where T : IAnalyticRecord
    {
        //!!! SOLID Doesn't Applied Correctly Here !!!
        //!!note:
        // I will let it as it, and I'll comeback later and I'll refactor it.
        // I need to move forward for now.
        DateTime date = default;
        foreach (var rec in record)
        {
            var listName = $"{type}|{GetTheDayOfTicks(rec)}|{rec.Page}";
         // Alternative of Json could be used to lighter in serialization and deserialization.
         // But I'll keep Json because I don't have time.
            var recordJson = JsonSerializer.Serialize(rec);
            var listLength = await _redis.AddLastAsync(listName, recordJson);
            if (listLength == 24 && type == "GA")
            {
                await _redis.AddLastAsync(
                    enCompletedListsNames.CompletedListPSI.ToString(),
                    listName
                );
            }
            else if (listLength == 24 && type == "PSI")
            {
                await _redis.AddLastAsync(
                    enCompletedListsNames.CompletedListGA.ToString(),
                    listName
                );
            }
            _logger.LogInformation(
                "Saved record for type '{type}' with key '{fieldKey}'.",
                type,
                listName
            );
        }

        return date;
    }
}
