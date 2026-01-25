namespace Tracker.Web.Entities;

/// <summary>
/// Represents a timestamped note entry for an enhancement.
/// </summary>
public class Note
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string EnhancementId { get; set; } = string.Empty;
    
    public string NoteText { get; set; } = string.Empty;
    
    public string? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? ModifiedBy { get; set; }
    
    public DateTime? ModifiedAt { get; set; }
    
    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual User? CreatedByUser { get; set; }
}
