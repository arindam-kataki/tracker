namespace Tracker.Web.Entities;

/// <summary>
/// Represents a person in the system. Can be an internal team member with login access,
/// or an external contact (client) without login access.
/// This entity replaces the separate User entity - authentication fields are now here.
/// </summary>
public class Resource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // === BASIC INFO ===
    
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public OrganizationType OrganizationType { get; set; } = OrganizationType.Implementor;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // === AUTHENTICATION (merged from User) ===
    
    /// <summary>
    /// Whether this resource can login to the system
    /// </summary>
    public bool HasLoginAccess { get; set; } = false;
    
    /// <summary>
    /// Password hash for local authentication (BCrypt)
    /// </summary>
    public string? PasswordHash { get; set; }
    
    /// <summary>
    /// External OAuth provider ID (e.g., Google)
    /// </summary>
    public string? ExternalId { get; set; }
    
    /// <summary>
    /// Whether this resource has SuperAdmin privileges
    /// </summary>
    public bool IsAdmin { get; set; } = false;
    
    /// <summary>
    /// Whether this resource can access the Consolidation feature
    /// </summary>
    public bool CanConsolidate { get; set; } = false;
    
    /// <summary>
    /// Last successful login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
    
    // === LEGACY (to be removed after migration verification) ===
    
    public string? ResourceTypeId { get; set; }
    public virtual ResourceTypeLookup? ResourceType { get; set; }
    
    // Kept for backward compatibility during transition
    public string? UserId { get; set; }
    
    // === NAVIGATION PROPERTIES ===
    
    /// <summary>
    /// Service area memberships with permissions
    /// </summary>
    public virtual ICollection<ResourceServiceArea> ServiceAreas { get; set; } = new List<ResourceServiceArea>();
    
    /// <summary>
    /// Skills assigned to this resource
    /// </summary>
    public virtual ICollection<ResourceSkill> Skills { get; set; } = new List<ResourceSkill>();
    
    /// <summary>
    /// Enhancements where this resource is a contact
    /// </summary>
    public virtual ICollection<EnhancementContact> EnhancementContacts { get; set; } = new List<EnhancementContact>();
    
    /// <summary>
    /// Enhancements where this resource is assigned
    /// </summary>
    public virtual ICollection<EnhancementResource> EnhancementResources { get; set; } = new List<EnhancementResource>();
    
    /// <summary>
    /// Enhancements where this resource is a sponsor
    /// </summary>
    public virtual ICollection<EnhancementSponsor> EnhancementSponsors { get; set; } = new List<EnhancementSponsor>();
    
    /// <summary>
    /// Enhancements where this resource is a SPOC
    /// </summary>
    public virtual ICollection<EnhancementSpoc> EnhancementSpocs { get; set; } = new List<EnhancementSpoc>();
    
    /// <summary>
    /// Time entries logged by this resource
    /// </summary>
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    
    // === COMPUTED PROPERTIES ===
    
    public string OrganizationTypeDisplay => OrganizationType.ToDisplayString();
    public string OrganizationTypeBadgeClass => OrganizationType.ToBadgeClass();
    
    /// <summary>
    /// Get the primary service area (if any)
    /// </summary>
    public ResourceServiceArea? PrimaryServiceArea => 
        ServiceAreas.FirstOrDefault(sa => sa.IsPrimary) ?? ServiceAreas.FirstOrDefault();
    
    /// <summary>
    /// Get role string for claims (SuperAdmin or User)
    /// </summary>
    public string RoleString => IsAdmin ? "SuperAdmin" : "User";
    
    /// <summary>
    /// Display name for UI (same as Name, for compatibility)
    /// </summary>
    public string DisplayName => Name;
}
