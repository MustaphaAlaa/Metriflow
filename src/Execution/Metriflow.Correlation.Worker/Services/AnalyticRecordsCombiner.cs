using System.Collections;
using Metriflow.Domain.Entities.Workers;
using Metriflow.DTOs;

namespace Metriflow.Correlation.Worker;

public static class AnalyticRecordsCombiner
{
    public static bool CanCombine(Dictionary<Type, IList> records)
    {
        return records.ContainsKey(typeof(GARecord)) && records.ContainsKey(typeof(PSIRecord));
    }

    public static IEnumerable<CombinedAnalyticsMessage> Combine(Dictionary<Type, IList> records)
    {
        var gaRecords = records[typeof(GARecord)].Cast<GARecord>();
        var psiRecords = records[typeof(PSIRecord)].Cast<PSIRecord>();

        var result = gaRecords.Join(
            psiRecords,
            ga => (ga.Date, ga.Page),
            psi => (psi.Date, psi.Page),
            (ga, psi) =>
                new CombinedAnalyticsMessage
                {
                    Date = ga.Date,
                    Page = ga.Page,
                    Sessions = ga.Sessions,
                    Users = ga.Users,
                    Views = ga.Views,
                    PerformanceScore = psi.PerformanceScore,
                    LCP_ms = psi.LCP_MS,
                }
        );
        return result;
    }
}
