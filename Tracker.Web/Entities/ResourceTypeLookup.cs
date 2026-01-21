namespace Tracker.Web.Entities;

/// <summary>
/// Defines which Enhancement column a resource type maps to
/// </summary>
public enum EnhancementColumnType
{
    Sponsors = 0,
    SPOCs = 1,
    Resources = 2
}

public class ResourceTypeLookup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Which Enhancement column resources of this type appear in
    /// </summary>
    public EnhancementColumnType EnhancementColumn { get; set; } = EnhancementColumnType.Resources;
    
    /// <summary>
    /// Whether multiple resources of this type are allowed on an enhancement
    /// </summary>
    public bool AllowMultiple { get; set; } = true;

    // Navigation
    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
    
    // Helper
    public string EnhancementColumnDisplay => EnhancementColumn switch
    {
        EnhancementColumnType.Sponsors => "Sponsors",
        EnhancementColumnType.SPOCs => "SPOCs",
        EnhancementColumnType.Resources => "Resources",
        _ => "Unknown"
    };
}
