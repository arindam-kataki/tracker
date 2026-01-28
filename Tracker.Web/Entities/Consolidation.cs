namespace Tracker.Web.Entities;

/// <summary>
/// Represents a billing consolidation for an enhancement for a specific period.
/// Can be derived from timesheet entries or entered manually.
/// </summary>
public class Consolidation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// The enhancement this consolidation is for
    /// </summary>
    public string EnhancementId { get; set; } = string.Empty;
    
    /// <summary>
    /// Service area (denormalized for easier querying/reporting)
    /// </summary>
    public string ServiceAreaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Start date of the billing period
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// End date of the billing period (must be in same month as StartDate)
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Final billable hours for this period
    /// </summary>
    public decimal BillableHours { get; set; }
    
    /// <summary>
    /// Sum of hours from linked TimeEntry sources (0 if manual entry)
    /// </summary>
    public decimal SourceHours { get; set; }
    
    /// <summary>
    /// Status of this consolidation
    /// </summary>
    public ConsolidationStatus Status { get; set; } = ConsolidationStatus.Draft;
    
    /// <summary>
    /// Notes (required if no source entries)
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// Future: Link to invoice when invoicing is implemented
    /// </summary>
    public string? InvoiceId { get; set; }
    
    /// <summary>
    /// User who created this consolidation
    /// </summary>
    public string? CreatedById { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// User who last modified this consolidation
    /// </summary>
    public string? ModifiedById { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual ServiceArea ServiceArea { get; set; } = null!;
    public virtual Resource? CreatedBy { get; set; }
    public virtual Resource? ModifiedBy { get; set; }
    
    /// <summary>
    /// Source timesheet entries that contributed to this consolidation
    /// </summary>
    public virtual ICollection<ConsolidationSource> Sources { get; set; } = new List<ConsolidationSource>();
    
    /// <summary>
    /// Whether this is a manual entry (no source entries)
    /// </summary>
    public bool IsManual => !Sources.Any();
    
    /// <summary>
    /// Period display (e.g., "Jan 2025")
    /// </summary>
    public string PeriodDisplay => StartDate.ToString("MMM yyyy");
    
    /// <summary>
    /// Date range display (e.g., "Jan 1-31, 2025")
    /// </summary>
    public string DateRangeDisplay => StartDate == EndDate 
        ? StartDate.ToString("MMM d, yyyy")
        : $"{StartDate:MMM d} - {EndDate:d}, {EndDate:yyyy}";
}

/// <summary>
/// Status of a consolidation record
/// </summary>
public enum ConsolidationStatus
{
    /// <summary>
    /// Draft - can be edited or deleted
    /// </summary>
    Draft = 0,
    
    /// <summary>
    /// Finalized - locked, ready for invoicing
    /// </summary>
    Finalized = 1,
    
    /// <summary>
    /// Invoiced - linked to an invoice (future use)
    /// </summary>
    Invoiced = 2
}
