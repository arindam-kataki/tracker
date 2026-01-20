namespace Tracker.Web.Entities;

/// <summary>
/// Represents a named/saved report definition that users can create, edit, and run.
/// Reports can span multiple service areas (for users with access to those areas).
/// </summary>
public class NamedReport
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Display name of the report.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// User who created the report.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON array of service area IDs to include in this report.
    /// e.g., ["sa-1", "sa-2", "sa-3"]
    /// </summary>
    public string ServiceAreaIdsJson { get; set; } = "[]";
    
    /// <summary>
    /// JSON representation of filter criteria (same structure as EnhancementFilterViewModel).
    /// </summary>
    public string FilterJson { get; set; } = "{}";
    
    /// <summary>
    /// JSON array of column keys to include in the report, in display order.
    /// e.g., ["workIdDescription", "status", "estimatedHours", "sponsors"]
    /// </summary>
    public string ColumnsJson { get; set; } = "[]";
    
    /// <summary>
    /// Optional description of what this report contains.
    /// </summary>
    public string? Description { get; set; }
    
    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    
    // Navigation
    public virtual User User { get; set; } = null!;
}
