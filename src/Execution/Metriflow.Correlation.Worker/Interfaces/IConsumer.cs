namespace Metriflow.Correlation.Worker.Interfaces;

public interface IConsumer
{
    Task Consume(CancellationToken stoppingToken);
}
