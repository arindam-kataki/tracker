namespace Tracker.Web.Entities;

/// <summary>
/// Represents estimated hours for a specific work phase on an enhancement.
/// Replaces the old hardcoded EstimationBreakdown columns with a flexible structure.
/// </summary>
public class EstimationBreakdownItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string EnhancementId { get; set; } = string.Empty;
    
    public string WorkPhaseId { get; set; } = string.Empty;
    
    /// <summary>
    /// Estimated hours for this phase
    /// </summary>
    public decimal Hours { get; set; }
    
    /// <summary>
    /// Optional notes for this phase estimate
    /// </summary>
    public string? Notes { get; set; }
    
    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual WorkPhase WorkPhase { get; set; } = null!;
}
