using Metriflow.Domain.Entities;
using Metriflow.Domain.Interfaces;

namespace IRepository.Generic;

public interface IAggregationProgressRepository : IBaseRepository<AggregationProgress>
{
    void DailyAggregated(AggregationProgress aggregationProgress);
    void IntervalAggregated(AggregationProgress aggregationProgress);
    void MonthlyAggregated(AggregationProgress aggregationProgress);
    void QuarterlyAggregated(AggregationProgress aggregationProgress);

    void YearlyAggregated(AggregationProgress aggregationProgress);
    void CorrelationAggregated(AggregationProgress aggregationProgress);

    Task<List<AggregationKey>> GetUnprocessedKeysAsync();
    IQueryable<AggregateRecordsJoins> GetNoneMonthlyAggregateRecords();
    IQueryable<AggregationProgress> GetNoneIntervalsAggregateRecords();
    IQueryable<AggregateRecordsJoins> GetNoneDailyAggregateRecords();
    IQueryable<AggregateRecordsJoins> GetNoneYearlyAggregateRecords();
    IQueryable<AggregateRecordsJoins> GetNoneQueryableAggregateRecords();
    IQueryable<AggregateRecordsJoins> GetNoneCorrelationAggregateRecords();
    Task CreateRangeWithKeysAsync(IEnumerable<AggregationKey> keys);
    Task<int> InsertMissingAggregationProgressesAsync();
}

