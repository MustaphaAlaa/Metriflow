// using IRepository.Generic;
// using Metriflow.Application.Interfaces;
// using Metriflow.Domain.CustomAttributes;
// using Metriflow.Domain.Entities;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
//
// namespace Metriflow.AggregationWorker.Services;
//
// [ServiceRegistration(ServiceLifetime.Scoped, typeof(IDailyStatOrchestrator))]
// public class DailyAnalyticsOrchestrator(
//     IDailyAnalyticsService dailyStatCalculator,
//     IDailyAnalyticsRepository dailyStatRepository,
//     IUnitOfWork unitOfWork,
//     ILogger<DailyAnalyticsOrchestrator> logger)
//     : IDailyStatOrchestrator
// {
//     public async Task CalculateAndPersist(List<CombinedAnalyticsMessage> combinedAnalyticsMessages)
//     {
//         await ExecuteTransactionAsync(async () =>
//         {
//             var calculatedDailyState = await dailyStatCalculator.CalculateDailyStat(
//                 combinedAnalyticsMessages
//             );
//             var dailyStat = await dailyStatRepository.CreateAsync(calculatedDailyState);
//         });
//     }
//
//     private async Task ExecuteTransactionAsync(Func<Task> action)
//     {
//         try
//         {
//             await unitOfWork.BeginTransactionAsync();
//             await action();
//             await unitOfWork.SaveChangesAsync();
//             await unitOfWork.CommitAsync();
//         }
//         catch (Exception e)
//         {
//             await unitOfWork.RollbackAsync();
//             logger.LogError(e, "Raw data ingestion failed during transaction.");
//             throw;
//         }
//     }
// }