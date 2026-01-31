using Tracker.Web.Entities.Enums;

namespace Tracker.Web.ViewModels.Billing;

/// <summary>
/// Result of utilization calculation for a resource
/// </summary>
public class UtilizationResultViewModel
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public string? ServiceAreaCode { get; set; }
    
    public ExemptionStatus ExemptionStatus { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    
    /// <summary>
    /// Standard scheduled hours in the period (before PTO/holidays)
    /// </summary>
    public decimal ScheduledHours { get; set; }
    
    /// <summary>
    /// Available hours after PTO, holidays, etc.
    /// </summary>
    public decimal AvailableHours { get; set; }
    
    /// <summary>
    /// Actual hours logged (may exceed available for non-exempt)
    /// </summary>
    public decimal TotalHoursWorked { get; set; }
    
    /// <summary>
    /// Hours logged to billable work
    /// </summary>
    public decimal BillableHours { get; set; }
    
    /// <summary>
    /// Hours logged to non-billable work (internal, admin, etc.)
    /// </summary>
    public decimal NonBillableHours { get; set; }
    
    /// <summary>
    /// Hours worked over standard (for overtime tracking)
    /// </summary>
    public decimal OvertimeHours { get; set; }
    
    /// <summary>
    /// The denominator used for utilization calculation
    /// Exempt: AvailableHours, NonExempt: TotalHoursWorked
    /// </summary>
    public decimal DenominatorHours { get; set; }
    
    /// <summary>
    /// Calculated utilization percentage
    /// </summary>
    public decimal UtilizationPercent { get; set; }
    
    /// <summary>
    /// Target utilization for this resource
    /// </summary>
    public decimal TargetPercent { get; set; }
    
    /// <summary>
    /// Variance from target (positive = exceeding, negative = below)
    /// </summary>
    public decimal VariancePercent { get; set; }
    
    // === COMPUTED ===
    
    public string ExemptionStatusDisplay => ExemptionStatus.ToDisplayString();
    
    public string PeriodDisplay => $"{StartDate:MMM d} - {EndDate:MMM d, yyyy}";
    
    /// <summary>
    /// Whether utilization meets or exceeds target
    /// </summary>
    public bool MeetsTarget => UtilizationPercent >= TargetPercent;
    
    /// <summary>
    /// CSS class for utilization display
    /// </summary>
    public string UtilizationCssClass
    {
        get
        {
            if (UtilizationPercent >= TargetPercent) return "text-success";
            if (UtilizationPercent >= TargetPercent * 0.8m) return "text-warning";
            return "text-danger";
        }
    }
    
    /// <summary>
    /// Badge class for utilization
    /// </summary>
    public string UtilizationBadgeClass
    {
        get
        {
            if (UtilizationPercent >= TargetPercent) return "bg-success";
            if (UtilizationPercent >= TargetPercent * 0.8m) return "bg-warning text-dark";
            return "bg-danger";
        }
    }
    
    /// <summary>
    /// Progress bar percentage (capped at 100 for display)
    /// </summary>
    public decimal ProgressBarPercent => Math.Min(100, UtilizationPercent);
    
    /// <summary>
    /// Progress bar class based on utilization
    /// </summary>
    public string ProgressBarClass
    {
        get
        {
            if (UtilizationPercent >= TargetPercent) return "bg-success";
            if (UtilizationPercent >= TargetPercent * 0.8m) return "bg-warning";
            return "bg-danger";
        }
    }
}

/// <summary>
/// View model for utilization report page
/// </summary>
public class UtilizationReportViewModel
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? ServiceAreaId { get; set; }
    public string? ServiceAreaName { get; set; }
    
    /// <summary>
    /// Individual resource utilization results
    /// </summary>
    public List<UtilizationResultViewModel> Resources { get; set; } = new();
    
    /// <summary>
    /// Team/aggregate summary
    /// </summary>
    public UtilizationSummaryViewModel Summary { get; set; } = new();
    
    public string PeriodDisplay => $"{StartDate:MMM d} - {EndDate:MMM d, yyyy}";
}

/// <summary>
/// Summary statistics for utilization
/// </summary>
public class UtilizationSummaryViewModel
{
    public decimal TotalAvailableHours { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal TotalNonBillableHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    
    public decimal AverageUtilization { get; set; }
    public decimal AverageTarget { get; set; }
    
    public int ResourceCount { get; set; }
    public int MeetingTargetCount { get; set; }
    public int BelowTargetCount { get; set; }
    
    public decimal MeetingTargetPercent => ResourceCount > 0
        ? Math.Round((decimal)MeetingTargetCount / ResourceCount * 100, 1)
        : 0;
}
