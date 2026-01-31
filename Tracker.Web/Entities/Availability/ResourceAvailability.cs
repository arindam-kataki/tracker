using Tracker.Web.Entities.Enums;

namespace Tracker.Web.Entities.Availability;

/// <summary>
/// Tracks resource unavailability (PTO, sick leave, training, etc.)
/// </summary>
public class ResourceAvailability
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The resource this availability entry belongs to
    /// </summary>
    public string ResourceId { get; set; } = string.Empty;
    public virtual Resource? Resource { get; set; }

    /// <summary>
    /// Type of unavailability
    /// </summary>
    public AvailabilityType Type { get; set; }

    /// <summary>
    /// Start date of the unavailability (inclusive)
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// End date of the unavailability (inclusive)
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Hours per day unavailable. 
    /// Null means full day based on resource's standard hours.
    /// Use for partial days (e.g., 2 hours for doctor appointment).
    /// </summary>
    public decimal? HoursPerDay { get; set; }

    /// <summary>
    /// Whether this is a recurring entry
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// Recurrence pattern (if IsRecurring is true)
    /// </summary>
    public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;

    /// <summary>
    /// End date for recurrence (null = no end)
    /// </summary>
    public DateOnly? RecurrenceEndDate { get; set; }

    /// <summary>
    /// Optional notes/reason for the unavailability
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Approval status for requests that require approval
    /// </summary>
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Approved;

    /// <summary>
    /// Who approved/rejected the request
    /// </summary>
    public string? ApprovedById { get; set; }
    public virtual Resource? ApprovedBy { get; set; }

    /// <summary>
    /// When the request was approved/rejected
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Rejection reason (if Status = Rejected)
    /// </summary>
    public string? RejectionReason { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedById { get; set; }

    // === COMPUTED PROPERTIES ===

    /// <summary>
    /// Number of days in this entry (inclusive)
    /// </summary>
    public int DayCount => EndDate.DayNumber - StartDate.DayNumber + 1;

    /// <summary>
    /// Display string for the date range
    /// </summary>
    public string DateRangeDisplay => StartDate == EndDate
        ? StartDate.ToString("MMM d, yyyy")
        : $"{StartDate:MMM d} - {EndDate:MMM d, yyyy}";

    /// <summary>
    /// Whether this entry is currently active (today falls within range)
    /// </summary>
    public bool IsActive
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return StartDate <= today && EndDate >= today && Status == ApprovalStatus.Approved;
        }
    }

    /// <summary>
    /// Whether this entry is in the future
    /// </summary>
    public bool IsFuture => StartDate > DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Whether this entry is in the past
    /// </summary>
    public bool IsPast => EndDate < DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// Type display string
    /// </summary>
    public string TypeDisplay => Type.ToDisplayString();

    /// <summary>
    /// Status display string
    /// </summary>
    public string StatusDisplay => Status.ToDisplayString();

    /// <summary>
    /// Badge class for the type
    /// </summary>
    public string TypeBadgeClass => Type.ToBadgeClass();

    /// <summary>
    /// Badge class for the status
    /// </summary>
    public string StatusBadgeClass => Status.ToBadgeClass();
}
