using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IWorkPhaseService
{
    /// <summary>
    /// Get all work phases, optionally filtered
    /// </summary>
    Task<List<WorkPhase>> GetAllAsync(bool activeOnly = true);
    
    /// <summary>
    /// Get work phases for estimation breakdown
    /// </summary>
    Task<List<WorkPhase>> GetForEstimationAsync();
    
    /// <summary>
    /// Get work phases for time recording
    /// </summary>
    Task<List<WorkPhase>> GetForTimeRecordingAsync();
    
    /// <summary>
    /// Get work phases for consolidation
    /// </summary>
    Task<List<WorkPhase>> GetForConsolidationAsync();
    
    /// <summary>
    /// Get a specific work phase by ID
    /// </summary>
    Task<WorkPhase?> GetByIdAsync(string id);
    
    /// <summary>
    /// Get a specific work phase by code
    /// </summary>
    Task<WorkPhase?> GetByCodeAsync(string code);
    
    /// <summary>
    /// Create a new work phase
    /// </summary>
    Task<WorkPhase> CreateAsync(string name, string code, string? description, 
        int defaultContributionPercent, int displayOrder,
        bool forEstimation, bool forTimeRecording, bool forConsolidation);
    
    /// <summary>
    /// Update an existing work phase
    /// </summary>
    Task<WorkPhase?> UpdateAsync(string id, string name, string code, string? description,
        int defaultContributionPercent, int displayOrder, bool isActive,
        bool forEstimation, bool forTimeRecording, bool forConsolidation);
    
    /// <summary>
    /// Delete a work phase (only if not in use)
    /// </summary>
    Task<(bool success, string? error)> DeleteAsync(string id);
    
    /// <summary>
    /// Seed default work phases if none exist
    /// </summary>
    Task SeedDefaultPhasesAsync();
}
