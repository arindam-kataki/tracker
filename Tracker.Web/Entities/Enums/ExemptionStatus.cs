namespace Tracker.Web.Entities.Enums;

/// <summary>
/// FLSA exemption status for overtime calculation purposes
/// </summary>
public enum ExemptionStatus
{
    /// <summary>
    /// Exempt from overtime (salaried employees)
    /// Utilization calculated against standard hours, no OT premium
    /// </summary>
    Exempt = 0,

    /// <summary>
    /// Non-exempt (hourly employees, eligible for overtime)
    /// Utilization calculated against actual hours, OT paid at premium rate
    /// </summary>
    NonExempt = 1
}

/// <summary>
/// Extension methods for ExemptionStatus
/// </summary>
public static class ExemptionStatusExtensions
{
    public static string ToDisplayString(this ExemptionStatus status) => status switch
    {
        ExemptionStatus.Exempt => "Exempt (Salaried)",
        ExemptionStatus.NonExempt => "Non-Exempt (Hourly)",
        _ => status.ToString()
    };

    public static string ToShortString(this ExemptionStatus status) => status switch
    {
        ExemptionStatus.Exempt => "Exempt",
        ExemptionStatus.NonExempt => "Non-Exempt",
        _ => status.ToString()
    };

    public static string ToBadgeClass(this ExemptionStatus status) => status switch
    {
        ExemptionStatus.Exempt => "bg-primary",
        ExemptionStatus.NonExempt => "bg-info",
        _ => "bg-secondary"
    };

    /// <summary>
    /// Whether overtime premium applies for this status
    /// </summary>
    public static bool HasOvertimePremium(this ExemptionStatus status) => status switch
    {
        ExemptionStatus.NonExempt => true,
        _ => false
    };
}
