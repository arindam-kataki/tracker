using System.ComponentModel.DataAnnotations;
using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

// ============================================================
// TIMESHEET VIEW MODELS
// ============================================================

/// <summary>
/// Main view model for the My Timesheet page
/// </summary>
public class MyTimesheetViewModel
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    
    // Date Range Filter
    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(6 - (int)DateTime.Today.DayOfWeek);
    
    // Time Entry Data
    public List<TimeEntry> Entries { get; set; } = new();
    public List<WorkPhase> WorkPhases { get; set; } = new();
    
    // Summary
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
    
    // Grouping for display
    public Dictionary<string, List<TimeEntry>> EntriesByDate { get; set; } = new();
    public Dictionary<string, decimal> HoursByPhase { get; set; } = new();
    
    // Filter Options (populated from database)
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    public List<string> AvailableStatuses { get; set; } = new();
    public List<string> AvailableTags { get; set; } = new();
    
    // Current Filter Values (for maintaining state)
    public string? FilterServiceAreaId { get; set; }
    public string? FilterStatus { get; set; }
    public string? FilterWorkId { get; set; }
    public string? FilterDescription { get; set; }
    public string? FilterTag { get; set; }
}

/// <summary>
/// View model for creating/editing a time entry
/// </summary>
public class TimeEntryEditViewModel
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Enhancement is required")]
    public string EnhancementId { get; set; } = string.Empty;
    
    public string? ServiceAreaId { get; set; }
    
    public string ResourceId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Work phase is required")]
    public string WorkPhaseId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Start date is required")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;
    
    [Required(ErrorMessage = "End date is required")]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; } = DateTime.Today;
    
    [Required(ErrorMessage = "Hours is required")]
    [Range(0.01, 999.99, ErrorMessage = "Hours must be between 0.01 and 999.99")]
    public decimal Hours { get; set; }
    
    [Range(0, 999.99, ErrorMessage = "Contributed hours must be between 0 and 999.99")]
    public decimal ContributedHours { get; set; }
    
    public string? Notes { get; set; }
    
    // For dropdowns - service areas where user has LogTimesheet permission
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    public List<Enhancement> AvailableEnhancements { get; set; } = new();
    public List<WorkPhase> AvailableWorkPhases { get; set; } = new();
    
    // Display info (for edit mode)
    public string? EnhancementWorkId { get; set; }
    public string? EnhancementDescription { get; set; }
    public string? WorkPhaseName { get; set; }
    public bool IsConsolidated { get; set; }
}

/// <summary>
/// View model for enhancement selection in timesheet
/// </summary>
public class EnhancementSelectViewModel
{
    public string Id { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ServiceAreaCode { get; set; }
    public string? Status { get; set; }
    public string? Tags { get; set; }
    
    public string DisplayText => $"{WorkId} - {Description?.Substring(0, Math.Min(Description?.Length ?? 0, 50))}";
}

/// <summary>
/// View model for timesheet entries list (admin view)
/// </summary>
public class TimesheetEntriesViewModel
{
    // Filters
    public string? ServiceAreaId { get; set; }
    public string? EnhancementId { get; set; }
    public string? ResourceId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Data
    public List<TimeEntry> Entries { get; set; } = new();
    
    // For filter dropdowns
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    public List<Enhancement> AvailableEnhancements { get; set; } = new();
    public List<Resource> AvailableResources { get; set; } = new();
    
    // Summary
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
}

/// <summary>
/// Filter model for enhancement search in timesheet entry
/// </summary>
public class EnhancementFilterForTimesheetViewModel
{
    public string? ServiceAreaId { get; set; }
    public string? Status { get; set; }
    public string? WorkId { get; set; }
    public string? Description { get; set; }
    public string? Tag { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
}


// ============================================================
// WORK PHASE VIEW MODELS
// ============================================================

/// <summary>
/// View model for Work Phases management page
/// </summary>
public class WorkPhasesViewModel
{
    public List<WorkPhase> WorkPhases { get; set; } = new();
    public string? SearchTerm { get; set; }
}

/// <summary>
/// View model for editing a work phase
/// </summary>
public class EditWorkPhaseViewModel
{
    public string? Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Range(0, 100)]
    public int DefaultContributionPercent { get; set; } = 100;
    
    public int DisplayOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public bool ForEstimation { get; set; } = true;
    
    public bool ForTimeRecording { get; set; } = true;
    
    public bool ForConsolidation { get; set; } = true;
}


// ============================================================
// CONSOLIDATION VIEW MODELS
// ============================================================

/// <summary>
/// Main view model for Consolidation list page
/// </summary>
public class ConsolidationListViewModel
{
    // Filters
    public string? ServiceAreaId { get; set; }
    public string? EnhancementId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ConsolidationStatus? Status { get; set; }
    
    // Data
    public List<Consolidation> Consolidations { get; set; } = new();
    
