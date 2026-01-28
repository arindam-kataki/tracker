namespace Tracker.Web.Extensions;

/// <summary>
/// Extension methods for converting between DateTime and DateOnly.
/// Helps maintain compatibility when mixing DateTime-based APIs with DateOnly entities.
/// </summary>
public static class DateExtensions
{
    /// <summary>
    /// Convert DateTime to DateOnly (just the date part, no timezone issues)
    /// </summary>
    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }
    
    /// <summary>
    /// Convert nullable DateTime to nullable DateOnly
    /// </summary>
    public static DateOnly? ToDateOnly(this DateTime? dateTime)
    {
        return dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
    }
    
    /// <summary>
    /// Convert DateOnly to DateTime (at midnight, local time)
    /// </summary>
    public static DateTime ToDateTime(this DateOnly dateOnly)
    {
        return dateOnly.ToDateTime(TimeOnly.MinValue);
    }
    
    /// <summary>
    /// Convert nullable DateOnly to nullable DateTime
    /// </summary>
    public static DateTime? ToDateTime(this DateOnly? dateOnly)
    {
        return dateOnly?.ToDateTime(TimeOnly.MinValue);
    }
    
    /// <summary>
    /// Get the first day of the month for a DateTime
    /// </summary>
    public static DateTime FirstDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, 1);
    }
    
    /// <summary>
    /// Get the last day of the month for a DateTime
    /// </summary>
    public static DateTime LastDayOfMonth(this DateTime dateTime)
    {
        return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
    }
    
    /// <summary>
    /// Get the first day of the month for a DateOnly
    /// </summary>
    public static DateOnly FirstDayOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, 1);
    }
    
    /// <summary>
    /// Get the last day of the month for a DateOnly
    /// </summary>
    public static DateOnly LastDayOfMonth(this DateOnly date)
    {
        return new DateOnly(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }
}
