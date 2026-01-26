namespace Tracker.Web.Entities;

/// <summary>
/// Represents a person in the system - can be a user (with login) or just a contact.
/// Combines the old User and Resource tables into one unified entity.
/// </summary>
public class Resource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // === BASIC INFO ===
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // === CLASSIFICATION ===
    /// <summary>
    /// Client, Implementor, or Vendor
    /// </summary>
    public OrganizationType OrganizationType { get; set; } = OrganizationType.Implementor;
    
    // === LOGIN (optional) ===
    /// <summary>
    /// Whether this resource can log into the system
    /// </summary>
    public bool CanLogin { get; set; } = false;
    
    /// <summary>
    /// Password hash for authentication (null if CanLogin is false)
    /// </summary>
    public string? PasswordHash { get; set; }
    
    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    // === ADMIN ===
    /// <summary>
    /// Full system access - bypasses all SA-based permissions.
    /// Only Implementor resources can be admin.
    /// </summary>
    public bool IsAdmin { get; set; } = false;
    
    // === NAVIGATION ===
    
    /// <summary>
    /// Service area memberships with permissions
    /// </summary>
    public virtual ICollection<ResourceServiceArea> ServiceAreas { get; set; } = new List<ResourceServiceArea>();
    
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
    /// Whether this resource can be an admin (only Implementor)
    /// </summary>
    public bool CanBeAdmin => OrganizationType == OrganizationType.Implementor;
    
    /// <summary>
    /// Whether this resource can have login capability
    /// </summary>
    public bool CanHaveLogin => OrganizationType != OrganizationType.Client;
    
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
    /// External client/customer - cannot login, cannot be admin
    /// </summary>
    Client = 0,
    
    /// <summary>
    /// Internal team member - can login, can be admin
    /// </summary>
    Implementor = 1,
    
    /// <summary>
    /// Third-party vendor - can login, cannot be admin
    /// </summary>
    Vendor = 2
}
