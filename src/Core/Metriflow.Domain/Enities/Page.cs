using Metriflow.Domain.Entities;

namespace Metriflow.Domain.Entities;

public class Page
{
    public int Id { get; set; }

    // The actual URL path
    public string Path { get; set; }

    public ICollection<DailyStat> DailyStatsRecords { get; set; }
    public ICollection<MonthlyStat> MonthlyStatsRecords { get; set; }
    public ICollection<YearlyStat> YearlyStatsRecords { get; set; }
}
