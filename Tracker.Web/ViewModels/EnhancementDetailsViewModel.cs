using System.ComponentModel.DataAnnotations;
using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

/// <summary>
/// Main view model for the Enhancement Details page with tabs.
/// </summary>
public class EnhancementDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    
    // Tab 1: Ticket Details
    public TicketDetailsViewModel TicketDetails { get; set; } = new();
    
    // Tab 2: Attachments
    public List<AttachmentViewModel> Attachments { get; set; } = new();
    
    // Tab 3: Notes
    public List<NoteViewModel> Notes { get; set; } = new();
    
    // Tab 4: Notifications (NEW)
    public NotificationsViewModel Notifications { get; set; } = new();
    
    // Tab 5: Sharing
    public SharingViewModel Sharing { get; set; } = new();
    
    // Tab 6: Time Recording
    public TimeRecordingViewModel TimeRecording { get; set; } = new();
    
    // Active tab (for returning to same tab after save)
    public string ActiveTab { get; set; } = "details";
    
    // Sidebar
    public bool IsNew => string.IsNullOrEmpty(Id);
}

/// <summary>
/// Tab 1: Ticket Details
/// </summary>
public class TicketDetailsViewModel
{
    public string? Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string WorkId { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
    
    public string? Status { get; set; }  
    public string ServiceAreaId { get; set; } = string.Empty;
    
    // ---------------------------------------------------------------
    // Sizing Section - L3H fields
    // ---------------------------------------------------------------
    public decimal? EstimatedHours { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public string? EstimationNotes { get; set; }
    public string? EstimatedStatus { get; set; }              // L3H Status (Sizing Status)
    public string? ServiceLine { get; set; }
    public string? EstimatedLaborType { get; set; }           // ADD THIS - L3H Labor Type
    public string? EstimatedPriority { get; set; }            // ADD THIS - L3H Priority
    
    // ---------------------------------------------------------------
    // Core Information / Actual Section - Inf* fields
    // ---------------------------------------------------------------
    public decimal? ReturnedHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? InfStatus { get; set; }
    public string? InfServiceLine { get; set; }
    public string? InfLaborType { get; set; }        // ADD THIS - Infosys Labor Type
    public string? InfPriority { get; set; }         // ADD THIS - Infosys Priority
    
    // Legacy time allocations
    public decimal? TimeW1 { get; set; }
    public decimal? TimeW2 { get; set; }
    public decimal? TimeW3 { get; set; }
    public decimal? TimeW4 { get; set; }
    public decimal? TimeW5 { get; set; }
    public decimal? TimeW6 { get; set; }
    public decimal? TimeW7 { get; set; }
    public decimal? TimeW8 { get; set; }
    public decimal? TimeW9 { get; set; }
    
    // Audit
    public string? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    // ---------------------------------------------------------------
    // Available options for dropdowns
    // ---------------------------------------------------------------
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    public List<Resource> AvailableSponsors { get; set; } = new();
    public List<Resource> AvailableSpocs { get; set; } = new();
    public List<Resource> AvailableResources { get; set; } = new();
    public List<Skill> AvailableSkills { get; set; } = new();
    public List<TimeRecordingCategory> AvailableTimeCategories { get; set; } = new();
    public List<string> AvailableServiceLines { get; set; } = new();
    public List<string> AvailableInfStatuses { get; set; } = new();
    
    // ADD THESE 3 - New dropdown option lists
    public List<string> AvailableLaborTypes { get; set; } = new() { "O&M", "Enhancement", "Project" };
    public List<string> AvailablePriorities { get; set; } = new() { "High", "Medium", "Low" };
    public List<string> AvailableSizingStatuses { get; set; } = new() 
    { 
        "Not Started", 
        "In Progress", 
        "Pending Review", 
        "Approved", 
        "Rejected" 
    };
    
    // Selected IDs
    public List<string> SelectedSponsorIds { get; set; } = new();
    public List<string> SelectedSpocIds { get; set; } = new();
    public List<string> SelectedResourceIds { get; set; } = new();
    public List<string> SelectedSkillIds { get; set; } = new();
    public List<string> SelectedTimeCategoryIds { get; set; } = new();
}

/// <summary>
/// Tab 2: Attachments
/// </summary>
public class AttachmentViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string? UploadedBy { get; set; }
    public string? UploadedByName { get; set; }
    public DateTime UploadedAt { get; set; }
    
    public string FileSizeDisplay => FileSize switch
    {
        < 1024 => $"{FileSize} B",
        < 1024 * 1024 => $"{FileSize / 1024.0:F1} KB",
        _ => $"{FileSize / (1024.0 * 1024.0):F1} MB"
    };
    
    // Alias for views that use FileSizeFormatted
    public string FileSizeFormatted => FileSizeDisplay;
}

/// <summary>
/// Tab 3: Notes
/// </summary>
public class NoteViewModel
{
    public string Id { get; set; } = string.Empty;
    public string NoteText { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public bool IsEdited => ModifiedAt.HasValue;
}

/// <summary>
/// Tab 4: Notifications - Persisted notification recipients (NEW)
/// </summary>
public class NotificationsViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    
    /// <summary>
    /// Currently selected recipient resource IDs
    /// </summary>
    public List<string> SelectedRecipientIds { get; set; } = new();
    
