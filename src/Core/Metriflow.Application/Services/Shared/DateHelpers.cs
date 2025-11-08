using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Metriflow.Application.Services.Shared;


 public static class DateHelpers
{
    // 1. Check if it's the last day of the month
    public static bool IsLastDayOfMonth(DateTime date)
    {
        return date.Day == DateTime.DaysInMonth(date.Year, date.Month);
    }

    // 2. Check if it's the first day of a new year
    public static bool IsNewYear(DateTime date)
    {
        return date.Month == 1 && date.Day == 1;
    }

    // 3. Check if it's the first day of a new month
    public static bool IsNewMonth(DateTime date)
    {
        return date.Day == 1;
    }
}
