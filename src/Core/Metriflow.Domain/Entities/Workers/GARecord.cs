using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Metriflow.Domain.CustomAttributes;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.Interfaces;

namespace Metriflow.Domain.Entities.Workers;

[AnalyticRecord( enTypesKey.GA)]
public class GARecord : IAnalyticRecord
{
    [Key]
    public Guid Id { get; set; }
    public long Ticks { get; set; }
    public int Page { get; set; }
    public long Users { get; set; }
    public long Views { get; set; }
    public long Sessions { get; set; }

    public override string ToString()
    {
        return $"Ticks: {this.Ticks}, Page: {this.Page}, Users: {this.Users},Views: {this.Views}, Sessions: {this.Sessions}";
    }
}
