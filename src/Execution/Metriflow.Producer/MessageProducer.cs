using System.Threading.Channels;
using Metriflow.Application.interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.Entities.Workers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metriflow.Producers.Implementation;

/// <summary>
/// Implements a hosted service that manages the lifecycle of the analytics data production process.
/// </summary>
public class MessageProducer : IHostedService
{
    // private readonly IMessageBrokerConnection _messageBrokerConnection;
    private readonly IStreamData _streamData;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<MessageProducer> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IProducer _producer;

    public MessageProducer(
        IStreamData streamData,
        IProducer producer,
        IHostApplicationLifetime appLifetime,
        ILogger<MessageProducer> logger,
            IHostEnvironment environment
    )
    {
        _logger = logger;
        _producer = producer;
        _appLifetime = appLifetime;
        _streamData = streamData;
        _environment =    environment;
    }

    private string JsonFilePath(string filename) =>  Path.Combine(_environment.ContentRootPath, "data",filename); 
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {

            _logger.LogInformation("Start sending data......");
            var gaMockJson =  this.JsonFilePath( "GA-mock.json") ;
            var psiMockJson = this.JsonFilePath( "PSI-mock.json") ;
            
            var GA = _streamData.RunPipelineAsync<GARecord>(
               gaMockJson,
                2300,
                (gaRecords) => _producer.PublishAnalyticRecords(gaRecords,"analytics.raw.GA","analytics.raw.GA")
            );

            var PSI = _streamData.RunPipelineAsync<PSIRecord>(
                psiMockJson,
                2300,
                (psiRecords) => _producer.PublishAnalyticRecords(psiRecords, "analytics.raw.PSI","analytics.raw.PSI")
            );
            await Task.WhenAll(GA, PSI);

            _logger.LogInformation("All files is processed.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An exception thrown. {ex}", ex.Message, ex);
        }
        finally
        {
            _appLifetime.StopApplication();
        }
    }

    /// <summary>
    /// Handles the shutdown of the message producer service.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
