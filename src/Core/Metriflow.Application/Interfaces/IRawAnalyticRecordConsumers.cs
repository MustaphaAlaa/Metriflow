namespace Metriflow.Application.Interfaces;

public interface IRawAnalyticRecordConsumers<T>
{
    Task Consume(string queueName, string routingKey, CancellationToken stoppingToken);
}
