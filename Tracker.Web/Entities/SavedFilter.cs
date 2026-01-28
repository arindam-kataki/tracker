namespace Tracker.Web.Entities;

public class SavedFilter
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ResourceId { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string FilterJson { get; set; } = "{}";
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    // Navigation
    public virtual Resource Resource { get; set; } = null!;
    public virtual ServiceArea ServiceArea { get; set; } = null!;
}
