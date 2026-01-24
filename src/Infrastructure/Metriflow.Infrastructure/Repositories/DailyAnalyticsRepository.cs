// using IRepository.Generic;
// using Metriflow.Domain.CustomAttributes;
// using Metriflow.Domain.Entities;
// using Metriflow.Domain.Entities.Reports;
// using Metriflow.Infrastructure;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
//
// namespace Repositories.Generic;
//
//
// [ServiceRegistration(ServiceLifetime.Scoped, typeof(IDailyAnalyticsRepository))]
// public class DailyAnalyticsRepository : BaseRepository<DailyAnalytics>, IDailyAnalyticsRepository
// {
//     protected readonly MetriflowDbContext _db;
//
//     public DailyAnalyticsRepository(MetriflowDbContext context)
//         : base(context)
//     {
//         _db = context;
//     }
//
//     public async Task<List<PageReport>> PageReportAsync()
//     {
//         var pageReports = await _db
//             .PageAnalytics.Include(r => r.PageId)
//             .GroupBy(r => new { r.PageId, r.PageId.Path })
//             .Select(g => new PageReport
//             {
//                 Path = g.Key.Path,
//                 TotalUsers = g.Sum(x => x.Users),
//                 TotalSessions = g.Sum(x => x.Sessions),
//                 TotalViews = g.Sum(x => x.Views),
//                 AvgPerformance = g.Average(x => x.PerformanceScore),
//             })
//             .ToListAsync();
//
//         return pageReports;
//     }
//
//     public Task<OverviewReport> StatsOverviewAsync()
//     {
//         var overview = _db
//             .PageAnalytics.GroupBy(r => 1)
//             .Select(g => new OverviewReport
//             {
//                 TotalUsers = g.Sum(x => x.Users),
//                 TotalSessions = g.Sum(x => x.Sessions),
//                 TotalViews = g.Sum(x => x.Views),
//                 AvgPerformance = g.Average(x => x.PerformanceScore),
//             })
//             .FirstOrDefaultAsync();
//
//         return overview;
//     }
// }
//
//
