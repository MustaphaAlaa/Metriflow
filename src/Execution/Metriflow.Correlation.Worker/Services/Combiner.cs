// using Metriflow.Correlation.Worker.Interfaces;
// using Metriflow.Domain;
// using Metriflow.DTOs;
// using Microsoft.Extensions.Logging;

// namespace Metriflow.Correlation.Worker;

// /// <summary>
// /// Combines matching GA and PSI records into raw domain records and forwards them to a producer.
// /// </summary>
// public class Combiner : ICombiner
// {
//     private readonly ILogger<Combiner> _logger;
//     private readonly IRowDataProducer _producer;

//     /// <summary>
//     /// Creates a new <see cref="Combiner"/> instance.
//     /// </summary>
//     /// <param name="logger">Logger for diagnostic output.</param>
//     /// <param name="producer">Producer used to publish combined raw records.</param>
//     public Combiner(ILogger<Combiner> logger, IRowDataProducer producer)
//     {
//         _logger = logger;
//         this._producer = producer;
//     }

//     /// <inheritdoc />
//     public async Task GA_PSI_Combiner(List<recordGA_PSI>? GA_PSI_LIST)
//     {
//         if (GA_PSI_LIST is null || GA_PSI_LIST.Count == 0)
//         {
//             _logger.LogDebug("GA_PSI_Combiner called with no items; nothing to combine.");
//             return;
//         }

//         List<CombinedAnalyticsMessage> rawRecords = new();
//         foreach (var tuple in GA_PSI_LIST)
//         {
//             _logger.LogInformation(
//                 $"Combing GA + PSI for {tuple.PSIRecord.Page} on {tuple.PSIRecord.Date}"
//             );
//             rawRecords.Add(
//                 new CombinedAnalyticsMessage()
//                 {
//                     Date = tuple.GARecord.Date,
//                     Page = tuple.GARecord.Page,
//                     Sessions = tuple.GARecord.Sessions,
//                     Users = tuple.GARecord.Users,
//                     Views = tuple.GARecord.Views,
//                     PerformanceScore = tuple.PSIRecord.PerformanceScore,
//                     LCP_ms = tuple.PSIRecord.LCP_MS,
//                 }
//             );
//         }
//         await _producer.PublishRawRecord(rawRecords);
//     }
// }
