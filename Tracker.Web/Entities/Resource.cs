namespace Tracker.Web.Entities;

public class Resource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public OrganizationType OrganizationType { get; set; } = OrganizationType.Implementor;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Legacy ResourceType link
    public string? ResourceTypeId { get; set; }
    public virtual ResourceTypeLookup? ResourceType { get; set; }
    
    // Optional User link
    public string? UserId { get; set; }
    public virtual User? User { get; set; }
    
    // New: Service area memberships
    public virtual ICollection<ResourceServiceArea> ServiceAreas { get; set; } = new List<ResourceServiceArea>();
    
    // Skills
    public virtual ICollection<ResourceSkill> Skills { get; set; } = new List<ResourceSkill>();
    
    // Legacy enhancement relationships
    public virtual ICollection<EnhancementContact> EnhancementContacts { get; set; } = new List<EnhancementContact>();
    public virtual ICollection<EnhancementResource> EnhancementResources { get; set; } = new List<EnhancementResource>();
    public virtual ICollection<EnhancementSponsor> EnhancementSponsors { get; set; } = new List<EnhancementSponsor>();
    public virtual ICollection<EnhancementSpoc> EnhancementSpocs { get; set; } = new List<EnhancementSpoc>();
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    
    // Computed
    public string OrganizationTypeDisplay => OrganizationType.ToDisplayString();
    public string OrganizationTypeBadgeClass => OrganizationType.ToBadgeClass();
}
