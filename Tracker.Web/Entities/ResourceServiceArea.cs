namespace Tracker.Web.Entities;

public class ResourceServiceArea
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ResourceId { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    public bool IsPrimary { get; set; } = false;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Permissions Permissions { get; set; } = Permissions.None;
    
    public virtual Resource Resource { get; set; } = null!;
    public virtual ServiceArea ServiceArea { get; set; } = null!;
    
    public bool HasPermission(Permissions permission) => Permissions.HasFlag(permission);
}
