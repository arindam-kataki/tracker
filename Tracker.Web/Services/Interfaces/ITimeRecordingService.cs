using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface ITimeRecordingService
{
    // Categories (Business Areas)
    Task<List<TimeRecordingCategory>> GetAllCategoriesAsync(bool activeOnly = true);
    Task<TimeRecordingCategory?> GetCategoryByIdAsync(string id);
    Task<TimeRecordingCategory> CreateCategoryAsync(string name, string? description, int displayOrder = 0);
    Task<TimeRecordingCategory?> UpdateCategoryAsync(string id, string name, string? description, int displayOrder, bool isActive);
    Task<bool> DeleteCategoryAsync(string id);
    
    // Enhancement Time Categories (selected categories for an enhancement)
    Task<List<TimeRecordingCategory>> GetCategoriesForEnhancementAsync(string enhancementId);
    Task SetCategoriesForEnhancementAsync(string enhancementId, List<string> categoryIds);
    
    // Time Entries
    Task<List<EnhancementTimeEntry>> GetEntriesForEnhancementAsync(string enhancementId);
    Task<EnhancementTimeEntry?> GetEntryByIdAsync(string id);
    Task<EnhancementTimeEntry> CreateEntryAsync(string enhancementId, DateTime periodStart, DateTime periodEnd, Dictionary<string, decimal> hours, string? notes, string userId);
    Task<EnhancementTimeEntry?> UpdateEntryAsync(string id, DateTime periodStart, DateTime periodEnd, Dictionary<string, decimal> hours, string? notes, string userId);
    Task<bool> DeleteEntryAsync(string id, string userId);
    
    /// <summary>
    /// Validates that the date range doesn't overlap with existing entries for the same enhancement.
    /// </summary>
    Task<(bool isValid, string? errorMessage)> ValidateDateRangeAsync(string enhancementId, DateTime periodStart, DateTime periodEnd, string? excludeEntryId = null);
    
    /// <summary>
    /// Gets total hours for an enhancement, grouped by category.
    /// </summary>
    Task<Dictionary<string, decimal>> GetTotalHoursByCategoryAsync(string enhancementId);
}
