using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class WorkPhaseService : IWorkPhaseService
{
    private readonly TrackerDbContext _db;

    public WorkPhaseService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<List<WorkPhase>> GetAllAsync(bool activeOnly = true)
    {
        var query = _db.WorkPhases.AsQueryable();
        
        if (activeOnly)
            query = query.Where(wp => wp.IsActive);
        
        return await query
            .OrderBy(wp => wp.DisplayOrder)
            .ThenBy(wp => wp.Name)
            .ToListAsync();
    }

    public async Task<List<WorkPhase>> GetForEstimationAsync()
    {
        return await _db.WorkPhases
            .Where(wp => wp.IsActive && wp.ForEstimation)
            .OrderBy(wp => wp.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<WorkPhase>> GetForTimeRecordingAsync()
    {
        return await _db.WorkPhases
            .Where(wp => wp.IsActive && wp.ForTimeRecording)
            .OrderBy(wp => wp.DisplayOrder)
            .ToListAsync();
    }

    public async Task<List<WorkPhase>> GetForConsolidationAsync()
    {
        return await _db.WorkPhases
            .Where(wp => wp.IsActive && wp.ForConsolidation)
            .OrderBy(wp => wp.DisplayOrder)
            .ToListAsync();
    }

    public async Task<WorkPhase?> GetByIdAsync(string id)
    {
        return await _db.WorkPhases.FindAsync(id);
    }

    public async Task<WorkPhase?> GetByCodeAsync(string code)
    {
        return await _db.WorkPhases
            .FirstOrDefaultAsync(wp => wp.Code.ToLower() == code.ToLower());
    }

    public async Task<WorkPhase> CreateAsync(string name, string code, string? description,
        int defaultContributionPercent, int displayOrder,
        bool forEstimation, bool forTimeRecording, bool forConsolidation)
    {
        var workPhase = new WorkPhase
        {
            Name = name,
            Code = code.ToUpper(),
            Description = description,
            DefaultContributionPercent = Math.Clamp(defaultContributionPercent, 0, 100),
            DisplayOrder = displayOrder,
            ForEstimation = forEstimation,
            ForTimeRecording = forTimeRecording,
            ForConsolidation = forConsolidation,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.WorkPhases.Add(workPhase);
        await _db.SaveChangesAsync();

        return workPhase;
    }

    public async Task<WorkPhase?> UpdateAsync(string id, string name, string code, string? description,
        int defaultContributionPercent, int displayOrder, bool isActive,
        bool forEstimation, bool forTimeRecording, bool forConsolidation)
    {
        var workPhase = await _db.WorkPhases.FindAsync(id);
        if (workPhase == null)
            return null;

        workPhase.Name = name;
        workPhase.Code = code.ToUpper();
        workPhase.Description = description;
        workPhase.DefaultContributionPercent = Math.Clamp(defaultContributionPercent, 0, 100);
        workPhase.DisplayOrder = displayOrder;
        workPhase.IsActive = isActive;
        workPhase.ForEstimation = forEstimation;
        workPhase.ForTimeRecording = forTimeRecording;
        workPhase.ForConsolidation = forConsolidation;

        await _db.SaveChangesAsync();
        return workPhase;
    }

    public async Task<(bool success, string? error)> DeleteAsync(string id)
    {
        var workPhase = await _db.WorkPhases
            .Include(wp => wp.EstimationItems)
            .Include(wp => wp.TimeEntries)
            .FirstOrDefaultAsync(wp => wp.Id == id);

        if (workPhase == null)
            return (false, "Work phase not found.");

        // Check if in use
        if (workPhase.EstimationItems.Any())
            return (false, "Cannot delete work phase that has estimation data. Deactivate it instead.");

        if (workPhase.TimeEntries.Any())
            return (false, "Cannot delete work phase that has timesheet entries. Deactivate it instead.");

        _db.WorkPhases.Remove(workPhase);
        await _db.SaveChangesAsync();

        return (true, null);
    }

    public async Task SeedDefaultPhasesAsync()
    {
        // Only seed if no phases exist
        if (await _db.WorkPhases.AnyAsync())
            return;

        var defaultPhases = new List<WorkPhase>
        {
            new() { Name = "Requirements & Estimation", Code = "REQ", DefaultContributionPercent = 100, DisplayOrder = 1, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Vendor Coordination", Code = "VENDOR", DefaultContributionPercent = 100, DisplayOrder = 2, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Design - Functional/Technical", Code = "DESIGN", DefaultContributionPercent = 100, DisplayOrder = 3, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Development", Code = "DEV", DefaultContributionPercent = 100, DisplayOrder = 4, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Testing - STI", Code = "STI", DefaultContributionPercent = 100, DisplayOrder = 5, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Testing - UAT", Code = "UAT", DefaultContributionPercent = 100, DisplayOrder = 6, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Go-Live / Deployment", Code = "GOLIVE", DefaultContributionPercent = 100, DisplayOrder = 7, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Hypercare", Code = "HYPER", DefaultContributionPercent = 100, DisplayOrder = 8, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Documentation", Code = "DOC", DefaultContributionPercent = 100, DisplayOrder = 9, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "PM / Lead", Code = "PM", DefaultContributionPercent = 100, DisplayOrder = 10, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Contingency", Code = "CONTG", DefaultContributionPercent = 100, DisplayOrder = 11, ForEstimation = true, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Meetings", Code = "MTG", DefaultContributionPercent = 50, DisplayOrder = 12, ForEstimation = false, ForTimeRecording = true, ForConsolidation = true },
            new() { Name = "Training", Code = "TRAIN", DefaultContributionPercent = 0, DisplayOrder = 13, ForEstimation = false, ForTimeRecording = true, ForConsolidation = false },
            new() { Name = "Admin / Other", Code = "OTHER", DefaultContributionPercent = 0, DisplayOrder = 14, ForEstimation = false, ForTimeRecording = true, ForConsolidation = false },
        };

        _db.WorkPhases.AddRange(defaultPhases);
        await _db.SaveChangesAsync();
    }
}
