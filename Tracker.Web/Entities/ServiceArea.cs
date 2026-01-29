namespace Tracker.Web.Entities;

public class ServiceArea
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    //public virtual ICollection<ResourceServiceArea> ResourceServiceAreas { get; set; } = new List<ResourceServiceArea>();
    public virtual ICollection<Enhancement> Enhancements { get; set; } = new List<Enhancement>();
}
