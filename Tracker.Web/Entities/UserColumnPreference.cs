namespace Tracker.Web.Entities;

public class UserColumnPreference
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ColumnsJson { get; set; } = "[]"; // Array of visible column keys
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual User User { get; set; } = null!;
    public virtual ServiceArea ServiceArea { get; set; } = null!;
}
