using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

public interface IResourceService
{
    Task<List<ResourceListItem>> GetAllAsync(string? search = null, string? typeFilter = null);
    Task<Resource?> GetByIdAsync(string id);
    Task<Resource> CreateAsync(string name, string? email, string? resourceTypeId, List<string>? skillIds = null);
    Task UpdateAsync(string id, string name, string? email, string? resourceTypeId, bool isActive, List<string>? skillIds = null);
    Task DeleteAsync(string id);
    Task<List<Resource>> GetActiveAsync();
    Task<List<SelectListItem>> GetResourceTypesSelectListAsync();
    Task<List<SelectListItem>> GetSkillsSelectListAsync(List<string>? selectedIds = null);
    Task<List<string>> GetResourceSkillIdsAsync(string resourceId);
    Task<List<Resource>> GetByResourceTypeNameAsync(string typeName);
}
