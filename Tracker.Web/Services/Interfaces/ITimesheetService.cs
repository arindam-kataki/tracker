using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface ITimesheetService
{
    #region Time Entries
    
    /// <summary>
    /// Get all time entries for an enhancement
    /// </summary>
    Task<List<TimeEntry>> GetEntriesForEnhancementAsync(string enhancementId);
    
    /// <summary>
    /// Get all time entries for a resource within a date range.
    /// Uses DateOnly to avoid timezone issues.
    /// </summary>
    Task<List<TimeEntry>> GetEntriesForResourceAsync(string resourceId, DateOnly? startDate = null, DateOnly? endDate = null);
    
    /// <summary>
    /// Get time entries matching filter criteria.
    /// Uses DateOnly to avoid timezone issues.
    /// </summary>
    Task<List<TimeEntry>> GetEntriesAsync(
        string? serviceAreaId = null,
        string? enhancementId = null,
        string? resourceId = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null);
    
    /// <summary>
    /// Get a specific time entry by ID
    /// </summary>
    Task<TimeEntry?> GetEntryByIdAsync(string id);
    
    /// <summary>
    /// Create a new time entry.
    /// Uses DateOnly to avoid timezone issues.
    /// </summary>
    Task<TimeEntry> CreateEntryAsync(
        string enhancementId,
        string resourceId,
        string workPhaseId,
        DateOnly startDate,
        DateOnly endDate,
        decimal hours,
        decimal? contributedHours,
        string? notes,
        string createdByResourceId);
    
    /// <summary>
    /// Update an existing time entry.
    /// Uses DateOnly to avoid timezone issues.
    /// </summary>
    Task<TimeEntry?> UpdateEntryAsync(
        string id,
        string workPhaseId,
        DateOnly startDate,
        DateOnly endDate,
        decimal hours,
        decimal contributedHours,
        string? notes,
        string modifiedByResourceId);
    
    /// <summary>
    /// Delete a time entry
    /// </summary>
    Task<(bool success, string? error)> DeleteEntryAsync(string id);
    
    /// <summary>
    /// Validate time entry data.
    /// Uses DateOnly to avoid timezone issues.
    /// </summary>
    Task<(bool isValid, string? error)> ValidateEntryAsync(
        string enhancementId,
        DateOnly startDate,
        DateOnly endDate,
        decimal hours,
        decimal contributedHours);
    
    #endregion
    
    #region My Timesheet - Permission Based
    
    /// <summary>
    /// Get service areas where the resource has LogTimesheet permission
    /// </summary>
    Task<List<ServiceArea>> GetServiceAreasWithTimesheetPermissionAsync(string resourceId);
    
    /// <summary>
    /// Get enhancements accessible for timesheet entry based on resource's service area permissions
    /// </summary>
    Task<List<Enhancement>> GetEnhancementsForTimesheetAsync(
        string resourceId,
        string? serviceAreaId = null,
        string? search = null);
    
    /// <summary>
    /// Check if a resource has permission to log time against an enhancement
    /// </summary>
    Task<bool> CanLogTimeForEnhancementAsync(string resourceId, string enhancementId);
    
    #endregion
    
    #region Consolidation Support
    
    /// <summary>
    /// Get unconsolidated time entries for an enhancement within a date range
    /// </summary>
    Task<List<TimeEntry>> GetUnconsolidatedEntriesAsync(
        string enhancementId,
        DateOnly startDate,
        DateOnly endDate);
    
    /// <summary>
    /// Get time entries that have been (partially) consolidated
    /// </summary>
    Task<List<TimeEntry>> GetConsolidatedEntriesAsync(
        string enhancementId,
        DateOnly? startDate = null,
        DateOnly? endDate = null);
    
    #endregion
}
