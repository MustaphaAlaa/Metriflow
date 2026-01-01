using System.Collections;
using Metriflow.Domain.Entities;

namespace Metriflow.Application.Interfaces;

public interface IAnalyticRecordsCombiner
{
    bool CanCombine(Dictionary<Type, IList> records);
    IList<CombinedAnalyticsMessage> Combine(Dictionary<Type, IList> records);
}