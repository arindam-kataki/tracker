namespace Tracker.Web.Entities;

/// <summary>
/// Unified lookup table for work phases used across:
/// - Estimation breakdown (estimating hours per phase)
/// - Time recording (logging actual hours per phase)
/// - Consolidation (billing hours per phase)
/// </summary>
public class WorkPhase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name of the phase (e.g., "Requirements & Estimation")
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Short code for programmatic use (e.g., "REQ", "DEV", "TEST")
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of what this phase covers
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Default contribution percentage for timesheet → consolidation (0-100)
    /// 100 = fully billable, 50 = half billable, 0 = not billable
    /// </summary>
    public int DefaultContributionPercent { get; set; } = 100;
    
    /// <summary>
    /// Display order in lists and forms
    /// </summary>
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Whether this phase is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Show this phase in estimation breakdown
    /// </summary>
    public bool ForEstimation { get; set; } = true;
    
    /// <summary>
    /// Show this phase in timesheet entry
    /// </summary>
    public bool ForTimeRecording { get; set; } = true;
    
    /// <summary>
    /// Show this phase in consolidation
    /// </summary>
    public bool ForConsolidation { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual ICollection<EstimationBreakdownItem> EstimationItems { get; set; } = new List<EstimationBreakdownItem>();
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
