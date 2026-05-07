using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Hashing;
using System.Text;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Interfaces;

namespace Metriflow.Domain.Entities.Workers;

[AnalyticRecord(enTypesKey.PSI)]
public class PSIRecord : IAnalyticRecord
{
    // [Key] public Guid Id { get; set; }
    public long Ticks { get; set; }
    public int PageId { get; set; }
    public int PerformanceScore { get; set; }
    public long LCP_MS { get; set; }

    [NotMapped]
    public DateTime Date => new DateTime(Ticks, DateTimeKind.Utc);

    public bool IsCorrelation { get; set; } = false;

    public override string ToString()
    {
        return $"Ticks: {this.Ticks}, PageId: {this.PageId}, PerformanceScore: {this.PerformanceScore}, LCP: {this.LCP_MS}ms";
    }

    private Guid? _hashGuid = null;

    public Guid Hash { get; set; }

    public Guid ComputeHash()
    {
        var raw = $"{Ticks}|{PageId}|{PerformanceScore}|{LCP_MS}";

        var bytes = Encoding.UTF8.GetBytes(raw);

        var hash = XxHash128.Hash(bytes);
        var guid = new Guid(hash);
        return guid;
    }
}
