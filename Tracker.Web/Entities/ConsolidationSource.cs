namespace Tracker.Web.Entities;

/// <summary>
/// Links a consolidation to its source timesheet entries,
/// tracking how many hours were pulled from each entry.
/// </summary>
public class ConsolidationSource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// The consolidation this source belongs to
    /// </summary>
    public string ConsolidationId { get; set; } = string.Empty;
    
    /// <summary>
    /// The source timesheet entry
    /// </summary>
    public string TimeEntryId { get; set; } = string.Empty;
    
    /// <summary>
    /// Hours pulled from this entry into the consolidation.
    /// Can be less than or equal to TimeEntry.ContributedHours.
    /// </summary>
    public decimal PulledHours { get; set; }
    
    // Navigation
    public virtual Consolidation Consolidation { get; set; } = null!;
    public virtual TimeEntry TimeEntry { get; set; } = null!;
}
