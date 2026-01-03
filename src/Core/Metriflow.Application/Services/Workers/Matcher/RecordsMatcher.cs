using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Caches;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Application.Models.Enums;
using Metriflow.Application.Worker;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Metriflow.Application.Services.Workers;


[ServiceRegistration(ServiceLifetime.Scoped, typeof(IRecordsMatcher))]

public class RecordsMatcher(
    ILogger<RecordsMatcher> logger,
    IAnalyticsCacheServices analyticsCacheServices,
    IAnalyticRecordsDeserializer analyticRecordsDeserializer,
    IMessageBrokerProducer messageBrokerProducer,
    IListsKeysServices listsKeysServices,
    IAnalyticRecordsCombiner analyticRecordsCombiner,
    IRecordMatchingWorkflow recordMatchingWorkflow,
    IOptions<AnalyticsOptions> options)
    : IRecordsMatcher
{
    private readonly IAnalyticRecordsDeserializer _analyticRecordsDeserializer = analyticRecordsDeserializer;
    private readonly IAnalyticRecordsCombiner _analyticRecordsCombiner = analyticRecordsCombiner;

    private readonly AnalyticsOptions _analyticsOptions = options.Value;

    public async Task MatchRecords(
        Dictionary<enCompletedListsNames, IEnumerable<string>> completedListsKeysDic
    )
    {
        try
        {
            var keysSet = listsKeysServices.GetCompletedListsSharedKeys(completedListsKeysDic);

            foreach (var key in keysSet)
            {
                var keys = listsKeysServices.GetAllKeysWithPrefixes(key);

                var combinedRecords = await recordMatchingWorkflow.TryMatchAsync(keys);

                if (this.IsResultValid(combinedRecords))
                {
                    await messageBrokerProducer.PublishAsync(
                        combinedRecords,
                        _analyticsOptions.ExchangeName,
                        "analytics.combined",
                        true
                    );
                    Console.WriteLine("Before Removing........");
                    await analyticsCacheServices.RemoveKeysFromCompletedLists(keys);

                    logger.LogInformation(
                        $"Published raw {combinedRecords.Count} records to '{_analyticsOptions.ExchangeName}':\n{string.Join(" \t\t\n", combinedRecords)}"
                    );
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to match records");
            throw;
        }
    }

    private bool IsResultValid(IList<CombinedAnalyticsMessage>? combinedRecords)
    {
        return combinedRecords != null && combinedRecords.Any(r => r is not null);
    }
}
