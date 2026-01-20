using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IResourceService
{
    Task<List<Resource>> GetAllAsync(ResourceType? type = null, bool? isActive = null);
    Task<Resource?> GetByIdAsync(string id);
    Task<List<Resource>> GetByTypeAsync(ResourceType type);
    Task<List<Resource>> GetClientResourcesAsync();      // Type = Client
    Task<List<Resource>> GetSpocResourcesAsync();        // Type = SPOC
    Task<List<Resource>> GetInternalResourcesAsync();    // Type = Internal
    Task<Resource> CreateAsync(string name, string? email, ResourceType type);
    Task<Resource?> UpdateAsync(string id, string name, string? email, ResourceType type, bool isActive);
    Task<bool> DeleteAsync(string id);
}
