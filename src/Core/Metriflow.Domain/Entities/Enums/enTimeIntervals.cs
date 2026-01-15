using System.ComponentModel;

namespace Metriflow.Domain.Entities.Enums;

/// <summary>
/// Represents fixed 4-hour time intervals within a single day.
/// Each interval is defined using both 12-hour and 24-hour time formats.
/// </summary>
public enum enTimeIntervals : byte
{
    /// <summary>
    /// 12-hour format: 12:00 AM – 3:59 AM  
    /// 24-hour format: 00:00 – 03:59
    /// </summary>
    [Description("12-hour: 12:00 AM – 3:59 AM | 24-hour: 00:00 – 03:59")]
    First = 1,

    /// <summary>
    /// 12-hour format: 4:00 AM – 7:59 AM  
    /// 24-hour format: 04:00 – 07:59
    /// </summary>
    [Description("12-hour: 4:00 AM – 7:59 AM | 24-hour: 04:00 – 07:59")]
    Second,

    /// <summary>
    /// 12-hour format: 8:00 AM – 11:59 AM  
    /// 24-hour format: 08:00 – 11:59
    /// </summary>
    [Description("12-hour: 8:00 AM – 11:59 AM | 24-hour: 08:00 – 11:59")]
    Third,

    /// <summary>
    /// 12-hour format: 12:00 PM – 3:59 PM  
    /// 24-hour format: 12:00 – 15:59
    /// </summary>
    [Description("12-hour: 12:00 PM – 3:59 PM | 24-hour: 12:00 – 15:59")]
    Fourth,

    /// <summary>
    /// 12-hour format: 4:00 PM – 7:59 PM  
    /// 24-hour format: 16:00 – 19:59
    /// </summary>
    [Description("12-hour: 4:00 PM – 7:59 PM | 24-hour: 16:00 – 19:59")]
    Fifth,

    /// <summary>
    /// 12-hour format: 8:00 PM – 11:59 PM  
    /// 24-hour format: 20:00 – 23:59
    /// </summary>
    [Description("12-hour: 8:00 PM – 11:59 PM | 24-hour: 20:00 – 23:59")]
    Sixth
}