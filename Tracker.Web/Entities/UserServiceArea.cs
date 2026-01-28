namespace Tracker.Web.Entities;

public class UserServiceArea
{
    public string UserId { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;

    // Navigation
    public virtual Resource User { get; set; } = null!;
    public virtual ServiceArea ServiceArea { get; set; } = null!;
}
