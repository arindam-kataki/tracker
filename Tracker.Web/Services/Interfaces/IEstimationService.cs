using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IEstimationService
{
    /// <summary>
    /// Get estimation breakdown items for an enhancement
    /// </summary>
    Task<List<EstimationBreakdownItem>> GetBreakdownAsync(string enhancementId);
    
    /// <summary>
    /// Get estimation breakdown with work phases (including phases with no hours)
    /// </summary>
    Task<List<EstimationPhaseViewModel>> GetBreakdownWithPhasesAsync(string enhancementId);
    
    /// <summary>
    /// Save estimation breakdown (replaces all items)
    /// </summary>
    Task SaveBreakdownAsync(string enhancementId, List<EstimationPhaseInput> items);
    
    /// <summary>
    /// Get total estimated hours for an enhancement
    /// </summary>
    Task<decimal> GetTotalEstimatedHoursAsync(string enhancementId);
    
    /// <summary>
    /// Copy estimation from one enhancement to another
    /// </summary>
    Task CopyBreakdownAsync(string sourceEnhancementId, string targetEnhancementId);
    
    /// <summary>
    /// Delete all estimation items for an enhancement
    /// </summary>
    Task ClearBreakdownAsync(string enhancementId);
    
    /// <summary>
    /// Get estimation vs actual comparison
    /// </summary>
    Task<EstimationVsActualReport> GetEstimationVsActualAsync(string enhancementId);
}

/// <summary>
/// View model for estimation phase with hours
/// </summary>
public class EstimationPhaseViewModel
{
    public string WorkPhaseId { get; set; } = string.Empty;
    public string WorkPhaseName { get; set; } = string.Empty;
    public string WorkPhaseCode { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public decimal Hours { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Input for saving estimation phase
/// </summary>
public class EstimationPhaseInput
{
    public string WorkPhaseId { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Estimation vs actual hours comparison
/// </summary>
public class EstimationVsActualReport
{
    public string EnhancementId { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public decimal TotalEstimated { get; set; }
    public decimal TotalActual { get; set; }
    public decimal TotalContributed { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal Variance => TotalActual - TotalEstimated;
    public decimal VariancePercent => TotalEstimated > 0 
        ? Math.Round((Variance / TotalEstimated) * 100, 1) 
        : 0;
    public List<PhaseComparison> ByPhase { get; set; } = new();
}

/// <summary>
/// Per-phase comparison
/// </summary>
public class PhaseComparison
{
    public string WorkPhaseId { get; set; } = string.Empty;
    public string WorkPhaseName { get; set; } = string.Empty;
    public decimal Estimated { get; set; }
    public decimal Actual { get; set; }
    public decimal Contributed { get; set; }
    public decimal Billed { get; set; }
    public decimal Variance => Actual - Estimated;
}
