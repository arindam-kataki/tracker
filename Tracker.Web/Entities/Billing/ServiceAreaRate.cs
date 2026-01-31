namespace Tracker.Web.Entities.Billing;

/// <summary>
/// Standard billing rates by role/resource type within a service area.
/// Used as fallback when no resource-specific or client-specific rate exists.
/// </summary>
public class ServiceAreaRate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The service area this rate card belongs to
    /// </summary>
    public string ServiceAreaId { get; set; } = string.Empty;
    public virtual ServiceArea? ServiceArea { get; set; }

    /// <summary>
    /// The resource type/role this rate applies to (e.g., "Senior Developer", "Architect")
    /// </summary>
    public string ResourceTypeId { get; set; } = string.Empty;
    public virtual ResourceTypeLookup? ResourceType { get; set; }

    /// <summary>
    /// Optional: Client-specific rate within this service area
    /// Null means this is the default rate for all clients
    /// </summary>
    public string? ClientId { get; set; }
    public virtual Resource? Client { get; set; }

    /// <summary>
    /// When this rate takes effect
    /// </summary>
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>
    /// When this rate ends (null = still active)
    /// </summary>
    public DateOnly? EffectiveTo { get; set; }

    /// <summary>
    /// Hourly billing rate
    /// </summary>
    public decimal BillRate { get; set; }

    /// <summary>
    /// Standard cost rate for this role (for margin calculations)
    /// </summary>
    public decimal? CostRate { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Optional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this rate is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // === COMPUTED PROPERTIES ===

    /// <summary>
    /// Margin per hour (if cost rate is specified)
    /// </summary>
    public decimal? HourlyMargin => CostRate.HasValue ? BillRate - CostRate.Value : null;

    /// <summary>
    /// Margin percentage (if cost rate is specified)
    /// </summary>
    public decimal? MarginPercent => CostRate.HasValue && BillRate > 0
        ? Math.Round((BillRate - CostRate.Value) / BillRate * 100, 2)
        : null;

    /// <summary>
    /// Whether this rate is currently in effect
    /// </summary>
    public bool IsCurrentlyEffective
    {
        get
        {
            if (!IsActive) return false;
            var today = DateOnly.FromDateTime(DateTime.Today);
            return EffectiveFrom <= today && (!EffectiveTo.HasValue || EffectiveTo >= today);
        }
    }

    /// <summary>
    /// Check if this rate applies to a specific date
    /// </summary>
    public bool AppliesToDate(DateOnly date)
    {
        if (!IsActive) return false;
        return EffectiveFrom <= date && (!EffectiveTo.HasValue || EffectiveTo >= date);
    }

    /// <summary>
    /// Display string for the rate card
    /// </summary>
    public string DisplayName
    {
        get
        {
            var parts = new List<string>();
            if (ServiceArea != null) parts.Add(ServiceArea.Code);
            if (ResourceType != null) parts.Add(ResourceType.Name);
            if (Client != null) parts.Add($"({Client.Name})");
            return string.Join(" - ", parts);
        }
    }

    /// <summary>
    /// Display string for the effective period
    /// </summary>
    public string EffectivePeriodDisplay => EffectiveTo.HasValue
        ? $"{EffectiveFrom:MMM d, yyyy} - {EffectiveTo:MMM d, yyyy}"
        : $"{EffectiveFrom:MMM d, yyyy} - Present";
}
