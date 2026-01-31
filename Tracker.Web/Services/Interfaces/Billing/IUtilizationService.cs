using Tracker.Web.ViewModels.Billing;

namespace Tracker.Web.Services.Billing;

/// <summary>
/// Service for calculating resource utilization
/// </summary>
public interface IUtilizationService
{
    /// <summary>
    /// Calculate utilization for a single resource
    /// </summary>
    Task<UtilizationResultViewModel> CalculateUtilizationAsync(
        string resourceId, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Calculate utilization for multiple resources
    /// </summary>
    Task<List<UtilizationResultViewModel>> CalculateTeamUtilizationAsync(
        List<string> resourceIds, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Calculate utilization for a service area
    /// </summary>
    Task<UtilizationReportViewModel> GetServiceAreaUtilizationAsync(
        string serviceAreaId, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Calculate utilization for all resources (with optional filters)
    /// </summary>
    Task<UtilizationReportViewModel> GetUtilizationReportAsync(
        DateOnly startDate, 
        DateOnly endDate,
        string? serviceAreaId = null,
        bool activeOnly = true);
    
    /// <summary>
    /// Get utilization trend over time (monthly)
    /// </summary>
    Task<List<UtilizationTrendViewModel>> GetUtilizationTrendAsync(
        string resourceId, int months = 12);
    
    /// <summary>
    /// Get utilization trend for a team/service area
    /// </summary>
    Task<List<UtilizationTrendViewModel>> GetTeamUtilizationTrendAsync(
        string? serviceAreaId, int months = 12);
}

/// <summary>
/// Utilization data point for trend charts
/// </summary>
public class UtilizationTrendViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName => new DateTime(Year, Month, 1).ToString("MMM yyyy");
    
    public decimal AvailableHours { get; set; }
    public decimal BillableHours { get; set; }
    public decimal UtilizationPercent { get; set; }
    public decimal TargetPercent { get; set; }
}
