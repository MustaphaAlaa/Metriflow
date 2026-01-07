using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Metriflow.Domain.Entities.Enums;
using Metriflow.Domain.interfaces;

namespace Metriflow.Domain.Entities;

public class PageAnalytics
{
    [Key] public Guid Id { get; set; }
    [ForeignKey("Page")] public int PageId { get; set; }
    public DateTime Date { get; set; }
    [ForeignKey("TimeInterval")] public enTimeIntervals Intervals { get; set; }
    public long Users { get; set; }
    public long Sessions { get; set; }
    public long Views { get; set; }
    public double PerformanceScore { get; set; }
    public long LCP_ms { get; set; }

    public Page Page { get; set; }
    public TimeInterval TimeInterval { get; set; }
}