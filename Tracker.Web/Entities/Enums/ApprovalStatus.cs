namespace Tracker.Web.Entities.Enums;

/// <summary>
/// Status of approval workflows (PTO requests, timesheets, etc.)
/// </summary>
public enum ApprovalStatus
{
    /// <summary>
    /// Request submitted, awaiting review
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Request approved by manager/approver
    /// </summary>
    Approved = 1,

    /// <summary>
    /// Request rejected by manager/approver
    /// </summary>
    Rejected = 2,

    /// <summary>
    /// Request cancelled by the requester
    /// </summary>
    Cancelled = 3
}

/// <summary>
/// Extension methods for ApprovalStatus
/// </summary>
public static class ApprovalStatusExtensions
{
    public static string ToDisplayString(this ApprovalStatus status) => status switch
    {
        ApprovalStatus.Pending => "Pending",
        ApprovalStatus.Approved => "Approved",
        ApprovalStatus.Rejected => "Rejected",
        ApprovalStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };

    public static string ToBadgeClass(this ApprovalStatus status) => status switch
    {
        ApprovalStatus.Pending => "bg-warning text-dark",
        ApprovalStatus.Approved => "bg-success",
        ApprovalStatus.Rejected => "bg-danger",
        ApprovalStatus.Cancelled => "bg-secondary",
        _ => "bg-secondary"
    };

    public static string ToIcon(this ApprovalStatus status) => status switch
    {
        ApprovalStatus.Pending => "bi-clock",
        ApprovalStatus.Approved => "bi-check-circle",
        ApprovalStatus.Rejected => "bi-x-circle",
        ApprovalStatus.Cancelled => "bi-slash-circle",
        _ => "bi-question-circle"
    };

    /// <summary>
    /// Whether this status is a final state (no more changes expected)
    /// </summary>
    public static bool IsFinal(this ApprovalStatus status) => status switch
    {
        ApprovalStatus.Pending => false,
        _ => true
    };
}
