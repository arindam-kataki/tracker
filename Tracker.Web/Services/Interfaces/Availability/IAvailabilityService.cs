using Tracker.Web.Entities.Availability;
using Tracker.Web.ViewModels.Availability;

namespace Tracker.Web.Services.Availability;

/// <summary>
/// Service for managing resource availability, PTO, and capacity
/// </summary>
public interface IAvailabilityService
{
    #region Available Hours Calculation
    
    /// <summary>
    /// Get total available hours for a resource in a date range.
    /// Accounts for schedule, holidays, and approved time off.
    /// </summary>
    Task<decimal> GetAvailableHoursAsync(string resourceId, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Get daily availability breakdown for a resource
    /// </summary>
    Task<List<DailyAvailabilityViewModel>> GetDailyAvailabilityAsync(
        string resourceId, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Get availability for multiple resources (for capacity planning)
    /// </summary>
    Task<Dictionary<string, decimal>> GetTeamAvailabilityAsync(
        List<string> resourceIds, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Check if a resource is available on a specific date
    /// </summary>
    Task<bool> IsAvailableAsync(string resourceId, DateOnly date);
    
    #endregion
    
    #region Resource Schedule
    
    /// <summary>
    /// Get the active schedule for a resource on a given date
    /// </summary>
    Task<ResourceSchedule?> GetScheduleAsync(string resourceId, DateOnly date);
    
    /// <summary>
    /// Get scheduled hours for a specific day of week
    /// </summary>
    Task<decimal> GetScheduledHoursAsync(string resourceId, DateOnly date);
    
    /// <summary>
    /// Create or update a resource schedule
    /// </summary>
    Task<ResourceSchedule> SaveScheduleAsync(ResourceSchedule schedule);
    
    /// <summary>
    /// Get all schedules for a resource (history)
    /// </summary>
    Task<List<ResourceSchedule>> GetScheduleHistoryAsync(string resourceId);
    
    #endregion
    
    #region Availability Entries (PTO, etc.)
    
    /// <summary>
    /// Get availability entries for a resource in a date range
    /// </summary>
    Task<List<ResourceAvailability>> GetAvailabilityEntriesAsync(
        string resourceId, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Get a specific availability entry
    /// </summary>
    Task<ResourceAvailability?> GetAvailabilityEntryAsync(string id);
    
    /// <summary>
    /// Create a new availability entry (PTO request, etc.)
    /// </summary>
    Task<ResourceAvailability> CreateAvailabilityEntryAsync(ResourceAvailability entry);
    
    /// <summary>
    /// Update an existing availability entry
    /// </summary>
    Task<ResourceAvailability> UpdateAvailabilityEntryAsync(ResourceAvailability entry);
    
    /// <summary>
    /// Delete an availability entry
    /// </summary>
    Task<bool> DeleteAvailabilityEntryAsync(string id);
    
    #endregion
    
    #region PTO Requests & Approval
    
    /// <summary>
    /// Submit a PTO request
    /// </summary>
    Task<ResourceAvailability> SubmitPtoRequestAsync(PtoRequestViewModel request, string requesterId);
    
    /// <summary>
    /// Approve a PTO request
    /// </summary>
    Task<bool> ApprovePtoRequestAsync(string requestId, string approverId);
    
    /// <summary>
    /// Reject a PTO request
    /// </summary>
    Task<bool> RejectPtoRequestAsync(string requestId, string approverId, string reason);
    
    /// <summary>
    /// Cancel a PTO request (by the requester)
    /// </summary>
    Task<bool> CancelPtoRequestAsync(string requestId, string requesterId);
    
    /// <summary>
    /// Get pending PTO requests for approval (for a manager)
    /// </summary>
    Task<List<ResourceAvailability>> GetPendingPtoRequestsAsync(
        string managerId, List<string>? serviceAreaIds = null);
    
    /// <summary>
    /// Get PTO requests for a resource
    /// </summary>
    Task<List<ResourceAvailability>> GetPtoRequestsForResourceAsync(
        string resourceId, int? year = null);
    
    #endregion
    
    #region Team Capacity
    
    /// <summary>
    /// Get capacity view model for a team
    /// </summary>
    Task<TeamCapacityViewModel> GetTeamCapacityAsync(
        List<string> resourceIds, DateOnly startDate, DateOnly endDate);
    
    /// <summary>
    /// Get calendar view model for a resource
    /// </summary>
    Task<ResourceAvailabilityCalendarViewModel> GetCalendarViewModelAsync(
        string resourceId, int year, int month);
    
    #endregion
}
