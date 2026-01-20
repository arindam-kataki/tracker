using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IResourceService
{
    Task<List<Resource>> GetAllAsync(bool? isClientResource = null, bool? isActive = null);
    Task<Resource?> GetByIdAsync(string id);
    Task<List<Resource>> GetClientResourcesAsync();
    Task<List<Resource>> GetInternalResourcesAsync();
    Task<Resource> CreateAsync(string name, string? email, bool isClientResource);
    Task<Resource?> UpdateAsync(string id, string name, string? email, bool isClientResource, bool isActive);
    Task<bool> DeleteAsync(string id);
}
