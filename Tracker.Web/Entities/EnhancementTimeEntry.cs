namespace Tracker.Web.Entities;

/// <summary>
/// Represents a time recording entry for an enhancement.
/// Each row has a date range and hours per selected business area category.
/// Date ranges cannot overlap within the same enhancement.
/// </summary>
public class EnhancementTimeEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string EnhancementId { get; set; } = string.Empty;
    
    /// <summary>
    /// Start date of the time period.
    /// </summary>
    public DateTime PeriodStart { get; set; }
    
    /// <summary>
    /// End date of the time period (inclusive).
    /// </summary>
    public DateTime PeriodEnd { get; set; }
    
    /// <summary>
    /// Hours recorded for each category, stored as JSON.
    /// Format: { "categoryId1": 8.5, "categoryId2": 4.0, ... }
    /// </summary>
    public string HoursJson { get; set; } = "{}";
    
    /// <summary>
    /// Optional notes for this time entry.
    /// </summary>
    public string? Notes { get; set; }
    
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? ModifiedBy { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    
    /// <summary>
    /// Helper to get hours dictionary from JSON.
    /// </summary>
    public Dictionary<string, decimal> GetHours()
    {
        if (string.IsNullOrEmpty(HoursJson) || HoursJson == "{}")
            return new Dictionary<string, decimal>();
        
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, decimal>>(HoursJson) 
                   ?? new Dictionary<string, decimal>();
        }
        catch
        {
            return new Dictionary<string, decimal>();
        }
    }
    
    /// <summary>
    /// Helper to set hours dictionary to JSON.
    /// </summary>
    public void SetHours(Dictionary<string, decimal> hours)
    {
        HoursJson = System.Text.Json.JsonSerializer.Serialize(hours);
    }
    
    /// <summary>
    /// Get hours for a specific category.
    /// </summary>
    public decimal GetHoursForCategory(string categoryId)
    {
        var hours = GetHours();
        return hours.TryGetValue(categoryId, out var value) ? value : 0;
    }
    
    /// <summary>
    /// Set hours for a specific category.
    /// </summary>
    public void SetHoursForCategory(string categoryId, decimal value)
    {
        var hours = GetHours();
        hours[categoryId] = value;
        SetHours(hours);
    }
    
    /// <summary>
    /// Total hours across all categories.
    /// </summary>
    public decimal TotalHours => GetHours().Values.Sum();
}
