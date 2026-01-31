namespace Tracker.Web.Entities.Billing;

/// <summary>
/// Tracks billing and cost rate history for a resource.
/// Allows rate changes over time with effective dates.
/// </summary>
public class ResourceRate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The resource this rate belongs to
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;
    public virtual Resource? Resource { get; set; }

    /// <summary>
    /// When this rate takes effect
    /// </summary>
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>
    /// When this rate ends (null = still active)
    /// </summary>
    public DateOnly? EffectiveTo { get; set; }

    /// <summary>
    /// Hourly billing rate to clients
    /// </summary>
    public decimal BillRate { get; set; }

    /// <summary>
    /// Internal cost rate (what the resource costs the company)
    /// For employees: typically annual salary / 2080
    /// For contractors: the rate paid to them
    /// </summary>
    public decimal CostRate { get; set; }

    /// <summary>
    /// Currency code (e.g., "USD", "EUR", "INR")
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Optional notes about this rate (e.g., "Annual raise", "Promotion")
    /// </summary>
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedById { get; set; }

    // === COMPUTED PROPERTIES ===

    /// <summary>
    /// Margin per hour (BillRate - CostRate)
    /// </summary>
    public decimal HourlyMargin => BillRate - CostRate;

    /// <summary>
    /// Margin percentage ((BillRate - CostRate) / BillRate * 100)
    /// </summary>
    public decimal MarginPercent => BillRate > 0 
        ? Math.Round((BillRate - CostRate) / BillRate * 100, 2) 
        : 0;

    /// <summary>
    /// Whether this rate is currently active
    /// </summary>
    public bool IsActive
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return EffectiveFrom <= today && (!EffectiveTo.HasValue || EffectiveTo >= today);
        }
    }

    /// <summary>
    /// Check if this rate applies to a specific date
    /// </summary>
    public bool AppliesToDate(DateOnly date)
    {
        return EffectiveFrom <= date && (!EffectiveTo.HasValue || EffectiveTo >= date);
    }

    /// <summary>
    /// Display string for the effective period
    /// </summary>
    public string EffectivePeriodDisplay => EffectiveTo.HasValue
        ? $"{EffectiveFrom:MMM d, yyyy} - {EffectiveTo:MMM d, yyyy}"
        : $"{EffectiveFrom:MMM d, yyyy} - Present";
}
