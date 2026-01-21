namespace Tracker.Web.Entities;

public class EnhancementSkill
{
    public string EnhancementId { get; set; } = string.Empty;
    public string SkillId { get; set; } = string.Empty;

    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual Skill Skill { get; set; } = null!;
}
