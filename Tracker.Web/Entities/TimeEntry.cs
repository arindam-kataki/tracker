namespace Tracker.Web.Entities;

/// <summary>
/// Represents a timesheet entry logged by a resource against an enhancement.
/// Records actual hours worked, which can later be consolidated for billing.
/// 
/// NOTE: StartDate and EndDate use DateOnly to avoid timezone issues.
/// When users log time for "January 27", it should be January 27 regardless
/// of the user's or server's timezone.
/// </summary>
public class TimeEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// The enhancement this time was logged against
    /// </summary>
    public string EnhancementId { get; set; } = string.Empty;
    
    /// <summary>
    /// The resource who logged this time
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;
    
    /// <summary>
    /// The type of work performed
    /// </summary>
    public string WorkPhaseId { get; set; } = string.Empty;
    
    /// <summary>
    /// Start date of the work period.
    /// Uses DateOnly to avoid timezone issues - a date is just a date.
    /// </summary>
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// End date of the work period (can equal StartDate for single-day entries).
    /// Uses DateOnly to avoid timezone issues.
    /// </summary>
    public DateOnly EndDate { get; set; }
    
    /// <summary>
    /// Actual hours worked during this period
    /// </summary>
    public decimal Hours { get; set; }
    
    /// <summary>
    /// Hours that contribute toward billable work.
    /// Defaults to Hours * (WorkPhase.DefaultContributionPercent / 100)
    /// Can be adjusted by the resource.
    /// </summary>
    public decimal ContributedHours { get; set; }
    
    /// <summary>
    /// Optional notes describing the work performed
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// User ID who created this entry
    /// </summary>
    public string? CreatedById { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User ID who last modified this entry
    /// </summary>
    public string? ModifiedById { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual Resource Resource { get; set; } = null!;
    public virtual WorkPhase WorkPhase { get; set; } = null!;
    public virtual Resource? CreatedBy { get; set; }
    public virtual Resource? ModifiedBy { get; set; }

    // For consolidation tracking
    public virtual ICollection<ConsolidationSource> ConsolidationSources { get; set; } = new List<ConsolidationSource>();
    
    /// <summary>
    /// Total hours from this entry that have been pulled into consolidations
    /// </summary>
    public decimal TotalPulledHours => ConsolidationSources?.Sum(cs => cs.PulledHours) ?? 0;
    
    /// <summary>
    /// Remaining hours available to be pulled into consolidations
    /// </summary>
    public decimal RemainingHours => ContributedHours - TotalPulledHours;
    
    // Helper methods for DateTime compatibility (used in queries/reporting)
    
    /// <summary>
    /// Get StartDate as DateTime (midnight local)
    /// </summary>
    public DateTime StartDateTime => StartDate.ToDateTime(TimeOnly.MinValue);
    
    /// <summary>
    /// Get EndDate as DateTime (midnight local)
    /// </summary>
    public DateTime EndDateTime => EndDate.ToDateTime(TimeOnly.MinValue);

    public decimal AvailableHours { get; set; }
    public bool IsFullyConsolidated => AvailableHours <= 0;
}
