// using Metriflow.Domain.CustomAttributes;
// using Metriflow.Domain.Entities;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
//
// namespace Metriflow.Application.Services;
//
// [ServiceRegistration(lifetime: ServiceLifetime.Scoped, serviceType: typeof(IAggregationProgressService))]
// public class AggregationProgressService(ILogger<AggregationProgressService> logger) : IAggregationProgressService
// {
//     public IEnumerable<AggregationProgress> RangeAggregatedToInterval(IEnumerable<AggregationProgress> aggregationProgress)
//     {
//         foreach (var aggregation in aggregationProgress)
//             aggregation.Interval = true;
//          
//
//         return aggregationProgress;
//     } 
// }
