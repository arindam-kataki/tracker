namespace Tracker.Web.ViewModels;

/// <summary>
/// Tab 6: Time Recording - Monthly summary view by work phase
/// Shows consolidated hours for all resources by month, with rollups by work phase
/// </summary>
public class XTimeRecordingViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    
    /// <summary>
    /// All work phases that have recorded time (for pill badges)
    /// </summary>
    public List<WorkPhaseSummary> WorkPhases { get; set; } = new();
    
    /// <summary>
    /// Monthly summaries (rows) - ordered by date descending (most recent first)
    /// </summary>
    public List<MonthlyTimeSummary> MonthlySummaries { get; set; } = new();
    
    /// <summary>
    /// Grand totals by work phase ID (for footer row)
    /// </summary>
    public Dictionary<string, decimal> GrandTotalsByPhase { get; set; } = new();
    
    /// <summary>
    /// Overall grand total hours
    /// </summary>
    public decimal GrandTotal { get; set; }
    
    /// <summary>
    /// Overall grand total contributed hours
    /// </summary>
    public decimal GrandTotalContributed { get; set; }
    
    /// <summary>
    /// Whether there are any time entries
    /// </summary>
    public bool HasEntries => MonthlySummaries.Any();
}

/// <summary>
/// Work phase info for pill badges
/// </summary>
public class WorkPhaseSummary
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string BadgeClass { get; set; } = "bg-secondary";
    public int DisplayOrder { get; set; }
}

/// <summary>
/// One row in the monthly summary table
/// </summary>
public class MonthlyTimeSummary
{
    public int Year { get; set; }
    public int Month { get; set; }
    
    /// <summary>
    /// Display string like "Jan 2025"
    /// </summary>
    public string MonthYearDisplay => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    
    /// <summary>
    /// Sort key for ordering (YYYYMM)
    /// </summary>
    public int SortKey => Year * 100 + Month;
    
    /// <summary>
    /// Hours by work phase ID
    /// </summary>
    public Dictionary<string, decimal> HoursByPhase { get; set; } = new();
    
    /// <summary>
    /// Contributed hours by work phase ID
    /// </summary>
    public Dictionary<string, decimal> ContributedByPhase { get; set; } = new();
    
    /// <summary>
    /// Total hours for this month
    /// </summary>
    public decimal TotalHours { get; set; }
    
    /// <summary>
    /// Total contributed hours for this month
    /// </summary>
    public decimal TotalContributed { get; set; }
    
    /// <summary>
    /// Number of unique resources who logged time this month
    /// </summary>
    public int ResourceCount { get; set; }
}