    // For filter dropdowns
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    
    // Summary
    public decimal TotalBillableHours { get; set; }
    public int DraftCount { get; set; }
    public int FinalizedCount { get; set; }
}

/// <summary>
/// View model for creating a new consolidation (from timesheets tab)
/// </summary>
public class ConsolidationFromTimesheetsViewModel
{
    // Step 1: Filters
    public string? ServiceAreaId { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    
    [Required]
    public DateTime EndDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
    
    // Step 2: Enhancement selection
    [Required]
    public string EnhancementId { get; set; } = string.Empty;
    
    // Available service areas and enhancements
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    public List<EnhancementWithTimeEntries> AvailableEnhancements { get; set; } = new();
    
    // Step 3: Time entries with pull amounts
    public List<TimeEntryForConsolidationViewModel> TimeEntries { get; set; } = new();
    
    // Step 4: Final billable hours
    public decimal BillableHours { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Enhancement with summary of time entries
/// </summary>
public class EnhancementWithTimeEntries
{
    public string Id { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ServiceAreaCode { get; set; }
    public int EntryCount { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
}

/// <summary>
/// Time entry for consolidation selection
/// </summary>
public class TimeEntryForConsolidationViewModel
{
    public string Id { get; set; } = string.Empty;
    public string TimeEntryId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string WorkPhaseName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal ContributedHours { get; set; }
    public decimal AvailableHours { get; set; } // ContributedHours - already pulled
    public decimal AlreadyPulledHours { get; set; } // Hours already pulled into other consolidations
    public decimal PullHours { get; set; } // Amount to pull for this consolidation
    public bool IsSelected { get; set; }
    public string? Notes { get; set; }
    
    public string DateRangeDisplay => StartDate == EndDate 
        ? StartDate.ToString("MMM d") 
        : $"{StartDate:MMM d}-{EndDate:d}";
}

/// <summary>
/// View model for creating a manual consolidation
/// </summary>
public class ConsolidationManualViewModel
{
    public string? ServiceAreaId { get; set; }
    
    [Required]
    public string EnhancementId { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
    
    [Required]
    public DateTime EndDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
    
    [Required]
    [Range(0.01, 99999.99)]
    public decimal BillableHours { get; set; }
    
    [Required]
    public string Notes { get; set; } = string.Empty;
    
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    public List<Enhancement> AvailableEnhancements { get; set; } = new();
}

/// <summary>
/// View model for viewing/editing a consolidation
/// </summary>
public class ConsolidationDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string EnhancementId { get; set; } = string.Empty;
    public string EnhancementWorkId { get; set; } = string.Empty;
    public string? EnhancementDescription { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BillableHours { get; set; }
    public decimal SourceHours { get; set; } // Total hours from source time entries
    public ConsolidationStatus Status { get; set; }
    public string? Notes { get; set; }
    public bool IsManual { get; set; }
    
    // Sources
    public List<ConsolidationSourceViewModel> Sources { get; set; } = new();
    
    // Audit
    public string? CreatedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedByName { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    public string StatusDisplay => Status.ToString();
    public string PeriodDisplay => StartDate.ToString("MMM yyyy");
    public string DateRangeDisplay => StartDate == EndDate 
        ? StartDate.ToString("MMM d, yyyy")
        : $"{StartDate:MMM d} - {EndDate:d}, {EndDate:yyyy}";
    
    public bool CanEdit => Status == ConsolidationStatus.Draft;
    public bool CanDelete => Status == ConsolidationStatus.Draft;
    public bool CanFinalize => Status == ConsolidationStatus.Draft;
}

/// <summary>
/// Consolidation source display
/// </summary>
public class ConsolidationSourceViewModel
{
    public string TimeEntryId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string WorkPhaseName { get; set; } = string.Empty;
    public decimal OriginalHours { get; set; }
    public decimal OriginalContributedHours { get; set; }
    public decimal PulledHours { get; set; }
    
    public string DateRangeDisplay => StartDate == EndDate 
        ? StartDate.ToString("MMM d") 
        : $"{StartDate:MMM d}-{EndDate:d}";
}


// ============================================================
// ESTIMATION VIEW MODELS (Updated for new model)
// ============================================================

/// <summary>
/// View model for estimation breakdown (new dynamic model)
/// </summary>
public class EstimationBreakdownNewViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    
    public List<EstimationPhaseItemViewModel> Phases { get; set; } = new();
    
    public decimal TotalHours => Phases.Sum(p => p.Hours);
}

/// <summary>
/// Single phase in estimation breakdown
/// </summary>
public class EstimationPhaseItemViewModel
{
    public string WorkPhaseId { get; set; } = string.Empty;
    public string WorkPhaseName { get; set; } = string.Empty;
    public string WorkPhaseCode { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal Hours { get; set; }
    public string? Notes { get; set; }
}
