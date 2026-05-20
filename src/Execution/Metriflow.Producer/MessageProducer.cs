using System.Threading.Channels;
using Metriflow.Application.Entities;
using Metriflow.Application.Interfaces;
using Metriflow.Application.Interfaces.Workers;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Entities.Workers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Metriflow.Producers.Implementation;

/// <summary>
/// Implements a hosted service that manages the lifecycle of the analytics data production process.
/// </summary>
public class MessageProducer(
    IStreamData streamData,
    IProducer producer,
    IHostApplicationLifetime appLifetime,
    ILogger<MessageProducer> logger,
    IOptions<RabbitMqSettings> options,
    IHostEnvironment environment)
    : IHostedService
{
    private readonly RabbitMqSettings _settings = options.Value;
    // private readonly IMessageBrokerConnection _messageBrokerConnection;

    private string JsonFilePath(string filename) => Path.Combine(environment.ContentRootPath, "data", filename);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            const int batchSize = 25000;
            logger.LogInformation("Start sending data......");
            var gaMockJson = this.JsonFilePath("GA-mock.json");
            var psaMockJson = this.JsonFilePath("PSA-mock.json");

            var gaTask = streamData.RunPipelineAsync<GARecord>(
                gaMockJson,
                batchSize,
                (gaRecords) =>
                    producer.PublishAnalyticRecords(gaRecords, _settings.Queues.GA, _settings.Exchange)
            );

            var psaTask = streamData.RunPipelineAsync<PSARecord>(
                psaMockJson,
                batchSize,
                (psaRecords) =>
                    producer.PublishAnalyticRecords(psaRecords, _settings.Queues.PSA, _settings.Exchange)
            );

            await Task.WhenAll(gaTask, psaTask);

            logger.LogInformation("All files is processed.");
        }
        catch (Exception ex)
        {
            logger.LogError($"An exception thrown. {ex}", ex.Message, ex);
        }
        finally
        {
            appLifetime.StopApplication();
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