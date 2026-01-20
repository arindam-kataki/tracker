namespace Tracker.Web.Entities;

public class EnhancementSpoc
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EnhancementId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;

    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual Resource Resource { get; set; } = null!;
}
