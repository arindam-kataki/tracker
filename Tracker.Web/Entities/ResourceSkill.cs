namespace Tracker.Web.Entities;

public class ResourceSkill
{
    public string ResourceId { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;

    // Navigation
    public virtual Resource Resource { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}
