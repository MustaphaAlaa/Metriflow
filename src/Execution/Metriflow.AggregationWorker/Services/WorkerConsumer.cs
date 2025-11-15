using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRepository.Generic;
using Metriflow.AggregationWorker.Interfaces;
using Metriflow.Application.interfaces;
using Metriflow.Domain;
using Metriflow.Domain.Entities;
using Metriflow.DTOs;

namespace Metriflow.AggregationWorker.Services;

public interface IAggregationWorkerConsumer
{
    Task Consume(CancellationToken cancellationToken);
}

public class AggregationWorkerConsumer : IAggregationWorkerConsumer
{
    private readonly ILogger<AggregationWorkerConsumer> _logger;
    private readonly IRabbitMQConsumer _consumer;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBaseRepository<DailyStat> _dailyStateRepository;
    private readonly IBaseRepository<Page> _pageRepository;
    private readonly IBaseRepository<RawData> _rawDataRepository;
    private readonly IAggregationConsumer _aggregationConsumer;

    public AggregationWorkerConsumer(
        IBaseRepository<Page> pageRepository,
        IBaseRepository<RawData> rawDataRepo,
        ILogger<AggregationWorkerConsumer> logger,
        IRabbitMQConsumer consumer,
        IAggregationConsumer aggregationConsumer,
        IUnitOfWork unitOfWork
    )
    {
        _rawDataRepository = rawDataRepo;
        _pageRepository = pageRepository;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _consumer = consumer;
        _dailyStateRepository = _unitOfWork.GetRepository<DailyStat>();
        _aggregationConsumer = aggregationConsumer;
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
            async (List<CombinedAnalyticsMessage> rcs) =>
            {
                await _aggregationConsumer.Consume(rcs);
            },
            cancellationToken
        );
    }
}
