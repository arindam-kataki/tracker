namespace Tracker.Web.Entities.Availability;

/// <summary>
/// Company-wide holidays that apply to all or specific groups of resources
/// </summary>
public class CompanyHoliday
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the holiday (e.g., "Christmas Day", "Independence Day")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Date of the holiday
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Hours off for this holiday (typically 8 for full day, 4 for half day)
    /// </summary>
    public decimal HoursOff { get; set; } = 8m;

    /// <summary>
    /// Optional: Limit this holiday to a specific service area
    /// Null means it applies to all service areas
    /// </summary>
    public string? ServiceAreaId { get; set; }
    public virtual ServiceArea? ServiceArea { get; set; }

    /// <summary>
    /// Optional: Limit this holiday to a specific location/region
    /// Useful for international organizations (e.g., "US", "UK", "India")
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Whether this holiday is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is a recurring annual holiday
    /// If true, the holiday repeats every year on the same date
    /// </summary>
    public bool IsRecurringAnnually { get; set; } = true;

    /// <summary>
    /// Optional description or notes
    /// </summary>
    public string? Description { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // === COMPUTED PROPERTIES ===

    /// <summary>
    /// Whether this is a full day holiday
    /// </summary>
    public bool IsFullDay => HoursOff >= 8m;

    /// <summary>
    /// Display string for hours (e.g., "Full Day" or "Half Day (4h)")
    /// </summary>
    public string HoursDisplay => HoursOff >= 8m 
        ? "Full Day" 
        : $"Partial ({HoursOff}h)";

    /// <summary>
    /// Scope display (e.g., "All", "ADM only", "US only")
    /// </summary>
    public string ScopeDisplay
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(ServiceAreaId))
                parts.Add(ServiceArea?.Code ?? ServiceAreaId);
            if (!string.IsNullOrEmpty(Location))
                parts.Add(Location);
            return parts.Any() ? string.Join(", ", parts) : "All";
        }
    }

    /// <summary>
    /// Check if this holiday applies to a given date (considering recurrence)
    /// </summary>
    public bool AppliesToDate(DateOnly date)
    {
        if (!IsActive) return false;

        if (IsRecurringAnnually)
        {
            // Match month and day regardless of year
            return Date.Month == date.Month && Date.Day == date.Day;
        }
        else
        {
            // Exact date match
            return Date == date;
        }
    }

    /// <summary>
    /// Check if this holiday applies to a resource based on service area and location
    /// </summary>
    public bool AppliesToResource(string? resourceServiceAreaId, string? resourceLocation)
    {
        if (!IsActive) return false;

        // Check service area filter
        if (!string.IsNullOrEmpty(ServiceAreaId) && ServiceAreaId != resourceServiceAreaId)
            return false;

        // Check location filter
        if (!string.IsNullOrEmpty(Location) && Location != resourceLocation)
            return false;

        return true;
    }
}
