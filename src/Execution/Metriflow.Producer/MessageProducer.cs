using System.Threading.Channels;
using Metriflow.Application.interfaces;
using Metriflow.Producers.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Metriflow.Producers.Implementation;

/// <summary>
/// Implements a hosted service that manages the lifecycle of the analytics data production process.
/// </summary>
public class MessageProducer : IHostedService
{
    private readonly IRabbitMQConnection _rabbitMQConnection;
    private readonly IStreamData _streamData;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<MessageProducer> _logger;

    private readonly IProducer _producer;

    
    public MessageProducer(
        IStreamData streamData,
        IProducer producer,
        IHostApplicationLifetime appLifetime,
        ILogger<MessageProducer> logger
    )
    {
        _logger = logger;
        _producer = producer;
        _appLifetime = appLifetime;
        _streamData = streamData;
    }

    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start sending data......"); 


            var GA = _streamData.RunPipelineAsync<GARecord>("GA-mock.json", 1500,
                (gaRecords, channel) => _producer.ProduceGAAsync(gaRecords, channel));

            var PSI = _streamData.RunPipelineAsync<PSIRecord>("PSI-mock.json", 1500,
                (psiRecords, channel) => _producer.ProducePSIAsync(psiRecords, channel));
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