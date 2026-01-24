// using IRepository.Generic;
// using Metriflow.AggregationWorker.Interfaces;
// using Metriflow.Application.Interfaces;
// using Metriflow.Domain.Entities;
//
// namespace Metriflow.AggregationWorker.Services;
//
// public interface IAggregationWorkerConsumer
// {
//     Task Consume(CancellationToken cancellationToken);
// }
//
// public class AggregationWorkerConsumer : IAggregationWorkerConsumer
// {
//     private readonly ILogger<AggregationWorkerConsumer> _logger;
//     private readonly IMessageBrokerConsumer _consumer;
//     private readonly IUnitOfWork _unitOfWork;
//     private readonly IBaseRepository<DailyAnalytics> _dailyStateRepository;
//     private readonly IBaseRepository<PageId> _pageRepository;
//     private readonly IBaseRepository<PageAnalytics> _rawDataRepository;
//     private readonly IAggregationConsumer _aggregationConsumer;
//
//     public AggregationWorkerConsumer(
//         IBaseRepository<PageId> pageRepository,
//         IBaseRepository<PageAnalytics> rawDataRepo,
//         ILogger<AggregationWorkerConsumer> logger,
//         IMessageBrokerConsumer consumer,
//         IAggregationConsumer aggregationConsumer,
//         IUnitOfWork unitOfWork
//     )
//     {
//         _rawDataRepository = rawDataRepo;
//         _pageRepository = pageRepository;
//         _logger = logger;
//         _unitOfWork = unitOfWork;
//         _consumer = consumer;
//         _dailyStateRepository = _unitOfWork.GetRepository<DailyAnalytics>();
//         _aggregationConsumer = aggregationConsumer;
//     }
//
//     public async Task Consume(CancellationToken cancellationToken)
//     {
//         await _consumer.InitializeSharedChannelAsync(
//             queueName: "analytic.q",
//             exchangeName: "analytics.raw"
//         );
//
//         await _consumer.ConsumeWithSharedChannelAsync(
//             queueName: "analytic.q",
//             exchangeName: "analytics.raw",
//             routingKey: "analytics.raw",
//             async (List<CombinedAnalyticsMessage> rcs) =>
//             {
//                 await _aggregationConsumer.Consume(rcs);
//             },
//             cancellationToken
//         );
//     }
// }
