using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Metriflow.Domain.interfaces;

namespace Metriflow.Domain.Entities;

public class DailyAnalytics : AggregateAnalytics
{
    public Guid Id { get; set; }

    // Unique Key (Business) - Needs Fluent API mapping for composite key
    public DateTime Date { get; set; }
    public DateTime ReceivedAt { get; set; }

}
