using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class TimesheetService : ITimesheetService
{
    private readonly TrackerDbContext _db;
    private readonly IWorkPhaseService _workPhaseService;
    private readonly ILogger<TimesheetService> _logger;

    public TimesheetService(
        TrackerDbContext db,
        IWorkPhaseService workPhaseService,
        ILogger<TimesheetService> logger)
    {
        _db = db;
        _workPhaseService = workPhaseService;
        _logger = logger;
    }

    #region Time Entries

    public async Task<List<TimeEntry>> GetEntriesForEnhancementAsync(string enhancementId)
    {
        return await _db.TimeEntries
            .Include(te => te.Resource)
            .Include(te => te.WorkPhase)
            .Where(te => te.EnhancementId == enhancementId)
            .OrderByDescending(te => te.StartDate)
            .ToListAsync();
    }

    public async Task<List<TimeEntry>> GetEntriesForResourceAsync(string resourceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.TimeEntries
            .Include(te => te.Enhancement)
                .ThenInclude(e => e.ServiceArea)
            .Include(te => te.WorkPhase)
            .Include(te => te.ConsolidationSources)
            .Where(te => te.ResourceId == resourceId);

        if (startDate.HasValue)
            query = query.Where(te => te.EndDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(te => te.StartDate <= endDate.Value);

        return await query
            .OrderByDescending(te => te.StartDate)
            .ThenBy(te => te.Enhancement.WorkId)
            .ToListAsync();
    }

    public async Task<List<TimeEntry>> GetEntriesAsync(
        string? serviceAreaId = null,
        string? enhancementId = null,
        string? resourceId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var query = _db.TimeEntries
            .Include(te => te.Enhancement)
                .ThenInclude(e => e.ServiceArea)
            .Include(te => te.Resource)
            .Include(te => te.WorkPhase)
            .AsQueryable();

        if (!string.IsNullOrEmpty(serviceAreaId))
            query = query.Where(te => te.Enhancement.ServiceAreaId == serviceAreaId);

        if (!string.IsNullOrEmpty(enhancementId))
            query = query.Where(te => te.EnhancementId == enhancementId);

        if (!string.IsNullOrEmpty(resourceId))
            query = query.Where(te => te.ResourceId == resourceId);

        if (startDate.HasValue)
            query = query.Where(te => te.EndDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(te => te.StartDate <= endDate.Value);

        return await query
            .OrderByDescending(te => te.StartDate)
            .ThenBy(te => te.Resource.Name)
            .ToListAsync();
    }

    public async Task<TimeEntry?> GetEntryByIdAsync(string id)
    {
        return await _db.TimeEntries
            .Include(te => te.Enhancement)
                .ThenInclude(e => e.ServiceArea)
            .Include(te => te.Resource)
            .Include(te => te.WorkPhase)
            .Include(te => te.ConsolidationSources)
            .FirstOrDefaultAsync(te => te.Id == id);
    }

    public async Task<TimeEntry> CreateEntryAsync(
        string enhancementId,
        string resourceId,
        string workPhaseId,
        DateTime startDate,
        DateTime endDate,
        decimal hours,
        decimal? contributedHours,
        string? notes,
        string createdByResourceId)
    {
        // Get work phase for default contribution
        var workPhase = await _workPhaseService.GetByIdAsync(workPhaseId);
        
        // Calculate contributed hours if not provided
        var actualContributedHours = contributedHours ?? 
            (hours * (workPhase?.DefaultContributionPercent ?? 100) / 100);

        var entry = new TimeEntry
        {
            EnhancementId = enhancementId,
            ResourceId = resourceId,
            WorkPhaseId = workPhaseId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Hours = hours,
            ContributedHours = actualContributedHours,
            Notes = notes,
            CreatedById = createdByResourceId,
            CreatedAt = DateTime.UtcNow
        };

        _db.TimeEntries.Add(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Time entry created: {Id} for enhancement {Enhancement} by resource {Resource}", 
            entry.Id, enhancementId, resourceId);

        // Reload with navigation properties
        return (await GetEntryByIdAsync(entry.Id))!;
    }

    public async Task<TimeEntry?> UpdateEntryAsync(
        string id,
        string workPhaseId,
        DateTime startDate,
        DateTime endDate,
        decimal hours,
        decimal contributedHours,
        string? notes,
        string modifiedByResourceId)
    {
        var entry = await _db.TimeEntries
            .Include(te => te.ConsolidationSources)
            .FirstOrDefaultAsync(te => te.Id == id);

        if (entry == null)
            return null;

        // Check if entry has been consolidated
        if (entry.ConsolidationSources.Any())
        {
            // Only allow notes update if consolidated
            entry.Notes = notes;
            entry.ModifiedById = modifiedByResourceId;
            entry.ModifiedAt = DateTime.UtcNow;
            _logger.LogInformation("Time entry {Id} (consolidated) notes updated by {Resource}", id, modifiedByResourceId);
        }
        else
        {
            entry.WorkPhaseId = workPhaseId;
            entry.StartDate = startDate.Date;
            entry.EndDate = endDate.Date;
            entry.Hours = hours;
            entry.ContributedHours = contributedHours;
            entry.Notes = notes;
            entry.ModifiedById = modifiedByResourceId;
            entry.ModifiedAt = DateTime.UtcNow;
            _logger.LogInformation("Time entry {Id} updated by {Resource}", id, modifiedByResourceId);
        }

        await _db.SaveChangesAsync();

        return await GetEntryByIdAsync(id);
    }

    public async Task<(bool success, string? error)> DeleteEntryAsync(string id)
    {
        var entry = await _db.TimeEntries
            .Include(te => te.ConsolidationSources)
            .FirstOrDefaultAsync(te => te.Id == id);

        if (entry == null)
            return (false, "Time entry not found.");

        if (entry.ConsolidationSources.Any())
            return (false, "Cannot delete time entry that has been used in a consolidation.");

        _db.TimeEntries.Remove(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Time entry {Id} deleted", id);

        return (true, null);
    }

    public Task<(bool isValid, string? error)> ValidateEntryAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate,
        decimal hours,
        decimal contributedHours)
    {
        // End date must be >= start date
        if (endDate < startDate)
            return Task.FromResult<(bool, string?)>((false, "End date must be on or after start date."));

        // Hours must be positive
        if (hours <= 0)
            return Task.FromResult<(bool, string?)>((false, "Hours must be greater than zero."));

        // Contributed hours cannot exceed hours
        if (contributedHours > hours)
            return Task.FromResult<(bool, string?)>((false, "Contributed hours cannot exceed total hours."));

        // Contributed hours cannot be negative
        if (contributedHours < 0)
            return Task.FromResult<(bool, string?)>((false, "Contributed hours cannot be negative."));

        return Task.FromResult<(bool, string?)>((true, null));
    }

    #endregion

    #region My Timesheet - Permission Based

    public async Task<List<ServiceArea>> GetServiceAreasWithTimesheetPermissionAsync(string resourceId)
    {
        var resource = await _db.Resources
            .Include(r => r.ServiceAreas)
                .ThenInclude(rsa => rsa.ServiceArea)
            .FirstOrDefaultAsync(r => r.Id == resourceId);

        if (resource == null)
            return new List<ServiceArea>();

        // SuperAdmin has access to all service areas
        if (resource.IsAdmin)
        {
            return await _db.ServiceAreas
                .Where(sa => sa.IsActive)
                .OrderBy(sa => sa.DisplayOrder)
                .ThenBy(sa => sa.Code)
                .ToListAsync();
        }

        // Regular resources only have access to service areas where they have LogTimesheet permission
        return resource.ServiceAreas
            .Where(rsa => rsa.HasPermission(Permissions.LogTimesheet) && 
                          rsa.ServiceArea != null && 
                          rsa.ServiceArea.IsActive)
            .Select(rsa => rsa.ServiceArea!)
            .OrderBy(sa => sa.DisplayOrder)
            .ThenBy(sa => sa.Code)
            .ToList();
    }

    public async Task<List<Enhancement>> GetEnhancementsForTimesheetAsync(
        string resourceId,
        string? serviceAreaId = null,
        string? status = null,
        string? workIdSearch = null,
        string? descriptionSearch = null,
        string? tagSearch = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null)
    {
        // Get service areas where resource has LogTimesheet permission
        var permittedServiceAreas = await GetServiceAreasWithTimesheetPermissionAsync(resourceId);
        var permittedServiceAreaIds = permittedServiceAreas.Select(sa => sa.Id).ToList();

        if (!permittedServiceAreaIds.Any())
            return new List<Enhancement>();

        var query = _db.Enhancements
            .Include(e => e.ServiceArea)
            .Where(e => permittedServiceAreaIds.Contains(e.ServiceAreaId))
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(serviceAreaId))
            query = query.Where(e => e.ServiceAreaId == serviceAreaId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status || e.InfStatus == status);

        if (!string.IsNullOrEmpty(workIdSearch))
            query = query.Where(e => e.WorkId.Contains(workIdSearch));

        if (!string.IsNullOrEmpty(descriptionSearch))
            query = query.Where(e => e.Description.Contains(descriptionSearch));

        if (!string.IsNullOrEmpty(tagSearch))
            query = query.Where(e => e.Tags != null && e.Tags.Contains(tagSearch));

        // Date range filter - uses actual StartDate/EndDate fields
        if (startDateFrom.HasValue)
            query = query.Where(e => e.StartDate >= startDateFrom.Value || e.EstimatedStartDate >= startDateFrom.Value);

        if (startDateTo.HasValue)
            query = query.Where(e => e.StartDate <= startDateTo.Value || e.EstimatedStartDate <= startDateTo.Value);

        return await query
            .OrderBy(e => e.ServiceArea.Code)
            .ThenBy(e => e.WorkId)
            .Take(500) // Limit results for performance
            .ToListAsync();
    }

    public async Task<bool> CanLogTimeForEnhancementAsync(string resourceId, string enhancementId)
    {
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement == null)
            return false;

        var resource = await _db.Resources
            .Include(r => r.ServiceAreas)
            .FirstOrDefaultAsync(r => r.Id == resourceId);

        if (resource == null)
            return false;

        // SuperAdmin can log time for any enhancement
        if (resource.IsAdmin)
            return true;

        // Check if resource has LogTimesheet permission for the enhancement's service area
        return resource.ServiceAreas
            .Any(rsa => rsa.ServiceAreaId == enhancement.ServiceAreaId && 
                        rsa.HasPermission(Permissions.LogTimesheet));
    }

    public async Task<TimesheetSummary> GetTimesheetSummaryAsync(string resourceId, DateTime startDate, DateTime endDate)
    {
        var resource = await _db.Resources.FindAsync(resourceId);
        var entries = await GetEntriesForResourceAsync(resourceId, startDate, endDate);

        var summary = new TimesheetSummary
        {
            ResourceId = resourceId,
            ResourceName = resource?.Name ?? "Unknown",
            StartDate = startDate,
            EndDate = endDate,
            Entries = entries,
            TotalHours = entries.Sum(e => e.Hours),
            TotalContributedHours = entries.Sum(e => e.ContributedHours)
        };

        // Hours by phase
        summary.HoursByPhase = entries
            .GroupBy(e => e.WorkPhase?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours));

        // Hours by enhancement
        summary.HoursByEnhancement = entries
            .GroupBy(e => e.Enhancement?.WorkId ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours));

        return summary;
    }

    public async Task<List<string>> GetDistinctEnhancementStatusesAsync(string resourceId)
    {
        var permittedServiceAreas = await GetServiceAreasWithTimesheetPermissionAsync(resourceId);
        var permittedServiceAreaIds = permittedServiceAreas.Select(sa => sa.Id).ToList();

        var statuses = await _db.Enhancements
            .Where(e => permittedServiceAreaIds.Contains(e.ServiceAreaId))
            .Select(e => e.Status)
            .Where(s => s != null)
            .Distinct()
            .ToListAsync();

        var infStatuses = await _db.Enhancements
            .Where(e => permittedServiceAreaIds.Contains(e.ServiceAreaId))
            .Select(e => e.InfStatus)
            .Where(s => s != null)
            .Distinct()
            .ToListAsync();

        return statuses.Union(infStatuses!)
            .Where(s => !string.IsNullOrEmpty(s))
            .OrderBy(s => s)
            .ToList()!;
    }

    public async Task<List<string>> GetDistinctEnhancementTagsAsync(string resourceId)
    {
        var permittedServiceAreas = await GetServiceAreasWithTimesheetPermissionAsync(resourceId);
        var permittedServiceAreaIds = permittedServiceAreas.Select(sa => sa.Id).ToList();

        var allTags = await _db.Enhancements
            .Where(e => permittedServiceAreaIds.Contains(e.ServiceAreaId) && e.Tags != null)
            .Select(e => e.Tags)
            .ToListAsync();

        return allTags
            .Where(t => !string.IsNullOrEmpty(t))
            .SelectMany(t => t!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }

    #endregion

    #region Reporting

    public async Task<Dictionary<string, decimal>> GetHoursByPhaseForEnhancementAsync(string enhancementId)
    {
        var entries = await GetEntriesForEnhancementAsync(enhancementId);
        return entries
            .GroupBy(e => e.WorkPhase?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours));
    }

    public async Task<Dictionary<string, decimal>> GetHoursByResourceForEnhancementAsync(string enhancementId)
    {
        var entries = await GetEntriesForEnhancementAsync(enhancementId);
        return entries
            .GroupBy(e => e.Resource?.Name ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours));
    }

    #endregion
}
