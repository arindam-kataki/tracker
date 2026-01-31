namespace Tracker.Web.ViewModels.Availability;

/// <summary>
/// View model for team capacity planning view
/// </summary>
public class TeamCapacityViewModel
{
    public string? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public string? ServiceAreaId { get; set; }
    public string? ServiceAreaName { get; set; }
    
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    
    /// <summary>
    /// Capacity data for each team member
    /// </summary>
    public List<ResourceCapacityViewModel> Resources { get; set; } = new();
    
    /// <summary>
    /// Team totals
    /// </summary>
    public TeamCapacitySummaryViewModel Summary { get; set; } = new();
    
    // Navigation
    public DateOnly PreviousWeekStart => StartDate.AddDays(-7);
    public DateOnly NextWeekStart => StartDate.AddDays(7);
    
    public string PeriodDisplay => $"{StartDate:MMM d} - {EndDate:MMM d, yyyy}";
}

/// <summary>
/// Capacity information for a single resource
/// </summary>
public class ResourceCapacityViewModel
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? ServiceAreaCode { get; set; }
    
    /// <summary>
    /// Total available hours in the period (after PTO, holidays)
    /// </summary>
    public decimal AvailableHours { get; set; }
    
    /// <summary>
    /// Hours already allocated to work
    /// </summary>
    public decimal AllocatedHours { get; set; }
    
    /// <summary>
    /// Remaining capacity (Available - Allocated)
    /// </summary>
    public decimal RemainingHours => AvailableHours - AllocatedHours;
    
    /// <summary>
    /// Allocation percentage
    /// </summary>
    public decimal AllocationPercent => AvailableHours > 0
        ? Math.Round(AllocatedHours / AvailableHours * 100, 1)
        : 0;
    
    /// <summary>
    /// Whether the resource is over-allocated
    /// </summary>
    public bool IsOverAllocated => AllocatedHours > AvailableHours;
    
    /// <summary>
    /// Whether the resource is on PTO for the entire period
    /// </summary>
    public bool IsOnPto { get; set; }
    
    /// <summary>
    /// Reason for unavailability (if fully unavailable)
    /// </summary>
    public string? UnavailabilityReason { get; set; }
    
    /// <summary>
    /// Status indicator for UI
    /// </summary>
    public CapacityStatus Status
    {
        get
        {
            if (IsOnPto || AvailableHours == 0) return CapacityStatus.Unavailable;
            if (IsOverAllocated) return CapacityStatus.OverAllocated;
            if (AllocationPercent >= 90) return CapacityStatus.NearlyFull;
            if (AllocationPercent >= 50) return CapacityStatus.PartiallyAllocated;
            return CapacityStatus.Available;
        }
    }
    
    /// <summary>
    /// CSS class for status display
    /// </summary>
    public string StatusCssClass => Status switch
    {
        CapacityStatus.Available => "text-success",
        CapacityStatus.PartiallyAllocated => "text-primary",
        CapacityStatus.NearlyFull => "text-warning",
        CapacityStatus.OverAllocated => "text-danger",
        CapacityStatus.Unavailable => "text-muted",
        _ => ""
    };
    
    /// <summary>
    /// Status badge class
    /// </summary>
    public string StatusBadgeClass => Status switch
    {
        CapacityStatus.Available => "bg-success",
        CapacityStatus.PartiallyAllocated => "bg-primary",
        CapacityStatus.NearlyFull => "bg-warning text-dark",
        CapacityStatus.OverAllocated => "bg-danger",
        CapacityStatus.Unavailable => "bg-secondary",
        _ => "bg-secondary"
    };
    
    /// <summary>
    /// Status display text
    /// </summary>
    public string StatusDisplay => Status switch
    {
        CapacityStatus.Available => "Available",
        CapacityStatus.PartiallyAllocated => "Allocated",
        CapacityStatus.NearlyFull => "Nearly Full",
        CapacityStatus.OverAllocated => "Over-Allocated",
        CapacityStatus.Unavailable => UnavailabilityReason ?? "Unavailable",
        _ => ""
    };
}

/// <summary>
/// Status levels for capacity
/// </summary>
public enum CapacityStatus
{
    Available,
    PartiallyAllocated,
    NearlyFull,
    OverAllocated,
    Unavailable
}

/// <summary>
/// Summary totals for team capacity
/// </summary>
public class TeamCapacitySummaryViewModel
{
    /// <summary>
    /// Total available hours across all team members
    /// </summary>
    public decimal TotalAvailableHours { get; set; }
    
    /// <summary>
    /// Total allocated hours across all team members
    /// </summary>
    public decimal TotalAllocatedHours { get; set; }
    
    /// <summary>
    /// Total remaining capacity
    /// </summary>
    public decimal TotalRemainingHours => TotalAvailableHours - TotalAllocatedHours;
    
    /// <summary>
    /// Team allocation percentage
    /// </summary>
    public decimal TeamAllocationPercent => TotalAvailableHours > 0
        ? Math.Round(TotalAllocatedHours / TotalAvailableHours * 100, 1)
        : 0;
    
    /// <summary>
    /// Number of resources available
    /// </summary>
    public int ResourcesAvailable { get; set; }
    
    /// <summary>
    /// Number of resources on PTO or unavailable
    /// </summary>
    public int ResourcesUnavailable { get; set; }
    
    /// <summary>
    /// Number of over-allocated resources
    /// </summary>
    public int ResourcesOverAllocated { get; set; }
    
    /// <summary>
    /// Total team size
    /// </summary>
    public int TotalResources => ResourcesAvailable + ResourcesUnavailable;
}
