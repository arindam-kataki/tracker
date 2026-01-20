namespace Tracker.Web.Entities;

public class Resource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsClientResource { get; set; } // True = Client/Sponsor, False = Internal
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ICollection<EnhancementContact> EnhancementContacts { get; set; } = new List<EnhancementContact>();
    public virtual ICollection<EnhancementResource> EnhancementResources { get; set; } = new List<EnhancementResource>();
}
