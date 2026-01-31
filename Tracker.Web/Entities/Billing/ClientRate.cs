namespace Tracker.Web.Entities.Billing;

/// <summary>
/// Client-specific billing rate overrides for a resource.
/// Takes precedence over the resource's default rate when billing this client.
/// </summary>
public class ClientRate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The resource this rate applies to
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;
    public virtual Resource? Resource { get; set; }

    /// <summary>
    /// The client (Organization with OrganizationType=Client) this rate is for
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
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
    /// Hourly billing rate for this specific client
    /// </summary>
    public decimal BillRate { get; set; }

    /// <summary>
    /// Currency code (e.g., "USD", "EUR", "INR")
    /// </summary>
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Optional notes (e.g., "Negotiated volume discount", "Premium rate for urgent work")
    /// </summary>
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedById { get; set; }

    // === COMPUTED PROPERTIES ===

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
