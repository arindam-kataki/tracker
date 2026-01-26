namespace Tracker.Web.Entities;

/// <summary>
/// Represents a resource/person that can be assigned to enhancements.
/// This is separate from User (which handles authentication).
/// Resources are categorized by ResourceType and OrganizationType, and can have Skills.
/// </summary>
public class Resource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // === BASIC INFO ===
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // === CLASSIFICATION ===
    
    /// <summary>
    /// Client, Implementor, or Vendor
    /// </summary>
    public OrganizationType OrganizationType { get; set; } = OrganizationType.Implementor;
    
    /// <summary>
    /// Foreign key to ResourceTypeLookup (e.g., Developer, Tester, Analyst, Sponsor, SPOC)
    /// </summary>
    public string? ResourceTypeId { get; set; }
    
    /// <summary>
    /// Navigation property to ResourceTypeLookup
    /// </summary>
    public virtual ResourceTypeLookup? ResourceType { get; set; }
    
    // === NAVIGATION ===
    
    /// <summary>
    /// Skills associated with this resource
    /// </summary>
    public virtual ICollection<ResourceSkill> Skills { get; set; } = new List<ResourceSkill>();
    
    /// <summary>
    /// Enhancements where this resource is a sponsor
    /// </summary>
    public virtual ICollection<EnhancementSponsor> EnhancementSponsors { get; set; } = new List<EnhancementSponsor>();
    
    /// <summary>
    /// Enhancements where this resource is a SPOC
    /// </summary>
    public virtual ICollection<EnhancementSpoc> EnhancementSpocs { get; set; } = new List<EnhancementSpoc>();
    
    /// <summary>
    /// Enhancements where this resource is assigned
    /// </summary>
    public virtual ICollection<EnhancementResource> EnhancementResources { get; set; } = new List<EnhancementResource>();
    
    /// <summary>
    /// Time entries logged by this resource
    /// </summary>
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    
    // Legacy navigation (keep for backward compatibility)
    public virtual ICollection<EnhancementContact> EnhancementContacts { get; set; } = new List<EnhancementContact>();
    
    // === COMPUTED PROPERTIES ===
    
    /// <summary>
    /// Display string for organization type
    /// </summary>
    public string OrganizationTypeDisplay => OrganizationType switch
    {
        OrganizationType.Client => "Client",
        OrganizationType.Implementor => "Implementor",
        OrganizationType.Vendor => "Vendor",
        _ => "Unknown"
    };
    
    /// <summary>
    /// Resource type name for display
    /// </summary>
    public string ResourceTypeDisplay => ResourceType?.Name ?? "Unassigned";
    
    /// <summary>
    /// Skills as comma-separated display string
    /// </summary>
    public string SkillsDisplay => Skills?.Any() == true 
        ? string.Join(", ", Skills.Select(s => s.Skill?.Name).Where(n => n != null)) 
        : string.Empty;
}

/// <summary>
/// Organization type classification for resources
/// </summary>
public enum OrganizationType
{
    /// <summary>
    /// External client/customer
    /// </summary>
    Client = 0,
    
    /// <summary>
    /// Internal team member
    /// </summary>
    Implementor = 1,
    
    /// <summary>
    /// Third-party vendor
    /// </summary>
    Vendor = 2
}