using System.ComponentModel.DataAnnotations;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

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
    
    // Filter
    public DateTime StartDate { get; set; } = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
    public DateTime EndDate { get; set; } = DateTime.Today.AddDays(6 - (int)DateTime.Today.DayOfWeek);
    
    // Data
    public List<TimeEntry> Entries { get; set; } = new();
    public List<Enhancement> AssignedEnhancements { get; set; } = new();
    public List<WorkPhase> WorkPhases { get; set; } = new();
    
    // Summary
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
    
    // Grouping
    public Dictionary<string, List<TimeEntry>> EntriesByDate { get; set; } = new();
    public Dictionary<string, decimal> HoursByPhase { get; set; } = new();
}

/// <summary>
/// View model for creating/editing a time entry
/// </summary>
public class TimeEntryEditViewModel
{
    public string? Id { get; set; }
    
    [Required]
    public string EnhancementId { get; set; } = string.Empty;
    
    public string ResourceId { get; set; } = string.Empty;
    
    [Required]
    public string WorkPhaseId { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; } = DateTime.Today;
    
    [Required]
    public DateTime EndDate { get; set; } = DateTime.Today;
    
    [Required]
    [Range(0.01, 999.99, ErrorMessage = "Hours must be between 0.01 and 999.99")]
    public decimal Hours { get; set; }
    
    [Range(0, 999.99, ErrorMessage = "Contributed hours must be between 0 and 999.99")]
    public decimal ContributedHours { get; set; }
    
    public string? Notes { get; set; }
    
    // For dropdowns
    public List<Enhancement> AvailableEnhancements { get; set; } = new();
    public List<WorkPhase> AvailableWorkPhases { get; set; } = new();
    
    // Display info
    public string? EnhancementWorkId { get; set; }
    public string? WorkPhaseName { get; set; }
    public bool IsConsolidated { get; set; }
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
    
    // Available enhancements (with time entries in date range)
    public List<EnhancementWithTimeEntries> AvailableEnhancements { get; set; } = new();
    
    // Step 3: Time entries with pull amounts
    public List<TimeEntryForConsolidationViewModel> TimeEntries { get; set; } = new();
    
    // Step 4: Final billable hours
    public decimal BillableHours { get; set; }
    public string? Notes { get; set; }
    
    // For dropdowns
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
}

/// <summary>
/// Time entry with pull amount for consolidation UI
/// </summary>
public class TimeEntryForConsolidationViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string WorkPhaseName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal ContributedHours { get; set; }
    public decimal AlreadyPulledHours { get; set; }
    public decimal AvailableHours { get; set; }
    public decimal PullHours { get; set; } // User input
    public string? Notes { get; set; }
    public bool IsSelected { get; set; }
    
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
    [Range(0.01, 9999.99, ErrorMessage = "Billable hours must be greater than 0")]
    public decimal BillableHours { get; set; }
    
    [Required(ErrorMessage = "Notes are required for manual consolidation")]
    public string Notes { get; set; } = string.Empty;
    
    // For dropdowns
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
    public string EnhancementDescription { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BillableHours { get; set; }
    public decimal SourceHours { get; set; }
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


// ============================================================
// ESTIMATION VS ACTUAL VIEW MODEL
// ============================================================

/// <summary>
/// View model for estimation vs actual comparison
/// </summary>
public class EstimationVsActualViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public decimal TotalEstimated { get; set; }
    public decimal TotalActual { get; set; }
    public decimal TotalContributed { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal Variance { get; set; }
    public decimal VariancePercent { get; set; }
    
    public List<PhaseComparisonViewModel> ByPhase { get; set; } = new();
    
    public string VarianceClass => Variance switch
    {
        > 0 => "text-danger",
        < 0 => "text-success",
        _ => ""
    };
}

/// <summary>
/// Per-phase comparison
/// </summary>
public class PhaseComparisonViewModel
{
    public string WorkPhaseId { get; set; } = string.Empty;
    public string WorkPhaseName { get; set; } = string.Empty;
    public decimal Estimated { get; set; }
    public decimal Actual { get; set; }
    public decimal Contributed { get; set; }
    public decimal Billed { get; set; }
    public decimal Variance { get; set; }
    
    public string VarianceClass => Variance switch
    {
        > 0 => "text-danger",
        < 0 => "text-success",
        _ => ""
    };
}
