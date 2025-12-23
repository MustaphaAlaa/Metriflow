using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Metriflow.Domain.interfaces;

namespace Metriflow.Domain.Entities;

public class DailyStat
{
    public Guid Id { get; set; }

    // Unique Key (Business) - Needs Fluent API mapping for composite key
    public DateTime Date { get; set; }


    public long TotalUsers { get; set; }

    public long TotalSessions { get; set; }
    public long TotalViews { get; set; }
    public double AvgPerformance { get; set; }
    public DateTime ReceivedAt { get; set; }

}
