using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Entities.Enums;

namespace Tracker.Web.ViewModels.Availability;

/// <summary>
/// View model for submitting a PTO/time-off request
/// </summary>
public class PtoRequestViewModel
{
    public string? Id { get; set; }
    
    [Required]
    public string ResourceId { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Type")]
    public AvailabilityType Type { get; set; } = AvailabilityType.PTO;
    
    [Required]
    [Display(Name = "Start Date")]
    public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    
    [Required]
    [Display(Name = "End Date")]
    public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
    
    [Display(Name = "Hours Per Day")]
    [Range(0.5, 24, ErrorMessage = "Hours must be between 0.5 and 24")]
    public decimal? HoursPerDay { get; set; }
    
    [Display(Name = "Full Day")]
    public bool IsFullDay { get; set; } = true;
    
    [Display(Name = "Notes")]
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // === DROPDOWN OPTIONS ===
    
    public List<SelectListItem> AvailabilityTypes { get; set; } = new();
    
    // === COMPUTED ===
    
    public bool IsNew => string.IsNullOrEmpty(Id);
    
    public int DayCount => EndDate >= StartDate 
        ? EndDate.DayNumber - StartDate.DayNumber + 1 
        : 0;
    
    /// <summary>
    /// Estimated total hours for this request
    /// </summary>
    public decimal EstimatedTotalHours => DayCount * (HoursPerDay ?? 8m);
    
    public string PageTitle => IsNew ? "Request Time Off" : "Edit Time Off Request";
    
    /// <summary>
    /// Initialize dropdown options
    /// </summary>
    public void InitializeOptions()
    {
        AvailabilityTypes = Enum.GetValues<AvailabilityType>()
            .Where(t => t != AvailabilityType.Holiday) // Holidays are set by admin
            .Select(t => new SelectListItem
            {
                Value = ((int)t).ToString(),
                Text = t.ToDisplayString(),
                Selected = t == Type
            })
            .ToList();
    }
}

/// <summary>
/// View model for listing PTO requests (for approval workflow)
/// </summary>
public class PtoRequestListViewModel
{
    public string? ServiceAreaId { get; set; }
    public string? ServiceAreaName { get; set; }
    
    public ApprovalStatus? StatusFilter { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    
    public List<PtoRequestListItemViewModel> Requests { get; set; } = new();
    
    public int PendingCount => Requests.Count(r => r.Status == ApprovalStatus.Pending);
    public int ApprovedCount => Requests.Count(r => r.Status == ApprovalStatus.Approved);
    public int RejectedCount => Requests.Count(r => r.Status == ApprovalStatus.Rejected);
}

/// <summary>
/// List item for PTO requests
/// </summary>
public class PtoRequestListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    
    public AvailabilityType Type { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public decimal? HoursPerDay { get; set; }
    public string? Notes { get; set; }
    
    public ApprovalStatus Status { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // === COMPUTED ===
    
    public string TypeDisplay => Type.ToDisplayString();
    public string TypeBadgeClass => Type.ToBadgeClass();
    public string StatusDisplay => Status.ToDisplayString();
    public string StatusBadgeClass => Status.ToBadgeClass();
    
    public int DayCount => EndDate.DayNumber - StartDate.DayNumber + 1;
    
    public string DateRangeDisplay => StartDate == EndDate
        ? StartDate.ToString("MMM d, yyyy")
        : $"{StartDate:MMM d} - {EndDate:MMM d, yyyy}";
    
    public string HoursDisplay => HoursPerDay.HasValue
        ? $"{HoursPerDay}h/day ({DayCount * HoursPerDay}h total)"
        : $"Full days ({DayCount * 8}h total)";
    
    public bool CanApprove => Status == ApprovalStatus.Pending;
    public bool CanCancel => Status == ApprovalStatus.Pending || 
                            (Status == ApprovalStatus.Approved && StartDate > DateOnly.FromDateTime(DateTime.Today));
}

/// <summary>
/// View model for approving/rejecting a PTO request
/// </summary>
public class PtoApprovalViewModel
{
    [Required]
    public string RequestId { get; set; } = string.Empty;
    
    [Required]
    public bool Approve { get; set; }
    
    [MaxLength(500)]
    [Display(Name = "Reason (required for rejection)")]
    public string? RejectionReason { get; set; }
}
