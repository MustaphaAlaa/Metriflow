using Metriflow.Domain.Entities.Workers;

namespace Metriflow.Domain.Entities;


public class AggregateRecordsJoins
{
    public DateTime Date { get; set; }
    public int PageId { get; set; }
    public GARecord GARecord { get; set; }
    public PSARecord PsaRecord { get; set; }
    public AggregationProgress AggregationProgress { get; set; }
}