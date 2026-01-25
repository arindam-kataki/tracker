using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class EstimationService : IEstimationService
{
    private readonly TrackerDbContext _db;
    private readonly IWorkPhaseService _workPhaseService;

    public EstimationService(TrackerDbContext db, IWorkPhaseService workPhaseService)
    {
        _db = db;
        _workPhaseService = workPhaseService;
    }

    public async Task<List<EstimationBreakdownItem>> GetBreakdownAsync(string enhancementId)
    {
        return await _db.EstimationBreakdownItems
            .Include(e => e.WorkPhase)
            .Where(e => e.EnhancementId == enhancementId)
            .OrderBy(e => e.WorkPhase.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<EstimationPhaseViewModel>> GetBreakdownWithPhasesAsync(string enhancementId)
    {
        // Get all estimation-enabled phases
        var phases = await _workPhaseService.GetForEstimationAsync();
        
        // Get existing breakdown items
        var existingItems = await _db.EstimationBreakdownItems
            .Where(e => e.EnhancementId == enhancementId)
            .ToDictionaryAsync(e => e.WorkPhaseId);

        // Build view model with all phases
        return phases.Select(p => new EstimationPhaseViewModel
        {
            WorkPhaseId = p.Id,
            WorkPhaseName = p.Name,
            WorkPhaseCode = p.Code,
            DisplayOrder = p.DisplayOrder,
            Hours = existingItems.TryGetValue(p.Id, out var item) ? item.Hours : 0,
            Notes = existingItems.TryGetValue(p.Id, out var item2) ? item2.Notes : null
        }).ToList();
    }

    public async Task SaveBreakdownAsync(string enhancementId, List<EstimationPhaseInput> items)
    {
        // Remove existing items
        var existing = await _db.EstimationBreakdownItems
            .Where(e => e.EnhancementId == enhancementId)
            .ToListAsync();
        
        _db.EstimationBreakdownItems.RemoveRange(existing);

        // Add new items (only those with hours > 0 or notes)
        foreach (var input in items.Where(i => i.Hours > 0 || !string.IsNullOrWhiteSpace(i.Notes)))
        {
            _db.EstimationBreakdownItems.Add(new EstimationBreakdownItem
            {
                EnhancementId = enhancementId,
                WorkPhaseId = input.WorkPhaseId,
                Hours = input.Hours,
                Notes = input.Notes
            });
        }

        await _db.SaveChangesAsync();

        // Update enhancement's total estimated hours
        var totalHours = items.Sum(i => i.Hours);
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement != null)
        {
            enhancement.EstimatedHours = totalHours;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalEstimatedHoursAsync(string enhancementId)
    {
        return await _db.EstimationBreakdownItems
            .Where(e => e.EnhancementId == enhancementId)
            .SumAsync(e => e.Hours);
    }

    public async Task CopyBreakdownAsync(string sourceEnhancementId, string targetEnhancementId)
    {
        var sourceItems = await GetBreakdownAsync(sourceEnhancementId);

        // Clear target
        await ClearBreakdownAsync(targetEnhancementId);

        // Copy items
        foreach (var item in sourceItems)
        {
            _db.EstimationBreakdownItems.Add(new EstimationBreakdownItem
            {
                EnhancementId = targetEnhancementId,
                WorkPhaseId = item.WorkPhaseId,
                Hours = item.Hours,
                Notes = item.Notes
            });
        }

        await _db.SaveChangesAsync();

        // Update target enhancement's total
        var totalHours = sourceItems.Sum(i => i.Hours);
        var enhancement = await _db.Enhancements.FindAsync(targetEnhancementId);
        if (enhancement != null)
        {
            enhancement.EstimatedHours = totalHours;
            await _db.SaveChangesAsync();
        }
    }

    public async Task ClearBreakdownAsync(string enhancementId)
    {
        var items = await _db.EstimationBreakdownItems
            .Where(e => e.EnhancementId == enhancementId)
            .ToListAsync();

        _db.EstimationBreakdownItems.RemoveRange(items);

        // Clear enhancement's total
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement != null)
        {
            enhancement.EstimatedHours = null;
        }

        await _db.SaveChangesAsync();
    }

    public async Task<EstimationVsActualReport> GetEstimationVsActualAsync(string enhancementId)
    {
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement == null)
            throw new ArgumentException("Enhancement not found.");

        // Get all phases
        var phases = await _workPhaseService.GetAllAsync();
        var phaseDict = phases.ToDictionary(p => p.Id);

        // Get estimation items
        var estimationItems = await _db.EstimationBreakdownItems
            .Where(e => e.EnhancementId == enhancementId)
            .ToDictionaryAsync(e => e.WorkPhaseId);

        // Get time entries grouped by phase
        var timeEntries = await _db.TimeEntries
            .Where(te => te.EnhancementId == enhancementId)
            .GroupBy(te => te.WorkPhaseId)
            .Select(g => new
            {
                WorkPhaseId = g.Key,
                Hours = g.Sum(te => te.Hours),
                ContributedHours = g.Sum(te => te.ContributedHours)
            })
            .ToDictionaryAsync(x => x.WorkPhaseId);

        // Get consolidation sources grouped by phase (through time entries)
        var consolidationHours = await _db.ConsolidationSources
            .Include(cs => cs.TimeEntry)
            .Where(cs => cs.TimeEntry.EnhancementId == enhancementId)
            .GroupBy(cs => cs.TimeEntry.WorkPhaseId)
            .Select(g => new
            {
                WorkPhaseId = g.Key,
                BilledHours = g.Sum(cs => cs.PulledHours)
            })
            .ToDictionaryAsync(x => x.WorkPhaseId);

        var report = new EstimationVsActualReport
        {
            EnhancementId = enhancementId,
            WorkId = enhancement.WorkId
        };

        // Build phase comparisons
        var allPhaseIds = estimationItems.Keys
            .Union(timeEntries.Keys)
            .Union(consolidationHours.Keys)
            .Distinct();

        foreach (var phaseId in allPhaseIds)
        {
            var phaseName = phaseDict.TryGetValue(phaseId, out var phase) ? phase.Name : "Unknown";

            var comparison = new PhaseComparison
            {
                WorkPhaseId = phaseId,
                WorkPhaseName = phaseName,
                Estimated = estimationItems.TryGetValue(phaseId, out var est) ? est.Hours : 0,
                Actual = timeEntries.TryGetValue(phaseId, out var te) ? te.Hours : 0,
                Contributed = timeEntries.TryGetValue(phaseId, out var te2) ? te2.ContributedHours : 0,
                Billed = consolidationHours.TryGetValue(phaseId, out var ch) ? ch.BilledHours : 0
            };

            report.ByPhase.Add(comparison);
        }

        // Sort by phase order
        report.ByPhase = report.ByPhase
            .OrderBy(p => phaseDict.TryGetValue(p.WorkPhaseId, out var phase) ? phase.DisplayOrder : 999)
            .ToList();

        // Calculate totals
        report.TotalEstimated = report.ByPhase.Sum(p => p.Estimated);
        report.TotalActual = report.ByPhase.Sum(p => p.Actual);
        report.TotalContributed = report.ByPhase.Sum(p => p.Contributed);
        report.TotalBilled = report.ByPhase.Sum(p => p.Billed);

        return report;
    }
}
