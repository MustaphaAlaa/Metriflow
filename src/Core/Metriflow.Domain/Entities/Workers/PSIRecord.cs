using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Interfaces;

namespace Metriflow.Domain.Entities.Workers;

[AnalyticRecord(enTypesKey.PSI)]
public class PSIRecord : IAnalyticRecord
{
    [Key] public Guid Id { get; set; }
    public long Ticks { get; set; }
    public int PageId { get; set; }
    public int PerformanceScore { get; set; }
    public long LCP_MS { get; set; }

    [NotMapped]
    public DateTime Date => new DateTime(Ticks, DateTimeKind.Utc);

    public override string ToString()
    {
        return
            $"Ticks: {this.Ticks}, PageId: {this.PageId}, PerformanceScore: {this.PerformanceScore}, LCP: {this.LCP_MS}ms";
    }
}