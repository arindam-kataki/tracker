namespace Tracker.Web.Entities;

public enum ResourceType
{
    Client = 0,      // Client sponsors
    SPOC = 1,        // Infy Single Point of Contact
    Internal = 2     // Infy internal resources
}

public class Resource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public ResourceType Type { get; set; } = ResourceType.Internal;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Legacy property for backward compatibility
    public bool IsClientResource => Type == ResourceType.Client;

    // Navigation
    public virtual ICollection<EnhancementSponsor> EnhancementSponsors { get; set; } = new List<EnhancementSponsor>();
    public virtual ICollection<EnhancementSpoc> EnhancementSpocs { get; set; } = new List<EnhancementSpoc>();
    public virtual ICollection<EnhancementResource> EnhancementResources { get; set; } = new List<EnhancementResource>();
    
    // Legacy navigation (keep for now)
    public virtual ICollection<EnhancementContact> EnhancementContacts { get; set; } = new List<EnhancementContact>();
}
