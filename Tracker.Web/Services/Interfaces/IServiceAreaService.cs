using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IServiceAreaService
{
    Task<List<ServiceArea>> GetAllAsync(bool activeOnly = true);
    Task<ServiceArea?> GetByIdAsync(string id);
    Task<ServiceArea> CreateAsync(string name, string code);
    Task<ServiceArea?> UpdateAsync(string id, string name, string code, bool isActive, int displayOrder);
    Task<bool> DeleteAsync(string id);
}
