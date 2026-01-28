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

    public async Task<List<TimeEntry>> GetEntriesForResourceAsync(string resourceId, DateOnly? startDate = null, DateOnly? endDate = null)
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
        DateOnly? startDate = null,
        DateOnly? endDate = null)
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
        DateOnly startDate,
        DateOnly endDate,
        decimal hours,
        decimal? contributedHours,
        string? notes,
        string createdByResourceId)
    {
        // Get work phase for default contribution
        var workPhase = await _workPhaseService.GetByIdAsync(workPhaseId);

        // Calculate contributed hours if not provided
        var actualContributedHours = contributedHours ??
            (hours * (workPhase?.DefaultContributionPercent ?? 100) / 100m);

        var entry = new TimeEntry
        {
            EnhancementId = enhancementId,
            ResourceId = resourceId,
            WorkPhaseId = workPhaseId,
            StartDate = startDate,
            EndDate = endDate,
            Hours = hours,
            ContributedHours = actualContributedHours,
            Notes = notes,
            //CreatedById = createdByResourceId,
            CreatedAt = DateTime.UtcNow
        };

        _db.TimeEntries.Add(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Created time entry {Id} for enhancement {Enhancement} by resource {Resource}: {Hours}h on {Date}",
            entry.Id, enhancementId, resourceId, hours, startDate);

        return entry;
    }

    public async Task<TimeEntry?> UpdateEntryAsync(
        string id,
        string workPhaseId,
        DateOnly startDate,
        DateOnly endDate,
        decimal hours,
        decimal contributedHours,
        string? notes,
        string modifiedByResourceId)
    {
        var entry = await _db.TimeEntries.FindAsync(id);
        if (entry == null) return null;

        // Check if entry is consolidated
        var isConsolidated = await _db.ConsolidationSources
            .AnyAsync(cs => cs.TimeEntryId == id);

        if (isConsolidated)
        {
            // Only allow notes update for consolidated entries
            entry.Notes = notes;
            //entry.ModifiedById = modifiedByResourceId;
            entry.ModifiedAt = DateTime.UtcNow;
        }
        else
        {
            entry.WorkPhaseId = workPhaseId;
            entry.StartDate = startDate;
            entry.EndDate = endDate;
            entry.Hours = hours;
            entry.ContributedHours = contributedHours;
            entry.Notes = notes;
            //entry.ModifiedById = modifiedByResourceId;
            entry.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Updated time entry {Id}", id);

        return entry;
    }

    public async Task<(bool success, string? error)> DeleteEntryAsync(string id)
    {
        var entry = await _db.TimeEntries
            .Include(te => te.ConsolidationSources)
            .FirstOrDefaultAsync(te => te.Id == id);

        if (entry == null)
            return (false, "Time entry not found.");

        // Check if entry has been consolidated
        if (entry.ConsolidationSources?.Any() == true)
            return (false, "Cannot delete a time entry that has been consolidated.");

        _db.TimeEntries.Remove(entry);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Deleted time entry {Id}", id);

        return (true, null);
    }

    public async Task<(bool isValid, string? error)> ValidateEntryAsync(
        string enhancementId,
        DateOnly startDate,
        DateOnly endDate,
        decimal hours,
        decimal contributedHours)
    {
        // Check enhancement exists
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement == null)
            return (false, "Enhancement not found.");

        // Validate dates
        if (endDate < startDate)
            return (false, "End date cannot be before start date.");

        // Validate hours
        if (hours <= 0)
            return (false, "Hours must be greater than zero.");

        if (contributedHours < 0)
            return (false, "Contributed hours cannot be negative.");

        if (contributedHours > hours)
            return (false, "Contributed hours cannot exceed total hours.");

        return (true, null);
    }

    #endregion

    #region My Timesheet - Permission Based

    public async Task<List<ServiceArea>> GetServiceAreasWithTimesheetPermissionAsync(string resourceId)
    {
        // Get service areas where user has LogTimesheet permission
        // Permissions is a flags enum, so we need to use HasFlag in memory after fetching
        var memberships = await _db.ResourceServiceAreas
            .Where(rsa => rsa.ResourceId == resourceId)
            .ToListAsync();

        var serviceAreaIds = memberships
            .Where(rsa => rsa.Permissions.HasFlag(Permissions.LogTimesheet))
            .Select(rsa => rsa.ServiceAreaId)
            .ToList();

        return await _db.ServiceAreas
            .Where(sa => serviceAreaIds.Contains(sa.Id) && sa.IsActive)
            .OrderBy(sa => sa.Code)
            .ToListAsync();
    }

    public async Task<List<Enhancement>> GetEnhancementsForTimesheetAsync(
        string resourceId,
        string? serviceAreaId = null,
        string? search = null)
    {
        // First get permitted service areas (need to evaluate HasFlag in memory)
        var memberships = await _db.ResourceServiceAreas
            .Where(rsa => rsa.ResourceId == resourceId)
            .ToListAsync();

        var permittedServiceAreaIds = memberships
            .Where(rsa => rsa.Permissions.HasFlag(Permissions.LogTimesheet))
            .Select(rsa => rsa.ServiceAreaId)
            .ToList();

        if (!permittedServiceAreaIds.Any())
            return new List<Enhancement>();

        var query = _db.Enhancements
            .Include(e => e.ServiceArea)
            .Where(e => permittedServiceAreaIds.Contains(e.ServiceAreaId));

        // Filter by specific service area if provided
        if (!string.IsNullOrEmpty(serviceAreaId))
        {
            if (!permittedServiceAreaIds.Contains(serviceAreaId))
                return new List<Enhancement>();

            query = query.Where(e => e.ServiceAreaId == serviceAreaId);
        }

        // Search filter
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(e =>
                e.WorkId.ToLower().Contains(searchLower) ||
                e.Description.ToLower().Contains(searchLower));
        }

        // Only show active enhancements (not Cancelled)
        query = query.Where(e => e.Status != "Cancelled");

        return await query
            .OrderBy(e => e.ServiceArea.Code)
            .ThenBy(e => e.WorkId)
            .Take(100) // Limit results
            .ToListAsync();
    }

    public async Task<bool> CanLogTimeForEnhancementAsync(string resourceId, string enhancementId)
    {
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement == null) return false;

        // Get the resource's memberships and check in memory (for HasFlag)
        var membership = await _db.ResourceServiceAreas
            .FirstOrDefaultAsync(rsa =>
                rsa.ResourceId == resourceId &&
                rsa.ServiceAreaId == enhancement.ServiceAreaId);

        if (membership == null) return false;

        return membership.Permissions.HasFlag(Permissions.LogTimesheet);
    }

    #endregion

    #region Consolidation Support

    public async Task<List<TimeEntry>> GetUnconsolidatedEntriesAsync(
        string enhancementId,
        DateOnly startDate,
        DateOnly endDate)
    {
        return await _db.TimeEntries
            .Include(te => te.Resource)
            .Include(te => te.WorkPhase)
            .Include(te => te.ConsolidationSources)
            .Where(te => te.EnhancementId == enhancementId)
            .Where(te => te.StartDate >= startDate && te.EndDate <= endDate)
            .Where(te => te.ContributedHours > te.ConsolidationSources.Sum(cs => cs.PulledHours))
            .OrderBy(te => te.StartDate)
            .ThenBy(te => te.Resource.Name)
            .ToListAsync();
    }

    public async Task<List<TimeEntry>> GetConsolidatedEntriesAsync(
        string enhancementId,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var query = _db.TimeEntries
            .Include(te => te.Resource)
            .Include(te => te.WorkPhase)
            .Include(te => te.ConsolidationSources)
                .ThenInclude(cs => cs.Consolidation)
            .Where(te => te.EnhancementId == enhancementId)
            .Where(te => te.ConsolidationSources.Any());

        if (startDate.HasValue)
            query = query.Where(te => te.EndDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(te => te.StartDate <= endDate.Value);

        return await query
            .OrderBy(te => te.StartDate)
            .ThenBy(te => te.Resource.Name)
            .ToListAsync();
    }

    #endregion

    // Add this method to TimesheetService.cs:

    /// <summary>
    /// Gets time entries for multiple resources within a date range.
    /// Used for team/manager timesheet rollup views.
    /// </summary>
    public async Task<List<TimeEntry>> GetEntriesForResourcesAsync(
        List<string> resourceIds,
        List<string>? serviceAreaIds = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        if (resourceIds == null || !resourceIds.Any())
            return new List<TimeEntry>();

        var query = _db.TimeEntries
            .Include(te => te.Resource)
            .Include(te => te.Enhancement)
                .ThenInclude(e => e.ServiceArea)
            .Include(te => te.WorkPhase)
            .Include(te => te.ConsolidationSources)
            .Where(te => resourceIds.Contains(te.ResourceId));

        // Filter by service areas if specified
        if (serviceAreaIds != null && serviceAreaIds.Any())
        {
            query = query.Where(te => serviceAreaIds.Contains(te.Enhancement.ServiceAreaId));
        }

        // Filter by date range
        if (startDate.HasValue)
            query = query.Where(te => te.EndDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(te => te.StartDate <= endDate.Value);

        return await query
            .OrderBy(te => te.Enhancement.ServiceArea.Code)
            .ThenBy(te => te.Enhancement.WorkId)
            .ThenByDescending(te => te.StartDate)
            .ToListAsync();
    }


}
