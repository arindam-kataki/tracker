namespace Tracker.Web.Entities.Enums;

/// <summary>
/// Pattern for recurring availability/schedule entries
/// </summary>
public enum RecurrencePattern
{
    /// <summary>
    /// No recurrence (one-time entry)
    /// </summary>
    None = 0,

    /// <summary>
    /// Repeats every week on the same day(s)
    /// </summary>
    Weekly = 1,

    /// <summary>
    /// Repeats every other week
    /// </summary>
    BiWeekly = 2,

    /// <summary>
    /// Repeats on the same date each month
    /// </summary>
    Monthly = 3,

    /// <summary>
    /// Repeats on the same day each year
    /// </summary>
    Yearly = 4
}

/// <summary>
/// Extension methods for RecurrencePattern
/// </summary>
public static class RecurrencePatternExtensions
{
    public static string ToDisplayString(this RecurrencePattern pattern) => pattern switch
    {
        RecurrencePattern.None => "One-time",
        RecurrencePattern.Weekly => "Weekly",
        RecurrencePattern.BiWeekly => "Bi-Weekly",
        RecurrencePattern.Monthly => "Monthly",
        RecurrencePattern.Yearly => "Yearly",
        _ => pattern.ToString()
    };

    /// <summary>
    /// Get the next occurrence date after the given date
    /// </summary>
    public static DateOnly? GetNextOccurrence(this RecurrencePattern pattern, DateOnly fromDate, DateOnly originalDate)
    {
        if (pattern == RecurrencePattern.None)
            return null;

        var nextDate = originalDate;
        
        while (nextDate <= fromDate)
        {
            nextDate = pattern switch
            {
                RecurrencePattern.Weekly => nextDate.AddDays(7),
                RecurrencePattern.BiWeekly => nextDate.AddDays(14),
                RecurrencePattern.Monthly => nextDate.AddMonths(1),
                RecurrencePattern.Yearly => nextDate.AddYears(1),
                _ => nextDate
            };
        }

        return nextDate;
    }
}
