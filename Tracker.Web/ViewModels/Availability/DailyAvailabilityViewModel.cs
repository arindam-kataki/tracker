namespace Tracker.Web.ViewModels.Availability;

/// <summary>
/// Represents availability information for a single day
/// </summary>
public class DailyAvailabilityViewModel
{
    public DateOnly Date { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    
    /// <summary>
    /// Standard scheduled hours for this day (from resource schedule)
    /// </summary>
    public decimal ScheduledHours { get; set; }
    
    /// <summary>
    /// Hours unavailable due to PTO, holidays, etc.
    /// </summary>
    public decimal UnavailableHours { get; set; }
    
    /// <summary>
    /// Actual available hours (Scheduled - Unavailable)
    /// </summary>
    public decimal AvailableHours { get; set; }
    
    /// <summary>
    /// Hours already allocated to projects/work
    /// </summary>
    public decimal AllocatedHours { get; set; }
    
    /// <summary>
    /// Remaining unallocated hours (Available - Allocated)
    /// </summary>
    public decimal RemainingHours => AvailableHours - AllocatedHours;
    
    /// <summary>
    /// Whether this is a weekend day
    /// </summary>
    public bool IsWeekend { get; set; }
    
    /// <summary>
    /// Whether this is a company holiday
    /// </summary>
    public bool IsHoliday { get; set; }
    
    /// <summary>
    /// Name of the holiday (if applicable)
    /// </summary>
    public string? HolidayName { get; set; }
    
    /// <summary>
    /// Whether the resource has PTO on this day
    /// </summary>
    public bool HasPto { get; set; }
    
    /// <summary>
    /// List of reasons for unavailability
    /// </summary>
    public List<string> UnavailabilityReasons { get; set; } = new();
    
    /// <summary>
    /// CSS class for calendar display
    /// </summary>
    public string CssClass
    {
        get
        {
            if (IsWeekend) return "weekend";
            if (IsHoliday) return "holiday";
            if (HasPto) return "pto";
            if (AvailableHours == 0) return "unavailable";
            if (RemainingHours <= 0) return "fully-allocated";
            if (RemainingHours < AvailableHours * 0.25m) return "nearly-full";
            return "available";
        }
    }
    
    /// <summary>
    /// Display text for the day
    /// </summary>
    public string DisplayText
    {
        get
        {
            if (IsHoliday) return HolidayName ?? "Holiday";
            if (HasPto) return "PTO";
            if (IsWeekend) return "";
            if (AvailableHours == 0) return "Off";
            return $"{AvailableHours}h";
        }
    }
}
