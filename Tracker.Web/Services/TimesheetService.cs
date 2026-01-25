using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class TimesheetService : ITimesheetService
{
    private readonly TrackerDbContext _db;
    private readonly IWorkPhaseService _workPhaseService;

    public TimesheetService(TrackerDbContext db, IWorkPhaseService workPhaseService)
    {
        _db = db;
        _workPhaseService = workPhaseService;
    }

    #region Time Entries

    public async Task<List<TimeEntry>> GetEntriesForEnhancementAsync(string enhancementId)
    {
        return await _db.TimeEntries
            .Include(te => te.Resource)
            .Include(te => te.WorkPhase)
            .Where(te => te.EnhancementId == enhancementId)
            .OrderByDescending(te => te.StartDate)
            .ThenBy(te => te.Resource.Name)
            .ToListAsync();
    }

    public async Task<List<TimeEntry>> GetEntriesForResourceAsync(string resourceId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _db.TimeEntries
            .Include(te => te.Enhancement)
            .Include(te => te.WorkPhase)
            .Where(te => te.ResourceId == resourceId);

        if (startDate.HasValue)
            query = query.Where(te => te.EndDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(te => te.StartDate <= endDate.Value);

        return await query
            .OrderByDescending(te => te.StartDate)
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
        string userId)
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
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.TimeEntries.Add(entry);
        await _db.SaveChangesAsync();

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
        string userId)
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
            entry.ModifiedById = userId;
            entry.ModifiedAt = DateTime.UtcNow;
        }
        else
        {
            entry.WorkPhaseId = workPhaseId;
            entry.StartDate = startDate.Date;
            entry.EndDate = endDate.Date;
            entry.Hours = hours;
            entry.ContributedHours = contributedHours;
            entry.Notes = notes;
            entry.ModifiedById = userId;
            entry.ModifiedAt = DateTime.UtcNow;
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

    #region My Timesheet

    public async Task<Resource?> GetResourceForUserAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user?.Email == null)
            return null;

        // Match resource by email
        return await _db.Resources
            .FirstOrDefaultAsync(r => r.Email != null && 
                r.Email.ToLower() == user.Email.ToLower() && 
                r.IsActive);
    }

    public async Task<List<Enhancement>> GetAssignedEnhancementsAsync(string resourceId)
    {
        // Get enhancements where this resource is assigned as a resource (not sponsor or SPOC)
        var enhancementIds = await _db.EnhancementResources
            .Where(er => er.ResourceId == resourceId)
            .Select(er => er.EnhancementId)
            .ToListAsync();

        return await _db.Enhancements
            .Include(e => e.ServiceArea)
            .Where(e => enhancementIds.Contains(e.Id))
            .OrderBy(e => e.WorkId)
            .ToListAsync();
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

    #endregion

    #region Reporting

    public async Task<Dictionary<string, decimal>> GetHoursByPhaseForEnhancementAsync(string enhancementId)
    {
        return await _db.TimeEntries
            .Where(te => te.EnhancementId == enhancementId)
            .GroupBy(te => te.WorkPhase.Name)
            .Select(g => new { Phase = g.Key, Hours = g.Sum(te => te.Hours) })
            .ToDictionaryAsync(x => x.Phase, x => x.Hours);
    }

    public async Task<Dictionary<string, decimal>> GetHoursByResourceForEnhancementAsync(string enhancementId)
    {
        return await _db.TimeEntries
            .Where(te => te.EnhancementId == enhancementId)
            .GroupBy(te => te.Resource.Name)
            .Select(g => new { Resource = g.Key, Hours = g.Sum(te => te.Hours) })
            .ToDictionaryAsync(x => x.Resource, x => x.Hours);
    }

    #endregion
}
