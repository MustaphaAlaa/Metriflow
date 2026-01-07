using System.Collections;
using Metriflow.Application.Interfaces;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities;
using Metriflow.Domain.Entities.Workers;
using Microsoft.Extensions.DependencyInjection;

namespace Metriflow.Application.Worker;

[ServiceRegistration(ServiceLifetime.Scoped, typeof(IAnalyticRecordsCombiner))]
public class AnalyticRecordsCombiner : IAnalyticRecordsCombiner
{
    public bool CanCombine(Dictionary<Type, IList> records)
    {
        return records.ContainsKey(typeof(GARecord)) && records.ContainsKey(typeof(PSIRecord));
    }

    public IList<CombinedAnalyticsMessage> Combine(Dictionary<Type, IList> records)
    {
        var gaRecords = records[typeof(GARecord)].Cast<GARecord>();
        var psiRecords = records[typeof(PSIRecord)].Cast<PSIRecord>();

        var result = gaRecords
            .Join(
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
            )
            .ToList();
        return result;
    }
}
