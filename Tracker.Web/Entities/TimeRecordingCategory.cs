namespace Tracker.Web.Entities;

/// <summary>
/// Lookup table for time recording categories (business areas).
/// Examples: Custom Apps, SCM, ERP, MRP, CRM, etc.
/// </summary>
public class TimeRecordingCategory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    public int DisplayOrder { get; set; } = 0;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual ICollection<EnhancementTimeCategory> EnhancementTimeCategories { get; set; } = new List<EnhancementTimeCategory>();
}
