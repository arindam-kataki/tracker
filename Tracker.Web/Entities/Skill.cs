namespace Tracker.Web.Entities;

public class Skill
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public virtual ServiceArea ServiceArea { get; set; } = null!;
    public virtual ICollection<ResourceSkill> ResourceSkills { get; set; } = new List<ResourceSkill>();
    public virtual ICollection<EnhancementSkill> EnhancementSkills { get; set; } = new List<EnhancementSkill>();
}
