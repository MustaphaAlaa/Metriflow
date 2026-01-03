using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Caches;

namespace Metriflow.Matcher.Worker;

public class MatcherWorker(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<MatcherWorker> logger)
    : BackgroundService
{
    private readonly ILogger<MatcherWorker> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var scopedProvider = scope.ServiceProvider;
        var redis = scopedProvider.GetRequiredService<IAnalyticsCacheServices>();
        
            var mat = scopedProvider.GetRequiredService<IRecordsMatcher>();
        while (!stoppingToken.IsCancellationRequested)
        {
            var r = await redis.GetCompletedListsKeysAsync();
 
            // var prefixes = new List<string>() { "GA", "PSI" }; 

            await mat.MatchRecords(r);

          
            Console.WriteLine("Iterate Again");

            await Task.Delay(5000, stoppingToken);
        }
    }
}