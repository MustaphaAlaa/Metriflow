using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Domain;
using Microsoft.Extensions.Logging;

namespace Metriflow.Correlation.Worker;

/// <summary>
/// Combines matching GA and PSI records into raw domain records and forwards them to a producer.
/// </summary>
public class Combiner : ICombiner
{
    private readonly ILogger<Combiner> _logger;
    private readonly IRowRecordProducer _producer;

    /// <summary>
    /// Creates a new <see cref="Combiner"/> instance.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="producer">Producer used to publish combined raw records.</param>
    public Combiner(ILogger<Combiner> logger, IRowRecordProducer producer)
    {
        _logger = logger;
        this._producer = producer;
    }

    /// <inheritdoc />
    public async Task GA_PSI_Combiner(List<Tuple<GARecord, PSIRecord>>? GA_PSI_LIST)
    {
        if (GA_PSI_LIST is null || GA_PSI_LIST.Count == 0)
        {
            _logger.LogDebug("GA_PSI_Combiner called with no items; nothing to combine.");
            return;
        }

        List<RawRecord> rawRecords = new();
        foreach (var tuple in GA_PSI_LIST)
        {
            _logger.LogInformation(
                $"Combing GA + PSI for {tuple.Item2.Page} on {tuple.Item2.Date}"
            );
            rawRecords.Add(
                new RawRecord()
                {
                    Date = tuple.Item1.Date,
                    Page = tuple.Item1.Page,
                    Sessions = tuple.Item1.Sessions,
                    Users = tuple.Item1.Users,
                    Views = tuple.Item1.Views,
                    PerformanceScore = tuple.Item2.PerformanceScore,
                    LCP_MS = tuple.Item2.LCP_MS,
                }
            );
        }
        await _producer.PublishRawRecord(rawRecords);
    }
}
