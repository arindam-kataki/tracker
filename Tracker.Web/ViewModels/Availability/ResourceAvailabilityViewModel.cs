using Tracker.Web.Entities.Enums;

namespace Tracker.Web.ViewModels.Availability;

/// <summary>
/// View model for displaying and editing resource availability entries
/// </summary>
public class ResourceAvailabilityViewModel
{
    public string? Id { get; set; }
    public string ResourceId { get; set; } = string.Empty;
    public string? ResourceName { get; set; }
    
    public AvailabilityType Type { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? HoursPerDay { get; set; }
    
    public bool IsRecurring { get; set; }
    public RecurrencePattern RecurrencePattern { get; set; }
    public DateOnly? RecurrenceEndDate { get; set; }
    
    public string? Notes { get; set; }
    public ApprovalStatus Status { get; set; }
    
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    // === COMPUTED ===
    
    public bool IsNew => string.IsNullOrEmpty(Id);
    
    public string TypeDisplay => Type.ToDisplayString();
    public string TypeBadgeClass => Type.ToBadgeClass();
    public string StatusDisplay => Status.ToDisplayString();
    public string StatusBadgeClass => Status.ToBadgeClass();
    
    public int DayCount => EndDate.DayNumber - StartDate.DayNumber + 1;
    
    public string DateRangeDisplay => StartDate == EndDate
        ? StartDate.ToString("MMM d, yyyy")
        : $"{StartDate:MMM d} - {EndDate:MMM d, yyyy}";
    
    public string HoursDisplay => HoursPerDay.HasValue
        ? $"{HoursPerDay}h/day"
        : "Full day";
}

/// <summary>
/// View model for the resource availability calendar page
/// </summary>
public class ResourceAvailabilityCalendarViewModel
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    
    public int Year { get; set; }
    public int Month { get; set; }
    
    /// <summary>
    /// Daily availability for the month
    /// </summary>
    public List<DailyAvailabilityViewModel> Days { get; set; } = new();
    
    /// <summary>
    /// Availability entries for this month
    /// </summary>
    public List<ResourceAvailabilityViewModel> Entries { get; set; } = new();
    
    /// <summary>
    /// Summary statistics
    /// </summary>
    public AvailabilitySummaryViewModel Summary { get; set; } = new();
    
    // Navigation
    public int PreviousYear => Month == 1 ? Year - 1 : Year;
    public int PreviousMonth => Month == 1 ? 12 : Month - 1;
    public int NextYear => Month == 12 ? Year + 1 : Year;
    public int NextMonth => Month == 12 ? 1 : Month + 1;
    
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
}

/// <summary>
/// Summary statistics for availability
/// </summary>
public class AvailabilitySummaryViewModel
{
    /// <summary>
    /// Total scheduled working hours in the period
    /// </summary>
    public decimal ScheduledHours { get; set; }
    
    /// <summary>
    /// Hours lost to holidays
    /// </summary>
    public decimal HolidayHours { get; set; }
    
    /// <summary>
    /// Hours lost to PTO
    /// </summary>
    public decimal PtoHours { get; set; }
    
    /// <summary>
    /// Hours lost to other unavailability (sick, training, etc.)
    /// </summary>
    public decimal OtherUnavailableHours { get; set; }
    
    /// <summary>
    /// Total unavailable hours
    /// </summary>
    public decimal TotalUnavailableHours => HolidayHours + PtoHours + OtherUnavailableHours;
    
    /// <summary>
    /// Net available hours
    /// </summary>
    public decimal AvailableHours => ScheduledHours - TotalUnavailableHours;
    
    /// <summary>
    /// Availability percentage
    /// </summary>
    public decimal AvailabilityPercent => ScheduledHours > 0
        ? Math.Round(AvailableHours / ScheduledHours * 100, 1)
        : 0;
    
    /// <summary>
    /// Working days in period
    /// </summary>
    public int WorkingDays { get; set; }
    
    /// <summary>
    /// Days with full unavailability
    /// </summary>
    public int DaysOff { get; set; }
}
