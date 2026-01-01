using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Caches;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Application.Models.Enums;
using Metriflow.Application.Worker;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Metriflow.Application.Services.Workers;

public sealed class AnalyticsOptions
{
    public string ExchangeName { get; init; } = default!;
    public int HoursPerDay { get; init; }
}

public class RecordsMatcher : IRecordsMatcher
{
    private readonly ILogger<RecordsMatcher> _logger;
    private readonly IAnalyticsCacheServices _analyticsCacheServices;

    private readonly IAnalyticRecordsDeserializer _analyticRecordsDeserializer;
    private readonly IMessageBrokerProducer _messageBrokerProducer;
    private readonly IListsKeysServices _listsKeysServices;
    private readonly IAnalyticRecordsCombiner _analyticRecordsCombiner;

    private readonly AnalyticsOptions _analyticsOptions;
    private readonly IRecordMatchingWorkflow _recordMatchingWorkflow;

    public RecordsMatcher(
        ILogger<RecordsMatcher> logger,
        IAnalyticsCacheServices analyticsCacheServices,
        IAnalyticRecordsDeserializer analyticRecordsDeserializer,
        IMessageBrokerProducer messageBrokerProducer,
        IListsKeysServices listsKeysServices,
        IAnalyticRecordsCombiner analyticRecordsCombiner,
        IRecordMatchingWorkflow recordMatchingWorkflow,
        IOptions<AnalyticsOptions> options
    )
    {
        _logger = logger;
        _analyticsCacheServices = analyticsCacheServices;
        _analyticRecordsDeserializer = analyticRecordsDeserializer;
        _messageBrokerProducer = messageBrokerProducer;
        _listsKeysServices = listsKeysServices;
        _analyticRecordsCombiner = analyticRecordsCombiner;
        _recordMatchingWorkflow = recordMatchingWorkflow;
        _analyticsOptions = options.Value;
    }

    public async Task MatchRecords(
        Dictionary<enCompletedListsNames, IEnumerable<string>> completedListsKeysDic
    )
    {
        try
        {
            var keysSet = _listsKeysServices.GetCompletedListsSharedKeys(completedListsKeysDic);

            foreach (var key in keysSet)
            {
                var keys = _listsKeysServices.GetAllKeysWithPrefixes(key);

                var combinedRecords = await _recordMatchingWorkflow.TryMatchAsync(keys);

                if (this.IsResultValid(combinedRecords))
                {
                    await _messageBrokerProducer.PublishAsync(
                        combinedRecords,
                        _analyticsOptions.ExchangeName,
                        "analytics.raw.combined",
                        true
                    );
                    Console.WriteLine("Before Removing........");
                    await _analyticsCacheServices.RemoveKeysFromCompletedLists(keys);

                    _logger.LogInformation(
                        $"Published raw {combinedRecords.Count} records to '{_analyticsOptions.ExchangeName}':\n{string.Join(" \t\t\n", combinedRecords)}"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to match records");
            throw;
        }
    }

    private bool IsResultValid(IList<CombinedAnalyticsMessage>? combinedRecords)
    {
        return combinedRecords != null && combinedRecords.Any(r => r is not null);
    }
}
