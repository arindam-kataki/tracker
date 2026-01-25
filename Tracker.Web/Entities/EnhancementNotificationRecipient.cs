namespace Tracker.Web.Entities;

/// <summary>
/// Stores notification recipients for an enhancement.
/// Recipients are resources attached to the service area.
/// </summary>
public class EnhancementNotificationRecipient
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EnhancementId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    
    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual Resource Resource { get; set; } = null!;
}
