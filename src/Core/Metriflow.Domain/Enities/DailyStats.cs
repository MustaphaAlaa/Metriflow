using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Metriflow.Domain.interfaces;

namespace Metriflow.Domain.Entities;

public class DailyStat : IPageStats
{
    public Guid Id { get; set; }

    // Unique Key (Business) - Needs Fluent API mapping for composite key
    public DateOnly Date { get; set; }

    [ForeignKey("Key")]
    public int PageId { get; set; }

    public int TotalUsers { get; set; }

    public int TotalSessions { get; set; }
    public int TotalViews { get; set; }
    public double AvgPerformance { get; set; }
    public DateTime ReceivedAt { get; set; }
    public Page Page { get; set; }
}
