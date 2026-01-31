using Tracker.Web.Entities.Availability;
using Tracker.Web.ViewModels.Availability;

namespace Tracker.Web.Services.Availability;

/// <summary>
/// Service for managing company holidays
/// </summary>
public interface IHolidayService
{
    /// <summary>
    /// Get all holidays for a year
    /// </summary>
    Task<List<CompanyHoliday>> GetHolidaysAsync(int year);
    
    /// <summary>
    /// Get holidays in a date range
    /// </summary>
    Task<List<CompanyHoliday>> GetHolidaysAsync(DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Get holidays applicable to a specific resource (considering service area and location)
    /// </summary>
    Task<List<CompanyHoliday>> GetHolidaysForResourceAsync(
        string resourceId, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Get a specific holiday
    /// </summary>
    Task<CompanyHoliday?> GetHolidayAsync(string id);
    
    /// <summary>
    /// Create a new holiday
    /// </summary>
    Task<CompanyHoliday> CreateHolidayAsync(CompanyHoliday holiday);
    
    /// <summary>
    /// Update an existing holiday
    /// </summary>
    Task<CompanyHoliday> UpdateHolidayAsync(CompanyHoliday holiday);
    
    /// <summary>
    /// Delete a holiday
    /// </summary>
    Task<bool> DeleteHolidayAsync(string id);
    
    /// <summary>
    /// Check if a date is a holiday
    /// </summary>
    Task<bool> IsHolidayAsync(DateOnly date, string? serviceAreaId = null, string? location = null);
    
    /// <summary>
    /// Get holiday hours for a date (0 if not a holiday)
    /// </summary>
    Task<decimal> GetHolidayHoursAsync(DateOnly date, string? serviceAreaId = null, string? location = null);
    
    /// <summary>
    /// Get view model for holiday list page
    /// </summary>
    Task<CompanyHolidayListViewModel> GetHolidayListViewModelAsync(int year, string? serviceAreaId = null);
    
    /// <summary>
    /// Get view model for editing a holiday
    /// </summary>
    Task<CompanyHolidayViewModel> GetHolidayViewModelAsync(string? id = null);
    
    /// <summary>
    /// Get list of distinct locations from existing holidays
    /// </summary>
    Task<List<string>> GetDistinctLocationsAsync();
    
    /// <summary>
    /// Copy holidays from one year to another
    /// </summary>
    Task<int> CopyHolidaysToYearAsync(int sourceYear, int targetYear);
}
