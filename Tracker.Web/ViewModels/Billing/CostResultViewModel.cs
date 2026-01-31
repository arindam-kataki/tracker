using Tracker.Web.Entities.Enums;

namespace Tracker.Web.ViewModels.Billing;

/// <summary>
/// Result of cost calculation for a resource's time
/// </summary>
public class CostResultViewModel
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public ExemptionStatus ExemptionStatus { get; set; }
    
    public DateOnly WeekStartDate { get; set; }
    public DateOnly WeekEndDate { get; set; }
    
    /// <summary>
    /// Total hours worked in the period
    /// </summary>
    public decimal TotalHours { get; set; }
    
    /// <summary>
    /// Regular (non-overtime) hours
    /// </summary>
    public decimal RegularHours { get; set; }
    
    /// <summary>
    /// Overtime hours (over standard weekly hours)
    /// </summary>
    public decimal OvertimeHours { get; set; }
    
    /// <summary>
    /// Standard cost rate per hour
    /// </summary>
    public decimal CostRate { get; set; }
    
    /// <summary>
    /// Overtime rate (CostRate * OvertimeMultiplier)
    /// </summary>
    public decimal OvertimeRate { get; set; }
    
    /// <summary>
    /// Cost for regular hours
    /// </summary>
    public decimal RegularCost { get; set; }
    
    /// <summary>
    /// Cost for overtime hours
    /// </summary>
    public decimal OvertimeCost { get; set; }
    
    /// <summary>
    /// Total cost (Regular + Overtime)
    /// </summary>
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    // === COMPUTED ===
    
    public string PeriodDisplay => $"{WeekStartDate:MMM d} - {WeekEndDate:MMM d, yyyy}";
    
    public bool HasOvertime => OvertimeHours > 0;
    
    /// <summary>
    /// Effective average rate (TotalCost / TotalHours)
    /// </summary>
    public decimal EffectiveRate => TotalHours > 0 
        ? Math.Round(TotalCost / TotalHours, 2) 
        : 0;
    
    /// <summary>
    /// Format cost with currency
    /// </summary>
    public string TotalCostDisplay => $"{Currency} {TotalCost:N2}";
    public string RegularCostDisplay => $"{Currency} {RegularCost:N2}";
    public string OvertimeCostDisplay => $"{Currency} {OvertimeCost:N2}";
}

/// <summary>
/// View model for cost report
/// </summary>
public class CostReportViewModel
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? ServiceAreaId { get; set; }
    public string? ServiceAreaName { get; set; }
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Cost breakdown by resource
    /// </summary>
    public List<CostResultViewModel> ResourceCosts { get; set; } = new();
    
    /// <summary>
    /// Summary totals
    /// </summary>
    public CostSummaryViewModel Summary { get; set; } = new();
    
    public string PeriodDisplay => $"{StartDate:MMM d} - {EndDate:MMM d, yyyy}";
}

/// <summary>
/// Summary of costs
/// </summary>
public class CostSummaryViewModel
{
    public decimal TotalHours { get; set; }
    public decimal TotalRegularHours { get; set; }
    public decimal TotalOvertimeHours { get; set; }
    
    public decimal TotalRegularCost { get; set; }
    public decimal TotalOvertimeCost { get; set; }
    public decimal TotalCost { get; set; }
    
    public string Currency { get; set; } = "USD";
    
    public int ResourceCount { get; set; }
    public int ResourcesWithOvertime { get; set; }
    
    /// <summary>
    /// Overtime as percentage of total cost
    /// </summary>
    public decimal OvertimeCostPercent => TotalCost > 0
        ? Math.Round(TotalOvertimeCost / TotalCost * 100, 1)
        : 0;
    
    /// <summary>
    /// Average cost per hour
    /// </summary>
    public decimal AverageHourlyCost => TotalHours > 0
        ? Math.Round(TotalCost / TotalHours, 2)
        : 0;
    
    public string TotalCostDisplay => $"{Currency} {TotalCost:N2}";
}
