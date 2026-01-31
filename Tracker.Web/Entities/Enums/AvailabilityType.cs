namespace Tracker.Web.Entities.Enums;

/// <summary>
/// Types of resource unavailability or schedule modifications
/// </summary>
public enum AvailabilityType
{
    /// <summary>
    /// Paid time off, vacation, personal days
    /// </summary>
    PTO = 0,

    /// <summary>
    /// Sick leave, illness, medical appointments
    /// </summary>
    SickLeave = 1,

    /// <summary>
    /// Company holiday (typically set at company level)
    /// </summary>
    Holiday = 2,

    /// <summary>
    /// Training, certifications, learning
    /// </summary>
    Training = 3,

    /// <summary>
    /// Jury duty or other legal obligations
    /// </summary>
    JuryDuty = 4,

    /// <summary>
    /// Maternity, paternity, or family leave
    /// </summary>
    ParentalLeave = 5,

    /// <summary>
    /// Unpaid leave of absence, sabbatical
    /// </summary>
    UnpaidLeave = 6,

    /// <summary>
    /// Working from home (not unavailability, but useful for tracking)
    /// </summary>
    WorkFromHome = 7,

    /// <summary>
    /// Partial day unavailability (doctor appointment, etc.)
    /// </summary>
    PartialDay = 8,

    /// <summary>
    /// Time blocked/reserved for specific project or work
    /// </summary>
    Blocked = 9,

    /// <summary>
    /// Company event, all-hands, offsite meeting
    /// </summary>
    CompanyEvent = 10,

    /// <summary>
    /// Bereavement leave
    /// </summary>
    Bereavement = 11,

    /// <summary>
    /// Military leave
    /// </summary>
    MilitaryLeave = 12
}

/// <summary>
/// Extension methods for AvailabilityType
/// </summary>
public static class AvailabilityTypeExtensions
{
    public static string ToDisplayString(this AvailabilityType type) => type switch
    {
        AvailabilityType.PTO => "PTO / Vacation",
        AvailabilityType.SickLeave => "Sick Leave",
        AvailabilityType.Holiday => "Holiday",
        AvailabilityType.Training => "Training",
        AvailabilityType.JuryDuty => "Jury Duty",
        AvailabilityType.ParentalLeave => "Parental Leave",
        AvailabilityType.UnpaidLeave => "Unpaid Leave",
        AvailabilityType.WorkFromHome => "Work From Home",
        AvailabilityType.PartialDay => "Partial Day",
        AvailabilityType.Blocked => "Blocked",
        AvailabilityType.CompanyEvent => "Company Event",
        AvailabilityType.Bereavement => "Bereavement",
        AvailabilityType.MilitaryLeave => "Military Leave",
        _ => type.ToString()
    };

    public static string ToBadgeClass(this AvailabilityType type) => type switch
    {
        AvailabilityType.PTO => "bg-info",
        AvailabilityType.SickLeave => "bg-warning",
        AvailabilityType.Holiday => "bg-success",
        AvailabilityType.Training => "bg-primary",
        AvailabilityType.JuryDuty => "bg-secondary",
        AvailabilityType.ParentalLeave => "bg-info",
        AvailabilityType.UnpaidLeave => "bg-secondary",
        AvailabilityType.WorkFromHome => "bg-light text-dark",
        AvailabilityType.PartialDay => "bg-warning",
        AvailabilityType.Blocked => "bg-dark",
        AvailabilityType.CompanyEvent => "bg-primary",
        AvailabilityType.Bereavement => "bg-secondary",
        AvailabilityType.MilitaryLeave => "bg-secondary",
        _ => "bg-secondary"
    };

    /// <summary>
    /// Whether this type reduces available/billable hours
    /// </summary>
    public static bool ReducesAvailability(this AvailabilityType type) => type switch
    {
        AvailabilityType.WorkFromHome => false, // Location change, not unavailability
        _ => true
    };

    /// <summary>
    /// Whether this type is typically paid
    /// </summary>
    public static bool IsPaid(this AvailabilityType type) => type switch
    {
        AvailabilityType.UnpaidLeave => false,
        _ => true
    };
}
