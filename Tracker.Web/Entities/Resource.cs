namespace Tracker.Web.Entities;

public class Resource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ResourceTypeId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ResourceTypeLookup? ResourceType { get; set; }
    public virtual ICollection<ResourceSkill> Skills { get; set; } = new List<ResourceSkill>();
    public virtual ICollection<EnhancementSponsor> EnhancementSponsors { get; set; } = new List<EnhancementSponsor>();
    public virtual ICollection<EnhancementSpoc> EnhancementSpocs { get; set; } = new List<EnhancementSpoc>();
    public virtual ICollection<EnhancementResource> EnhancementResources { get; set; } = new List<EnhancementResource>();
    
    // Legacy navigation (keep for now)
    public virtual ICollection<EnhancementContact> EnhancementContacts { get; set; } = new List<EnhancementContact>();

    // Helper property for display
    public string ResourceTypeName => ResourceType?.Name ?? "Unknown";
    public string SkillsDisplay => Skills?.Any() == true 
        ? string.Join(", ", Skills.Select(s => s.Skill?.Name).Where(n => n != null)) 
        : string.Empty;
}