    /// <summary>
    /// All available resources for the service area (for selection)
    /// </summary>
    public List<NotificationRecipientOption> AvailableResources { get; set; } = new();
    
    /// <summary>
    /// Currently assigned recipients with details
    /// </summary>
    public List<NotificationRecipientViewModel> CurrentRecipients { get; set; } = new();
}

public class NotificationRecipientOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ResourceType { get; set; }
}

public class NotificationRecipientViewModel
{
    public string Id { get; set; } = string.Empty;  // EnhancementNotificationRecipient.Id
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ResourceType { get; set; }
    public DateTime AddedAt { get; set; }
    public string? AddedBy { get; set; }
}

/// <summary>
/// Tab 5: Sharing
/// </summary>
public class SharingViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string CurrentServiceAreaName { get; set; } = string.Empty;
    
    // Service areas where this enhancement can be shared
    public List<ServiceAreaOption> AvailableTargetServiceAreas { get; set; } = new();
    
    // Service areas where this WorkId already exists (for info display)
    public List<string> ExistingServiceAreaNames { get; set; } = new();
}

public class ServiceAreaOption
{
    public string Id { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;  // ADD THIS
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
    
    // ADD THIS computed property
    public string DisplayName => string.IsNullOrEmpty(Code) ? Name : $"{Code} - {Name}";
}


/// <summary>
/// Tab 6: Time Recording
/// </summary>
public class TimeRecordingViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    
    // Selected categories (columns) for this enhancement
    public List<TimeRecordingCategoryViewModel> SelectedCategories { get; set; } = new();
    
    // Time entries (rows)
    public List<TimeEntryViewModel> Entries { get; set; } = new();
    
    // Totals by category
    public Dictionary<string, decimal> TotalsByCategory { get; set; } = new();
    
    // Grand total
    public decimal GrandTotal => TotalsByCategory.Values.Sum();
}

public class TimeRecordingCategoryViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class TimeEntryViewModel
{
    public string Id { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodDisplay => $"{PeriodStart:MMM d} - {PeriodEnd:MMM d, yyyy}";
    public Dictionary<string, decimal> Hours { get; set; } = new();
    public decimal TotalHours => Hours.Values.Sum();
    public string? Notes { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// Request model for saving ticket details
/// </summary>
public class SaveTicketDetailsRequest
{
    public string? Id { get; set; }
    public string WorkId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string? Status { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;
    
    // Sizing - L3H fields
    public decimal? EstimatedHours { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public string? EstimationNotes { get; set; }
    public string? EstimatedStatus { get; set; }
    public string? ServiceLine { get; set; }
    public string? EstimatedLaborType { get; set; }       // ADD THIS
    public string? EstimatedPriority { get; set; }         // ADD THIS
    
    // Actual/Core Info - Inf* fields
    public decimal? ReturnedHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? InfStatus { get; set; }
    public string? InfServiceLine { get; set; }
    public string? InfLaborType { get; set; }    // ADD THIS
    public string? InfPriority { get; set; }     // ADD THIS
    
    // Legacy time allocations
    public decimal? TimeW1 { get; set; }
    public decimal? TimeW2 { get; set; }
    public decimal? TimeW3 { get; set; }
    public decimal? TimeW4 { get; set; }
    public decimal? TimeW5 { get; set; }
    public decimal? TimeW6 { get; set; }
    public decimal? TimeW7 { get; set; }
    public decimal? TimeW8 { get; set; }
    public decimal? TimeW9 { get; set; }
    
    // Resource selections
    public List<string> SponsorIds { get; set; } = new();
    public List<string> SpocIds { get; set; } = new();
    public List<string> ResourceIds { get; set; } = new();
    public List<string> SkillIds { get; set; } = new();
    public List<string> TimeCategoryIds { get; set; } = new();
}
/// <summary>
/// Request model for creating/updating a note
/// </summary>
public class SaveNoteRequest
{
    public string? Id { get; set; }
    public string EnhancementId { get; set; } = string.Empty;
    public string NoteText { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating/updating a time entry
/// </summary>
public class SaveTimeEntryRequest
{
    public string? Id { get; set; }
    public string EnhancementId { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public Dictionary<string, decimal> Hours { get; set; } = new();
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for sharing
/// </summary>
public class ShareEnhancementRequest
{
    public string EnhancementId { get; set; } = string.Empty;
    public string TargetServiceAreaId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for adding a notification recipient
/// </summary>
public class AddNotificationRecipientRequest
{
    public string EnhancementId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for removing a notification recipient
/// </summary>
public class RemoveNotificationRecipientRequest
{
    public string RecipientId { get; set; } = string.Empty;
}