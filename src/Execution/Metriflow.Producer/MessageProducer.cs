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
    private readonly ISeedData _seedData;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly ILogger<MessageProducer> _logger;

    private readonly IProducer _producer;

    /// <summary>
    /// Initializes a new instance of the MessageProducer class.
    /// </summary>
    /// <param name="seedData">The seed data provider for analytics records.</param>
    /// <param name="producer">The producer instance for publishing messages.</param>
    /// <param name="appLifetime">The application lifetime control.</param>
    /// <param name="logger">The logger instance for logging service events.</param>
    public MessageProducer(
        ISeedData seedData,
        IProducer producer,
        IHostApplicationLifetime appLifetime,
        ILogger<MessageProducer> logger
    )
    {
        _logger = logger;
        _producer = producer;
        _appLifetime = appLifetime;
        _seedData = seedData;
    }

    /// <summary>
    /// Starts the message production process when the application starts.
    /// </summary>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <remarks>
    /// This method will:
    /// 1. Load the seed data
    /// 2. Produce all records to RabbitMQ
    /// 3. Stop the application when complete or if an error occurs
    /// </remarks>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Start sending data......");

            await _seedData.SeedingData();
            var GARecords = _seedData.GARecords;
            var PSIRecords = _seedData.PSIRecords;

            await _producer.Produce(GARecords, PSIRecords);

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
