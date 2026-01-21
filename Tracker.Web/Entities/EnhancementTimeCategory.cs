namespace Tracker.Web.Entities;

/// <summary>
/// Junction table linking enhancements to their selected time recording categories.
/// User selects which business areas (columns) apply to this enhancement.
/// </summary>
public class EnhancementTimeCategory
{
    public string EnhancementId { get; set; } = string.Empty;
    
    public string TimeCategoryId { get; set; } = string.Empty;
    
    /// <summary>
    /// Display order for columns in the time recording table.
    /// </summary>
    public int DisplayOrder { get; set; } = 0;
    
    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual TimeRecordingCategory TimeCategory { get; set; } = null!;
}
