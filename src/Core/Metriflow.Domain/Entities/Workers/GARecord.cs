using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO.Hashing;
using System.Security;
using System.Text;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Interfaces;

namespace Metriflow.Domain.Entities.Workers;

[AnalyticRecord(enTypesKey.GA)]
public class GARecord : IAnalyticRecord
{
    // [Key]
    // public Guid Id { get; set; }
    public long Ticks { get; set; }
    public int PageId { get; set; }
    public long Users { get; set; }
    public long Views { get; set; }
    public long Sessions { get; set; }

    [NotMapped]
    public DateTime Date => new DateTime(Ticks, DateTimeKind.Utc);

    public bool IsCorrelation { get; set; } = false;

    public override string ToString()
    {
        return $"Ticks: {this.Ticks}, PageId: {this.PageId}, Users: {this.Users},Views: {this.Views}, Sessions: {this.Sessions}";
    }

    private Guid? _hashGuid = null;

    public Guid Hash { get; set; }

    public Guid ComputeHash()
    {
        var raw = $"{Ticks}|{PageId}|{Users}|{Views}|{Sessions}";

        var bytes = Encoding.UTF8.GetBytes(raw);

        var hash = XxHash128.Hash(bytes);
        var guid = new Guid(hash);
        return guid;
    }
}
