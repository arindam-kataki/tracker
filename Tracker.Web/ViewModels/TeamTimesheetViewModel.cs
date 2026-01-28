// Add these classes to TimesheetViewModels.cs:

using Tracker.Web.Entities;

/// <summary>
/// Group of work items within a service area
/// </summary>
public class ServiceAreaGroupViewModel
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
    public int EntryCount { get; set; }
    
    public List<WorkItemSummaryViewModel> WorkItems { get; set; } = new();
}


/// <summary>
/// Summary of hours for a single work item (enhancement)
/// </summary>
public class WorkItemSummaryViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
    public int EntryCount { get; set; }
    
    // Hours by work phase (for badges)
    public List<WorkPhaseSummaryViewModel> WorkPhaseBreakdown { get; set; } = new();
}

/// <summary>
/// Hours for a single work phase within a work item
/// </summary>
public class WorkPhaseSummaryViewModel
{
    public string WorkPhaseId { get; set; } = string.Empty;
    public string WorkPhaseName { get; set; } = string.Empty;
    public string WorkPhaseCode { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal ContributedHours { get; set; }
}


/// <summary>
/// View model for the Team Timesheet rollup page.
/// Shows aggregated time entries for a manager and all their reportees.
/// </summary>
public class TeamTimesheetViewModel
{
    public string ManagerId { get; set; } = string.Empty;
    public string ManagerName { get; set; } = string.Empty;
    
    // Date Range Filter
    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(6 - (int)DateTime.Today.DayOfWeek);
    
    // Team Members included in rollup
    public List<TeamMemberViewModel> TeamMembers { get; set; } = new();
    
    // Summary Totals
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
    public int EntryCount { get; set; }
    public int TeamMemberCount => TeamMembers.Count;
    
    // Grouped Data for display (by Service Area -> Work Items, aggregated across team)
    public List<ServiceAreaGroupViewModel> ServiceAreaGroups { get; set; } = new();
    
    // Service areas the manager can view
    public List<ServiceArea> ViewableServiceAreas { get; set; } = new();
    
    // Display helpers
    public string DateRangeDisplay => StartDate.Date == EndDate.Date 
        ? StartDate.ToString("MMM d, yyyy")
        : $"{StartDate:MMM d} - {EndDate:MMM d, yyyy}";
}

/// <summary>
/// Team member summary for the Team Timesheet view
/// </summary>
public class TeamMemberViewModel
{
    public string ResourceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
    public int EntryCount { get; set; }
    public bool IsSelf { get; set; } // Is this the manager themselves?
}
