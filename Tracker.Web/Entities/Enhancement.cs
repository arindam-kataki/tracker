using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Tracker.Web.Entities;

public class Enhancement
{
    [Description("Identifier")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    [Description("Spawned From Identifier")]
    public string SpawnedFromId { get; set; } = string.Empty;

    // Core fields from upload
    [Required]
    public string WorkId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? Status { get; set; }
    [MaxLength(500)]
    public string? Tags { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;

    // ---------------------------------------------------------------
    // L3H fields    
    // ---------------------------------------------------------------

    [Description("L3H Source")]
    public string? Source { get; set; }
    [Description("Request Raised Date")]
    public DateTime? RequestRaisedDate { get; set; }
    [Description("L3H Labor Type")]
    public string? EstimatedLaborType { get; set; }
    [Description("L3H Activity Type")]
    public string? EstimatedActivityType { get; set; }
    [Description("L3H Priority")]
    public string? EstimatedPriority { get; set; }
    [Description("L3H Estimated Hours")]
    public decimal? EstimatedHours { get; set; }
    [Description("L3H Estimated Start Date")]
    public DateTime? EstimatedStartDate { get; set; }
    [Description("L3H Estimated End Date")]
    public DateTime? EstimatedEndDate { get; set; }
    [Description("L3H Estimation Notes")]
    public string? EstimationNotes { get; set; }
    [Description("L3H Estimated Status")]
    public string? EstimatedStatus { get; set; }
    [Description("L3H Service Line")]
    public string? ServiceLine { get; set; }
    [Description("L3H SignIT Reference")]
    public string? SignITReference { get; set; }

    // ---------------------------------------------------------------
    // Infosys fields
    // ---------------------------------------------------------------

    [Description("Signed Hours")]
    public decimal? ReturnedHours { get; set; }
    [Description("Infosys Estimated Hours")]
    public decimal? InfEstimatedHours { get; set; }
    [Description("Start Date")]
    public DateTime? StartDate { get; set; }
    [Description("End Date")]
    public DateTime? EndDate { get; set; }
    [Description("Infosys Status")]
    public string? InfStatus { get; set; }
    [Description("Infosys Activity Type")]
    public string? InfActivityType { get; set; }
    [Description("Infosys Service Line")]
    public string? InfServiceLine { get; set; }
    [Description("Infosys Labor Type")]
    public string? InfLaborType { get; set; }
    [Description("Infosys Priority")]
    public string? InfPriority { get; set; }
    [Description("Dependencies")]
    public string? Dependencies { get; set; }
    [Description("Infosys Cost Center")]
    public string? InfCostCenter { get; set; }

    public decimal? TimeW1 { get; set; }
    public decimal? TimeW2 { get; set; }
    public decimal? TimeW3 { get; set; }
    public decimal? TimeW4 { get; set; }
    public decimal? TimeW5 { get; set; }
    public decimal? TimeW6 { get; set; }
    public decimal? TimeW7 { get; set; }
    public decimal? TimeW8 { get; set; }
    public decimal? TimeW9 { get; set; }

    // ---------------------------------------------------------------
    // Audit fields
    // ---------------------------------------------------------------

    [Timestamp]
    public byte[]? RowVersion { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? LockedBy { get; set; }
    public DateTime LockedAt { get; set; }

    // ---------------------------------------------------------------
    // Navigation properties
    // ---------------------------------------------------------------
    
    public virtual ServiceArea ServiceArea { get; set; } = null!;
    public virtual EstimationBreakdown? EstimationBreakdown { get; set; }
    
    // Resource collections
    public virtual ICollection<EnhancementSponsor> Sponsors { get; set; } = new List<EnhancementSponsor>();
    public virtual ICollection<EnhancementSpoc> Spocs { get; set; } = new List<EnhancementSpoc>();
    public virtual ICollection<EnhancementResource> Resources { get; set; } = new List<EnhancementResource>();
    
    // Legacy (keep for backward compatibility)
    public virtual ICollection<EnhancementContact> Contacts { get; set; } = new List<EnhancementContact>();
    
    // Skills
    public virtual ICollection<EnhancementSkill> Skills { get; set; } = new List<EnhancementSkill>();
    
    // Notes and attachments
    [Description("Enhancement Notes")]
    public virtual ICollection<Note> NoteHistory { get; set; } = new List<Note>();
    
    [Description("Enhancement Attachments")]
    public virtual ICollection<EnhancementAttachment> Attachments { get; set; } = new List<EnhancementAttachment>();
    
    // Time recording categories (selected business areas)
    public virtual ICollection<EnhancementTimeCategory> TimeCategories { get; set; } = new List<EnhancementTimeCategory>();
    
    // Time recording entries (legacy)
    public virtual ICollection<EnhancementTimeEntry> TimeEntries { get; set; } = new List<EnhancementTimeEntry>();
    
    // New timesheet entries
    public virtual ICollection<TimeEntry> TimeEntriesNew { get; set; } = new List<TimeEntry>();
    
    // Estimation breakdown items (new dynamic model)
    public virtual ICollection<EstimationBreakdownItem> EstimationBreakdownItems { get; set; } = new List<EstimationBreakdownItem>();
    
    // Consolidations
    public virtual ICollection<Consolidation> Consolidations { get; set; } = new List<Consolidation>();
    
    // Notification recipients
    public virtual ICollection<EnhancementNotificationRecipient> NotificationRecipients { get; set; } = new List<EnhancementNotificationRecipient>();

    // ---------------------------------------------------------------
    // Helper properties for display (CSV format)
    // ---------------------------------------------------------------
    
    public string SponsorsDisplay => Sponsors?.Any() == true 
        ? string.Join(", ", Sponsors.Select(s => s.Resource?.Name ?? ""))
        : string.Empty;
    
    public string SpocsDisplay => Spocs?.Any() == true 
        ? string.Join(", ", Spocs.Select(s => s.Resource?.Name ?? ""))
        : string.Empty;
    
    public string ResourcesDisplay => Resources?.Any() == true 
        ? string.Join(", ", Resources.Select(r => r.Resource?.Name ?? ""))
        : string.Empty;
    
    public string SkillsDisplay => Skills?.Any() == true 
        ? string.Join(", ", Skills.Select(s => s.Skill?.Name ?? ""))
        : string.Empty;

    // ---------------------------------------------------------------
    // Helper properties for tags
    // ---------------------------------------------------------------
    
    public List<string> TagList => string.IsNullOrWhiteSpace(Tags) 
        ? new List<string>() 
        : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    public void SetTags(IEnumerable<string> tags)
    {
        Tags = tags?.Any() == true 
            ? string.Join(",", tags.Select(t => t.Trim().ToLowerInvariant()).Distinct()) 
            : null;
    }

    public bool HasTag(string tag) => TagList.Contains(tag.Trim().ToLowerInvariant(), StringComparer.OrdinalIgnoreCase);

    // ---------------------------------------------------------------
    // Computed properties for time tracking
    // ---------------------------------------------------------------
    
    /// <summary>
    /// Total hours from time recording entries (legacy EnhancementTimeEntry).
    /// </summary>
    public decimal TotalRecordedHours => TimeEntries?.Sum(te => te.TotalHours) ?? 0;
}
