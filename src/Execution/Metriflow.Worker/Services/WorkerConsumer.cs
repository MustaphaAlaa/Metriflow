using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Metriflow.Domain;
using Metriflow.DTOs;
using Metriflow.Messaging.interfaces;

namespace Metriflow.Worker.Services;

public interface IWorkerConsumer
{
    Task Consume(CancellationToken cancellationToken);
}

public class WorkerConsumer : IWorkerConsumer
{
    private readonly ILogger<WorkerConsumer> _logger;
    private readonly IRabbitMQConsumer _consumer;

    public WorkerConsumer(ILogger<WorkerConsumer> logger, IRabbitMQConsumer consumer)
    {
        _logger = logger;
        _consumer = consumer;
    }

    public async Task Consume(CancellationToken cancellationToken)
    {
        await _consumer.InitializeSharedChannelAsync(
            queueName: "analytic.q",
            exchangeName: "analytics.raw"
        );

        await _consumer.ConsumeWithSharedChannelAsync(
            queueName: "analytic.q",
            exchangeName: "analytics.raw",
            routingKey: "analytics.raw",
            (List<CombinedAnalyticsMessage> rcs) =>
            {
                foreach (var rc in rcs)
                {
                    System.Console.WriteLine(
                        $"From Worker Template, {rc.Date} -- {rc.Page} --  \n I'll be the one."
                    );
                }
                return Task.CompletedTask;
            },
            cancellationToken
        );
    }
}
