using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

/// <summary>
/// Main view model for the Manager Timesheet page.
/// Displays a hierarchy tree on the left and calendar + work items on the right.
/// </summary>
public class ManagerTimesheetViewModel
{
    /// <summary>
    /// The current logged-in manager's resource ID
    /// </summary>
    public string ManagerId { get; set; } = string.Empty;
    
    /// <summary>
    /// The current logged-in manager's name
    /// </summary>
    public string ManagerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Currently selected year for the calendar
    /// </summary>
    public int Year { get; set; } = DateTime.Today.Year;
    
    /// <summary>
    /// Currently selected month for the calendar (1-12)
    /// </summary>
    public int Month { get; set; } = DateTime.Today.Month;
    
    /// <summary>
    /// Display name for the current month/year
    /// </summary>
    public string MonthYearDisplay => new DateTime(Year, Month, 1).ToString("MMMM yyyy");
    
    /// <summary>
    /// The hierarchy tree of resources under this manager
    /// </summary>
    public List<HierarchyNodeViewModel> HierarchyTree { get; set; } = new();
    
    /// <summary>
    /// Currently selected node ID (resource ID or rollup node ID)
    /// </summary>
    public string? SelectedNodeId { get; set; }
    
    /// <summary>
    /// Whether the selected node is a rollup (true) or individual (false)
    /// </summary>
    public bool IsRollupSelected { get; set; }
    
    /// <summary>
    /// Display name for the selected node
    /// </summary>
    public string SelectedNodeName { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of resources included in the current selection
    /// </summary>
    public int SelectedResourceCount { get; set; }
    
    /// <summary>
    /// Calendar data - hours per day for the selected month
    /// </summary>
    public List<CalendarDayViewModel> CalendarDays { get; set; } = new();
    
    /// <summary>
    /// Work items grouped by service area
    /// </summary>
    public List<ManagerServiceAreaGroupViewModel> ServiceAreaGroups { get; set; } = new();
    
    /// <summary>
    /// Total hours for the selected scope and month
    /// </summary>
    public decimal TotalHours { get; set; }
    
    /// <summary>
    /// Total contributed hours for the selected scope and month
    /// </summary>
    public decimal TotalContributedHours { get; set; }
    
    /// <summary>
    /// Service areas the manager can view (for reference)
    /// </summary>
    public List<ServiceArea> ViewableServiceAreas { get; set; } = new();
}

/// <summary>
/// Represents a node in the hierarchy tree.
/// Can be either a rollup node (aggregates subordinates) or an individual resource.
/// </summary>
public class HierarchyNodeViewModel
{
    /// <summary>
    /// Unique ID for this node. 
    /// For rollup nodes: "rollup_{resourceId}"
    /// For individual nodes: "{resourceId}"
    /// </summary>
    public string NodeId { get; set; } = string.Empty;
    
    /// <summary>
    /// The resource ID this node represents
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name for this node
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is a rollup node (true) or individual (false)
    /// </summary>
    public bool IsRollup { get; set; }
    
    /// <summary>
    /// Whether this is the current user (manager viewing the page)
    /// </summary>
    public bool IsSelf { get; set; }
    
    /// <summary>
    /// Total hours for this node (if rollup, includes all subordinates)
    /// </summary>
    public decimal TotalHours { get; set; }
    
    /// <summary>
    /// Number of resources included (1 for individual, N for rollup)
    /// </summary>
    public int ResourceCount { get; set; } = 1;
    
    /// <summary>
    /// Child nodes (for rollup nodes, contains the individual + subordinate rollups)
    /// </summary>
    public List<HierarchyNodeViewModel> Children { get; set; } = new();
    
    /// <summary>
    /// Indentation level for display (0 = root)
    /// </summary>
    public int Level { get; set; }
}

/// <summary>
/// Calendar day data for the monthly view
/// </summary>
public class CalendarDayViewModel
{
    /// <summary>
    /// The date
    /// </summary>
    public DateOnly Date { get; set; }
    
    /// <summary>
    /// Total hours logged on this day
    /// </summary>
    public decimal Hours { get; set; }
    
    /// <summary>
    /// Total contributed hours on this day
    /// </summary>
    public decimal ContributedHours { get; set; }
    
    /// <summary>
    /// Whether this day is in the current month
    /// </summary>
    public bool IsCurrentMonth { get; set; } = true;
    
    /// <summary>
    /// Whether this is today
    /// </summary>
    public bool IsToday => Date == DateOnly.FromDateTime(DateTime.Today);
    
    /// <summary>
    /// Whether this is a weekend
    /// </summary>
    public bool IsWeekend => Date.DayOfWeek == DayOfWeek.Saturday || Date.DayOfWeek == DayOfWeek.Sunday;
    
    /// <summary>
    /// Hours breakdown by work phase for this day
    /// </summary>
    public List<WorkPhaseHoursViewModel> WorkPhaseBreakdown { get; set; } = new();
    
    /// <summary>
    /// Whether this day has hours logged
    /// </summary>
    public bool HasHours => Hours > 0;
}

/// <summary>
/// Hours for a specific work phase
/// </summary>
public class WorkPhaseHoursViewModel
{
    public string WorkPhaseId { get; set; } = string.Empty;
    public string WorkPhaseCode { get; set; } = string.Empty;
    public string WorkPhaseName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal ContributedHours { get; set; }
    
    /// <summary>
    /// CSS class for badge color based on work phase code
    /// </summary>
    public string BadgeClass => WorkPhaseCode.ToUpperInvariant() switch
    {
        "REQ" => "bg-info",
        "DESIGN" => "bg-primary",
        "DEV" => "bg-success",
        "TEST" or "STI" or "UAT" => "bg-warning text-dark",
        "DEPLOY" or "GOLIVE" => "bg-danger",
        "SUPPORT" or "HYPERCARE" => "bg-secondary",
        "DOC" => "bg-light text-dark",
        "PM" or "LEAD" => "bg-purple",
        _ => "bg-secondary"
    };
}

/// <summary>
/// Service area group containing work items
/// </summary>
public class ManagerServiceAreaGroupViewModel
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
    
    /// <summary>
    /// Work items in this service area
    /// </summary>
    public List<ManagerWorkItemViewModel> WorkItems { get; set; } = new();
}

/// <summary>
/// Work item (enhancement) with resource breakdown
/// </summary>
public class ManagerWorkItemViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
    
    /// <summary>
    /// Resources who logged time on this work item
    /// </summary>
    public List<ResourceHoursViewModel> ResourceBreakdown { get; set; } = new();
    
    /// <summary>
    /// Whether multiple resources worked on this item
    /// </summary>
    public bool HasMultipleResources => ResourceBreakdown.Count > 1;
}

/// <summary>
/// Hours logged by a specific resource
/// </summary>
public class ResourceHoursViewModel
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal ContributedHours { get; set; }
    
    /// <summary>
    /// Hours breakdown by work phase for this resource on this work item
    /// </summary>
    public List<WorkPhaseHoursViewModel> WorkPhaseBreakdown { get; set; } = new();
}
