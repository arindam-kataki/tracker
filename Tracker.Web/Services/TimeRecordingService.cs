using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class TimeRecordingService : ITimeRecordingService
{
    private readonly TrackerDbContext _db;

    public TimeRecordingService(TrackerDbContext db)
    {
        _db = db;
    }

    #region Categories (Business Areas)

    public async Task<List<TimeRecordingCategory>> GetAllCategoriesAsync(bool activeOnly = true)
    {
        var query = _db.TimeRecordingCategories.AsQueryable();
        
        if (activeOnly)
            query = query.Where(c => c.IsActive);
        
        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<TimeRecordingCategory?> GetCategoryByIdAsync(string id)
    {
        return await _db.TimeRecordingCategories.FindAsync(id);
    }

    public async Task<TimeRecordingCategory> CreateCategoryAsync(string name, string? description, int displayOrder = 0)
    {
        var category = new TimeRecordingCategory
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.TimeRecordingCategories.Add(category);
        await _db.SaveChangesAsync();

        return category;
    }

    public async Task<TimeRecordingCategory?> UpdateCategoryAsync(string id, string name, string? description, int displayOrder, bool isActive)
    {
        var category = await _db.TimeRecordingCategories.FindAsync(id);
        if (category == null)
            return null;

        category.Name = name;
        category.Description = description;
        category.DisplayOrder = displayOrder;
        category.IsActive = isActive;

        await _db.SaveChangesAsync();
        return category;
    }

    public async Task<bool> DeleteCategoryAsync(string id)
    {
        var category = await _db.TimeRecordingCategories.FindAsync(id);
        if (category == null)
            return false;

        // Check if category is used in any time entries
        var isUsed = await _db.EnhancementTimeCategories.AnyAsync(etc => etc.TimeCategoryId == id);
        if (isUsed)
        {
            // Soft delete - just deactivate
            category.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        _db.TimeRecordingCategories.Remove(category);
        await _db.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Enhancement Time Categories

    public async Task<List<TimeRecordingCategory>> GetCategoriesForEnhancementAsync(string enhancementId)
    {
        return await _db.EnhancementTimeCategories
            .Where(etc => etc.EnhancementId == enhancementId)
            .OrderBy(etc => etc.DisplayOrder)
            .Select(etc => etc.TimeCategory)
            .ToListAsync();
    }

    public async Task SetCategoriesForEnhancementAsync(string enhancementId, List<string> categoryIds)
    {
        // Remove existing
        var existing = await _db.EnhancementTimeCategories
            .Where(etc => etc.EnhancementId == enhancementId)
            .ToListAsync();
        
        _db.EnhancementTimeCategories.RemoveRange(existing);

        // Add new
        var order = 0;
        foreach (var categoryId in categoryIds)
        {
            _db.EnhancementTimeCategories.Add(new EnhancementTimeCategory
            {
                EnhancementId = enhancementId,
                TimeCategoryId = categoryId,
                DisplayOrder = order++
            });
        }

        await _db.SaveChangesAsync();
    }

    #endregion

    #region Time Entries

    public async Task<List<EnhancementTimeEntry>> GetEntriesForEnhancementAsync(string enhancementId)
    {
        return await _db.EnhancementTimeEntries
            .Where(e => e.EnhancementId == enhancementId)
            .OrderByDescending(e => e.PeriodStart)
            .ToListAsync();
    }

    public async Task<EnhancementTimeEntry?> GetEntryByIdAsync(string id)
    {
        return await _db.EnhancementTimeEntries.FindAsync(id);
    }

    public async Task<(bool isValid, string? errorMessage)> ValidateDateRangeAsync(
        string enhancementId, 
        DateTime periodStart, 
        DateTime periodEnd, 
        string? excludeEntryId = null)
    {
        if (periodStart > periodEnd)
        {
            return (false, "Start date must be before or equal to end date.");
        }

        // Check for overlapping entries
        var query = _db.EnhancementTimeEntries
            .Where(e => e.EnhancementId == enhancementId);

        if (!string.IsNullOrEmpty(excludeEntryId))
        {
            query = query.Where(e => e.Id != excludeEntryId);
        }

        var overlapping = await query
            .Where(e => 
                // New range starts within existing range
                (periodStart >= e.PeriodStart && periodStart <= e.PeriodEnd) ||
                // New range ends within existing range
                (periodEnd >= e.PeriodStart && periodEnd <= e.PeriodEnd) ||
                // New range completely contains existing range
                (periodStart <= e.PeriodStart && periodEnd >= e.PeriodEnd))
            .FirstOrDefaultAsync();

        if (overlapping != null)
        {
            return (false, $"Date range overlaps with existing entry ({overlapping.PeriodStart:d} - {overlapping.PeriodEnd:d}).");
        }

        return (true, null);
    }

    public async Task<EnhancementTimeEntry> CreateEntryAsync(
        string enhancementId, 
        DateTime periodStart, 
        DateTime periodEnd, 
        Dictionary<string, decimal> hours, 
        string? notes, 
        string userId)
    {
        // Validate date range
        var validation = await ValidateDateRangeAsync(enhancementId, periodStart, periodEnd);
        if (!validation.isValid)
        {
            throw new InvalidOperationException(validation.errorMessage);
        }

        var entry = new EnhancementTimeEntry
        {
            Id = Guid.NewGuid().ToString(),
            EnhancementId = enhancementId,
            PeriodStart = periodStart.Date,
            PeriodEnd = periodEnd.Date,
            Notes = notes,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };
        
        entry.SetHours(hours);

        _db.EnhancementTimeEntries.Add(entry);
        await _db.SaveChangesAsync();

        return entry;
    }

    public async Task<EnhancementTimeEntry?> UpdateEntryAsync(
        string id, 
        DateTime periodStart, 
        DateTime periodEnd, 
        Dictionary<string, decimal> hours, 
        string? notes, 
        string userId)
    {
        var entry = await _db.EnhancementTimeEntries.FindAsync(id);
        if (entry == null)
            return null;

        // Validate date range (excluding current entry)
        var validation = await ValidateDateRangeAsync(entry.EnhancementId, periodStart, periodEnd, id);
        if (!validation.isValid)
        {
            throw new InvalidOperationException(validation.errorMessage);
        }

        entry.PeriodStart = periodStart.Date;
        entry.PeriodEnd = periodEnd.Date;
        entry.SetHours(hours);
        entry.Notes = notes;
        entry.ModifiedBy = userId;
        entry.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> DeleteEntryAsync(string id, string userId)
    {
        var entry = await _db.EnhancementTimeEntries.FindAsync(id);
        if (entry == null)
            return false;

        _db.EnhancementTimeEntries.Remove(entry);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<string, decimal>> GetTotalHoursByCategoryAsync(string enhancementId)
    {
        var entries = await GetEntriesForEnhancementAsync(enhancementId);
        var totals = new Dictionary<string, decimal>();

        foreach (var entry in entries)
        {
            var hours = entry.GetHours();
            foreach (var kvp in hours)
            {
                if (!totals.ContainsKey(kvp.Key))
                    totals[kvp.Key] = 0;
                totals[kvp.Key] += kvp.Value;
            }
        }

        return totals;
    }

    #endregion
}
