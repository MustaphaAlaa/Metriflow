using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metriflow.Domain;

public class RawRecord
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public string Page { get; set; }
    public long Users { get; set; }
    public long Views { get; set; }
    public long Sessions { get; set; }
    public int PerformanceScore { get; set; }
    public long LCP_MS { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public override string ToString()
    {
        return $" --- {this.Date} || {this.Page} --- ";
    }
}
