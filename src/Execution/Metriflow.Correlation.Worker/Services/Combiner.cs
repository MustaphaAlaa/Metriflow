using Metriflow.Correlation.Worker.Interfaces;
using Metriflow.Domain;

namespace Metriflow.Correlation.Worker;

public class Combiner : ICombiner
{
    private readonly ILogger<Combiner> _logger;
    private readonly IRowRecordProducer _producer;

    public Combiner(ILogger<Combiner> logger, IRowRecordProducer producer)
    {
        _logger = logger;
        this._producer = producer;
    }

    public async Task GA_PSI_Combiner(GARecord ga, PSIRecord psi)
    {
        _logger.LogInformation($"Combing GA + PSI for {ga.Page} on {ga.Date}");

        var raw = new RawRecord()
        {
            Date = ga.Date,
            Page = ga.Page,
            LCP_MS = psi.LCP_MS,
            PerformanceScore = psi.PerformanceScore,
            Sessions = ga.Sessions,
            Users = ga.Users,
            Views = ga.Views,
        };

        await _producer.PublishRawRecord(raw);
    }
}
