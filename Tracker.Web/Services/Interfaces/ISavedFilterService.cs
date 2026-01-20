using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

public interface ISavedFilterService
{
    Task<List<SavedFilter>> GetUserFiltersAsync(string userId, string serviceAreaId);
    Task<SavedFilter?> GetByIdAsync(string id);
    Task<SavedFilter?> GetDefaultFilterAsync(string userId, string serviceAreaId);
    Task<SavedFilter> SaveFilterAsync(string userId, SaveFilterRequest request);
    Task<bool> DeleteFilterAsync(string id, string userId);
    
    // Column preferences
    Task<List<string>> GetUserColumnsAsync(string userId, string serviceAreaId);
    Task SaveUserColumnsAsync(string userId, string serviceAreaId, List<string> columns);
    
    // Distinct values for filter dropdowns
    Task<List<string>> GetDistinctStatusesAsync(string serviceAreaId);
    Task<List<string>> GetDistinctInfStatusesAsync(string serviceAreaId);
    Task<List<string>> GetDistinctServiceLinesAsync(string serviceAreaId);
}
