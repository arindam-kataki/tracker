using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

public interface IResourceTypeLookupService
{
    Task<List<ResourceTypeLookupDto>> GetAllAsync(string? search = null);
    Task<ResourceTypeLookup?> GetByIdAsync(string id);
    Task<ResourceTypeLookup> CreateAsync(string name, string? description, int displayOrder, 
        EnhancementColumnType enhancementColumn, bool allowMultiple);
    Task UpdateAsync(string id, string name, string? description, int displayOrder, bool isActive,
        EnhancementColumnType enhancementColumn, bool allowMultiple);
    Task<bool> DeleteAsync(string id);
    Task<List<ResourceTypeLookup>> GetActiveAsync();
}
