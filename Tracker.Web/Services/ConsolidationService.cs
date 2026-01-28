using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class ConsolidationService : IConsolidationService
{
    private readonly TrackerDbContext _db;

    public ConsolidationService(TrackerDbContext db)
    {
        _db = db;
    }

    #region Consolidation CRUD

    public async Task<List<Consolidation>> GetConsolidationsAsync(
        string? serviceAreaId = null,
        string? enhancementId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        ConsolidationStatus? status = null)
    {
        var query = _db.Consolidations
            .Include(c => c.Enhancement)
            .Include(c => c.ServiceArea)
            .Include(c => c.Sources)
                .ThenInclude(s => s.TimeEntry)
                    .ThenInclude(te => te.Resource)
            .Include(c => c.CreatedBy)
            .AsQueryable();

        if (!string.IsNullOrEmpty(serviceAreaId))
            query = query.Where(c => c.ServiceAreaId == serviceAreaId);

        if (!string.IsNullOrEmpty(enhancementId))
            query = query.Where(c => c.EnhancementId == enhancementId);

        if (startDate.HasValue)
            query = query.Where(c => c.EndDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.StartDate <= endDate.Value);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        return await query
            .OrderByDescending(c => c.StartDate)
            .ThenBy(c => c.Enhancement.WorkId)
            .ToListAsync();
    }

    public async Task<Consolidation?> GetByIdAsync(string id)
    {
        return await _db.Consolidations
            .Include(c => c.Enhancement)
            .Include(c => c.ServiceArea)
            .Include(c => c.Sources)
                .ThenInclude(s => s.TimeEntry)
                    .ThenInclude(te => te.Resource)
            .Include(c => c.Sources)
                .ThenInclude(s => s.TimeEntry)
                    .ThenInclude(te => te.WorkPhase)
            .Include(c => c.CreatedBy)
            .Include(c => c.ModifiedBy)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Consolidation> CreateFromTimesheetsAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate,
        List<ConsolidationSourceInput> sources,
        decimal billableHours,
        string? notes,
        string userId)
    {
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement == null)
            throw new ArgumentException("Enhancement not found.");

        var consolidation = new Consolidation
        {
            EnhancementId = enhancementId,
            ServiceAreaId = enhancement.ServiceAreaId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            BillableHours = billableHours,
            SourceHours = sources.Sum(s => s.PulledHours),
            Status = ConsolidationStatus.Draft,
            Notes = notes,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Consolidations.Add(consolidation);

        // Add sources
        foreach (var source in sources.Where(s => s.PulledHours > 0))
        {
            _db.ConsolidationSources.Add(new ConsolidationSource
            {
                ConsolidationId = consolidation.Id,
                TimeEntryId = source.TimeEntryId,
                PulledHours = source.PulledHours
            });
        }

        await _db.SaveChangesAsync();

        return (await GetByIdAsync(consolidation.Id))!;
    }

    public async Task<Consolidation> CreateManualAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate,
        decimal billableHours,
        string notes,
        string userId)
    {
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement == null)
            throw new ArgumentException("Enhancement not found.");

        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notes are required for manual consolidation.");

        var consolidation = new Consolidation
        {
            EnhancementId = enhancementId,
            ServiceAreaId = enhancement.ServiceAreaId,
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            BillableHours = billableHours,
            SourceHours = 0,
            Status = ConsolidationStatus.Draft,
            Notes = notes,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Consolidations.Add(consolidation);
        await _db.SaveChangesAsync();

        return (await GetByIdAsync(consolidation.Id))!;
    }

    public async Task<Consolidation?> UpdateAsync(
        string id,
        List<ConsolidationSourceInput>? sources,
        decimal billableHours,
        string? notes,
        string userId)
    {
        var consolidation = await _db.Consolidations
            .Include(c => c.Sources)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consolidation == null)
            return null;

        if (consolidation.Status != ConsolidationStatus.Draft)
            throw new InvalidOperationException("Can only update Draft consolidations.");

        // Update sources if provided
        if (sources != null)
        {
            // Remove existing sources
            _db.ConsolidationSources.RemoveRange(consolidation.Sources);

            // Add new sources
            foreach (var source in sources.Where(s => s.PulledHours > 0))
            {
                _db.ConsolidationSources.Add(new ConsolidationSource
                {
                    ConsolidationId = consolidation.Id,
                    TimeEntryId = source.TimeEntryId,
                    PulledHours = source.PulledHours
                });
            }

            consolidation.SourceHours = sources.Sum(s => s.PulledHours);
        }

        consolidation.BillableHours = billableHours;
        consolidation.Notes = notes;
        consolidation.ModifiedById = userId;
        consolidation.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task<(bool success, string? error)> DeleteAsync(string id)
    {
        var consolidation = await _db.Consolidations
            .Include(c => c.Sources)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consolidation == null)
            return (false, "Consolidation not found.");

        if (consolidation.Status != ConsolidationStatus.Draft)
            return (false, "Can only delete Draft consolidations.");

        _db.ConsolidationSources.RemoveRange(consolidation.Sources);
        _db.Consolidations.Remove(consolidation);
        await _db.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool success, string? error)> ChangeStatusAsync(string id, ConsolidationStatus newStatus, string userId)
    {
        var consolidation = await _db.Consolidations
            .Include(c => c.Sources)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consolidation == null)
            return (false, "Consolidation not found.");

        // Validate status transition
        var validTransition = (consolidation.Status, newStatus) switch
        {
            (ConsolidationStatus.Draft, ConsolidationStatus.Finalized) => true,
            (ConsolidationStatus.Finalized, ConsolidationStatus.Draft) => true, // Allow un-finalizing
            (ConsolidationStatus.Finalized, ConsolidationStatus.Invoiced) => true,
            _ => false
        };

        if (!validTransition)
            return (false, $"Cannot change status from {consolidation.Status} to {newStatus}.");

        // Validate before finalizing
        if (newStatus == ConsolidationStatus.Finalized)
        {
            if (consolidation.BillableHours <= 0)
                return (false, "Cannot finalize consolidation with zero billable hours.");

            if (!consolidation.Sources.Any() && string.IsNullOrWhiteSpace(consolidation.Notes))
                return (false, "Manual consolidation requires notes.");
        }

        consolidation.Status = newStatus;
        consolidation.ModifiedById = userId;
        consolidation.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return (true, null);
    }

    #endregion

    #region Timesheet Entry Loading

    public async Task<List<TimeEntryForConsolidation>> GetAvailableEntriesAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate)
    {
        var entries = await _db.TimeEntries
            .Include(te => te.Resource)
            .Include(te => te.WorkPhase)
            .Include(te => te.ConsolidationSources)
            .Where(te => te.EnhancementId == enhancementId)
            .Where(te => te.StartDate <= DateOnly.FromDateTime(endDate) && 
             te.EndDate >= DateOnly.FromDateTime(startDate))
            .Where(te => te.WorkPhase.ForConsolidation)
            .OrderBy(te => te.Resource.Name)
            .ThenBy(te => te.StartDate)
            .ToListAsync();

        return entries.Select(te => new TimeEntryForConsolidation
        {
            Id = te.Id,
            ResourceId = te.ResourceId,
            ResourceName = te.Resource?.Name ?? "Unknown",
            StartDate = te.StartDate,
            EndDate = te.EndDate,
            WorkPhaseName = te.WorkPhase?.Name ?? "Unknown",
            Hours = te.Hours,
            ContributedHours = te.ContributedHours,
            AlreadyPulledHours = te.TotalPulledHours,
            AvailableHours = te.RemainingHours,
            Notes = te.Notes
        }).ToList();
    }

    public async Task<List<EnhancementWithTimeEntries>> GetEnhancementsWithEntriesAsync(
        string? serviceAreaId,
        DateTime startDate,
        DateTime endDate)
    {
        var query = _db.TimeEntries
            .Include(te => te.Enhancement)
                .ThenInclude(e => e.ServiceArea)
            .Include(te => te.WorkPhase)
            .Where(te => te.StartDate <= DateOnly.FromDateTime(endDate) && 
             te.EndDate >= DateOnly.FromDateTime(startDate))
            .Where(te => te.WorkPhase.ForConsolidation)
            .AsQueryable();

        if (!string.IsNullOrEmpty(serviceAreaId))
            query = query.Where(te => te.Enhancement.ServiceAreaId == serviceAreaId);

        var grouped = await query
            .GroupBy(te => new 
            { 
                te.EnhancementId, 
                te.Enhancement.WorkId, 
                te.Enhancement.Description,
                te.Enhancement.ServiceAreaId,
                ServiceAreaName = te.Enhancement.ServiceArea.Name
            })
            .Select(g => new EnhancementWithTimeEntries
            {
                Id = g.Key.EnhancementId,
                WorkId = g.Key.WorkId,
                Description = g.Key.Description,
                ServiceAreaId = g.Key.ServiceAreaId,
                ServiceAreaName = g.Key.ServiceAreaName,
                EntryCount = g.Count(),
                TotalHours = g.Sum(te => te.Hours),
                TotalContributedHours = g.Sum(te => te.ContributedHours)
            })
            .OrderBy(e => e.WorkId)
            .ToListAsync();

        return grouped;
    }

    #endregion

    #region Validation

    public (bool isValid, string? error) ValidateDateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            return (false, "End date must be on or after start date.");

        if (startDate.Year != endDate.Year || startDate.Month != endDate.Month)
            return (false, "Date range must be within the same month.");

        return (true, null);
    }

    public async Task<(bool isValid, string? error)> ValidateConsolidationAsync(
        string enhancementId,
        DateTime startDate,
        DateTime endDate,
        decimal billableHours,
        bool hasNotes,
        bool hasSources)
    {
        // Validate date range
        var dateValidation = ValidateDateRange(startDate, endDate);
        if (!dateValidation.isValid)
            return dateValidation;

        // Validate enhancement exists
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement == null)
            return (false, "Enhancement not found.");

        // Validate billable hours
        if (billableHours < 0)
            return (false, "Billable hours cannot be negative.");

        // Manual entries require notes
        if (!hasSources && !hasNotes)
            return (false, "Notes are required for manual consolidation.");

        return (true, null);
    }

    #endregion

    #region Reporting

    public async Task<List<ConsolidationSummary>> GetSummaryByEnhancementAsync(
        string? serviceAreaId,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = _db.Consolidations
            .Include(c => c.Enhancement)
            .AsQueryable();

        if (!string.IsNullOrEmpty(serviceAreaId))
            query = query.Where(c => c.ServiceAreaId == serviceAreaId);

        if (startDate.HasValue)
            query = query.Where(c => c.EndDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.StartDate <= endDate.Value);

        return await query
            .GroupBy(c => new { c.EnhancementId, c.Enhancement.WorkId })
            .Select(g => new ConsolidationSummary
            {
                GroupKey = g.Key.EnhancementId,
                GroupName = g.Key.WorkId,
                ConsolidationCount = g.Count(),
                TotalBillableHours = g.Sum(c => c.BillableHours),
                TotalSourceHours = g.Sum(c => c.SourceHours)
            })
            .OrderBy(s => s.GroupName)
            .ToListAsync();
    }

    public async Task<List<ConsolidationSummary>> GetSummaryByServiceAreaAsync(
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = _db.Consolidations
            .Include(c => c.ServiceArea)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(c => c.EndDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(c => c.StartDate <= endDate.Value);

        return await query
            .GroupBy(c => new { c.ServiceAreaId, c.ServiceArea.Name })
            .Select(g => new ConsolidationSummary
            {
                GroupKey = g.Key.ServiceAreaId,
                GroupName = g.Key.Name,
                ConsolidationCount = g.Count(),
                TotalBillableHours = g.Sum(c => c.BillableHours),
                TotalSourceHours = g.Sum(c => c.SourceHours)
            })
            .OrderBy(s => s.GroupName)
            .ToListAsync();
    }

    #endregion
}
