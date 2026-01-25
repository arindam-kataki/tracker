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
    


    // Service Area
    public string ServiceAreaId { get; set; } = string.Empty;

    // ---------------------------------------------------------------
    // L3H fields    
    // ---------------------------------------------------------------

    [Description("L3H Source")]
    public string? Source { get; set; }
    [Description("Request Raised Date")]
    public DateTime? RequestRaisedDate { get; set; }
    [Description("L3H Labor Type")]
    public decimal? LaborType { get; set; }
    [Description("L3H Activity Type")]
    public decimal? ActivityType { get; set; }
    [Description("L3H Priority")]
    public string? Priority { get; set; }
    [Description("L3H Estimated Hours")]
    public decimal? EstimatedHours { get; set; }
    [Description("L3H Estimated Start Date")]
    public DateTime? EstimatedStartDate { get; set; }
    [Description("L3H Estimated End Date")]
    public DateTime? EstimatedEndDate { get; set; }
    [Description("L3H Estimatimation Notes")]
    public string? EstimationNotes { get; set; }
    [Description("L3H Status")]
    public string? Status { get; set; }
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
    [Description("Start Date Hours")]
    public DateTime? StartDate { get; set; }
    [Description("End Date Hours")]
    public DateTime? EndDate { get; set; }
    [Description("Infosys Status")]
    public string? InfStatus { get; set; }
    [Description("Infosys Service Line")]
    public string? InfServiceLine { get; set; }
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
    
    // Navigation
    public virtual ServiceArea ServiceArea { get; set; } = null!;               // service area of the enhancement
    public virtual EstimationBreakdown? EstimationBreakdown { get; set; }       // estimation is by service acre
    
    // Resource collections
    public virtual ICollection<EnhancementSponsor> Sponsors { get; set; } = new List<EnhancementSponsor>();
    public virtual ICollection<EnhancementSpoc> Spocs { get; set; } = new List<EnhancementSpoc>();
    public virtual ICollection<EnhancementResource> Resources { get; set; } = new List<EnhancementResource>();
    
    // Legacy (keep for backward compatibility)
    public virtual ICollection<EnhancementContact> Contacts { get; set; } = new List<EnhancementContact>();
    
    // Skills
    public virtual ICollection<EnhancementSkill> Skills { get; set; } = new List<EnhancementSkill>();
    
    [Description("Enhancement Notes")]
    public virtual ICollection<Note> NoteHistory { get; set; } = new List<Note>();
    
    [Description("Enhancement Attachments")]
    public virtual ICollection<EnhancementAttachment> Attachments { get; set; } = new List<EnhancementAttachment>();
    
    // New: Time recording categories (selected business areas)
    public virtual ICollection<EnhancementTimeCategory> TimeCategories { get; set; } = new List<EnhancementTimeCategory>();
    
    // New: Time recording entries
    public virtual ICollection<EnhancementTimeEntry> TimeEntries { get; set; } = new List<EnhancementTimeEntry>();
    
    // Helper properties for display (CSV format)
    public string SponsorsDisplay => Sponsors?.Any() == true 
        ? string.Join(", ", Sponsors.Select(s => s.Resource?.Name).Where(n => n != null)) 
        : string.Empty;
    
    public string SpocsDisplay => Spocs?.Any() == true 
        ? string.Join(", ", Spocs.Select(s => s.Resource?.Name).Where(n => n != null)) 
        : string.Empty;
    
    public string ResourcesDisplay => Resources?.Any() == true 
        ? string.Join(", ", Resources.Select(r => r.Resource?.Name).Where(n => n != null)) 
        : string.Empty;
    
    public string SkillsDisplay => Skills?.Any() == true
        ? string.Join(", ", Skills.Select(s => s.Skill?.Name).Where(n => n != null))
        : string.Empty;
    
    /// <summary>
    /// Total hours from time recording entries.
    /// </summary>
    public decimal TotalRecordedHours => TimeEntries?.Sum(te => te.TotalHours) ?? 0;
    public virtual ICollection<EnhancementNotificationRecipient> NotificationRecipients { get; set; } = new List<EnhancementNotificationRecipient>();
    public virtual ICollection<EstimationBreakdownItem> EstimationBreakdownItems { get; set; } = new List<EstimationBreakdownItem>();
    public virtual ICollection<TimeEntry> TimeEntriesNew { get; set; } = new List<TimeEntry>();
    public virtual ICollection<Consolidation> Consolidations { get; set; } = new List<Consolidation>();
}
