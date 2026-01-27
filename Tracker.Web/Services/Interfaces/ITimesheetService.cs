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
    /// Get all time entries for a resource
    /// </summary>
    Task<List<TimeEntry>> GetEntriesForResourceAsync(string resourceId, DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// Get time entries matching filter criteria
    /// </summary>
    Task<List<TimeEntry>> GetEntriesAsync(
        string? serviceAreaId = null,
        string? enhancementId = null,
        string? resourceId = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    
    /// <summary>
    /// Get a specific time entry by ID
    /// </summary>
    Task<TimeEntry?> GetEntryByIdAsync(string id);
    
    /// <summary>
    /// Create a new time entry
    /// </summary>
    Task<TimeEntry> CreateEntryAsync(
        string enhancementId,
        string resourceId,
        string workPhaseId,
        DateTime startDate,
        DateTime endDate,
        decimal hours,
        decimal? contributedHours,
        string? notes,
        string createdByResourceId);
    
    /// <summary>
    /// Update an existing time entry
    /// </summary>
    Task<TimeEntry?> UpdateEntryAsync(
        string id,
        string workPhaseId,
        DateTime startDate,
        DateTime endDate,
        decimal hours,
        decimal contributedHours,
        string? notes,
        string modifiedByResourceId);
    
    /// <summary>
    /// Delete a time entry
    /// </summary>
    Task<(bool success, string? error)> DeleteEntryAsync(string id);
    
    /// <summary>
    /// Validate time entry data
    /// </summary>
    Task<(bool isValid, string? error)> ValidateEntryAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate,
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
        string? status = null,
        string? workIdSearch = null,
        string? descriptionSearch = null,
        string? tagSearch = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null);
    
    /// <summary>
    /// Check if resource has permission to log time against an enhancement
    /// </summary>
    Task<bool> CanLogTimeForEnhancementAsync(string resourceId, string enhancementId);
    
    /// <summary>
    /// Get timesheet summary for a resource by date range
    /// </summary>
    Task<TimesheetSummary> GetTimesheetSummaryAsync(string resourceId, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get distinct statuses from enhancements for dropdown
    /// </summary>
    Task<List<string>> GetDistinctEnhancementStatusesAsync(string resourceId);
    
    /// <summary>
    /// Get distinct tags from enhancements for dropdown/autocomplete
    /// </summary>
    Task<List<string>> GetDistinctEnhancementTagsAsync(string resourceId);
    
    #endregion
    
    #region Reporting
    
    /// <summary>
    /// Get total hours by work phase for an enhancement
    /// </summary>
    Task<Dictionary<string, decimal>> GetHoursByPhaseForEnhancementAsync(string enhancementId);
    
    /// <summary>
    /// Get total hours by resource for an enhancement
    /// </summary>
    Task<Dictionary<string, decimal>> GetHoursByResourceForEnhancementAsync(string enhancementId);
    
    #endregion
}

/// <summary>
/// Summary of timesheet data for a resource
/// </summary>
public class TimesheetSummary
{
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalContributedHours { get; set; }
    public List<TimeEntry> Entries { get; set; } = new();
    public Dictionary<string, decimal> HoursByPhase { get; set; } = new();
    public Dictionary<string, decimal> HoursByEnhancement { get; set; } = new();
}
