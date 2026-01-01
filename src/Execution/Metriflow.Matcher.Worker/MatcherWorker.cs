using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Caches;

namespace Metriflow.Matcher.Worker;

public class MatcherWorker : BackgroundService
{
    private readonly ILogger<MatcherWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public MatcherWorker(IServiceScopeFactory serviceScopeFactory,
        ILogger<MatcherWorker> logger)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var scopedProvider = scope.ServiceProvider;
        var redis = scopedProvider.GetRequiredService<IAnalyticsCacheServices>();
        
            var mat = scopedProvider.GetRequiredService<IRecordsMatcher>();
        while (!stoppingToken.IsCancellationRequested)
        {
            var r = await redis.GetCompletedListsKeysAsync();
 
            // var prefixes = new List<string>() { "GA", "PSI" }; 

            await mat.MatchRecords(r);

          
            Console.WriteLine("Iterate Again");
        }
    }
}