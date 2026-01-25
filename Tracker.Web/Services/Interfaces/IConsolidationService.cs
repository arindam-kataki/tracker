using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IConsolidationService
{
    #region Consolidation CRUD
    
    /// <summary>
    /// Get all consolidations with optional filters
    /// </summary>
    Task<List<Consolidation>> GetConsolidationsAsync(
        string? serviceAreaId = null,
        string? enhancementId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        ConsolidationStatus? status = null);
    
    /// <summary>
    /// Get a specific consolidation by ID
    /// </summary>
    Task<Consolidation?> GetByIdAsync(string id);
    
    /// <summary>
    /// Create a consolidation from timesheet entries
    /// </summary>
    Task<Consolidation> CreateFromTimesheetsAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate,
        List<ConsolidationSourceInput> sources,
        decimal billableHours,
        string? notes,
        string userId);
    
    /// <summary>
    /// Create a manual consolidation (no timesheet sources)
    /// </summary>
    Task<Consolidation> CreateManualAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate,
        decimal billableHours,
        string notes,
        string userId);
    
    /// <summary>
    /// Update a consolidation (only if Draft)
    /// </summary>
    Task<Consolidation?> UpdateAsync(
        string id,
        List<ConsolidationSourceInput>? sources,
        decimal billableHours,
        string? notes,
        string userId);
    
    /// <summary>
    /// Delete a consolidation (only if Draft)
    /// </summary>
    Task<(bool success, string? error)> DeleteAsync(string id);
    
    /// <summary>
    /// Change consolidation status
    /// </summary>
    Task<(bool success, string? error)> ChangeStatusAsync(string id, ConsolidationStatus newStatus, string userId);
    
    #endregion
    
    #region Timesheet Entry Loading
    
    /// <summary>
    /// Get timesheet entries available for consolidation
    /// </summary>
    Task<List<TimeEntryForConsolidation>> GetAvailableEntriesAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate);
    
    /// <summary>
    /// Get enhancements with timesheet entries in the given date range
    /// </summary>
    Task<List<EnhancementWithTimeEntries>> GetEnhancementsWithEntriesAsync(
        string? serviceAreaId,
        DateTime startDate,
        DateTime endDate);
    
    #endregion
    
    #region Validation
    
    /// <summary>
    /// Validate consolidation date range (must be same month)
    /// </summary>
    (bool isValid, string? error) ValidateDateRange(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Validate consolidation data
    /// </summary>
    Task<(bool isValid, string? error)> ValidateConsolidationAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate,
        decimal billableHours,
        bool hasNotes,
        bool hasSources);
    
    #endregion
    
    #region Reporting
    
    /// <summary>
    /// Get consolidation summary by enhancement
    /// </summary>
    Task<List<ConsolidationSummary>> GetSummaryByEnhancementAsync(
        string? serviceAreaId,
        DateTime? startDate,
        DateTime? endDate);
    
    /// <summary>
    /// Get consolidation summary by service area
    /// </summary>
    Task<List<ConsolidationSummary>> GetSummaryByServiceAreaAsync(
        DateTime? startDate,
        DateTime? endDate);
    
    #endregion
}

/// <summary>
/// Input for creating/updating consolidation sources
/// </summary>
public class ConsolidationSourceInput
{
    public string TimeEntryId { get; set; } = string.Empty;
    public decimal PulledHours { get; set; }
}

/// <summary>
/// TimeEntry with additional info for consolidation UI
/// </summary>
public class TimeEntryForConsolidation
{
    public string Id { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string WorkPhaseName { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public decimal ContributedHours { get; set; }
    public decimal AlreadyPulledHours { get; set; }
    public decimal AvailableHours { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Enhancement with summary of timesheet entries
/// </summary>
public class EnhancementWithTimeEntries
{
    public string Id { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public int EntryCount { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
}

/// <summary>
/// Summary of consolidations
/// </summary>
public class ConsolidationSummary
{
    public string GroupKey { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public int ConsolidationCount { get; set; }
    public decimal TotalBillableHours { get; set; }
    public decimal TotalSourceHours { get; set; }
}
