using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

public interface IResourceService
{
    // Legacy methods
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
    
    // New methods
    Task<List<ResourceListItem>> GetAllWithFiltersAsync(string? search, OrganizationType? orgType, string? serviceAreaId);
    Task<EditResourceViewModel?> GetForEditAsync(string id);
    Task<ResourceOperationResult> CreateResourceAsync(EditResourceViewModel model);
    Task<ResourceOperationResult> UpdateResourceAsync(EditResourceViewModel model);
    Task<ResourceOperationResult> DeleteResourceAsync(string id);
    
    // Service area memberships
    Task<ResourceOperationResult> AddServiceAreaMembershipAsync(string resourceId, string serviceAreaId, bool isPrimary, Permissions permissions);
    Task<ResourceOperationResult> RemoveServiceAreaMembershipAsync(string membershipId);
    Task<ResourceOperationResult> UpdateServiceAreaPermissionsAsync(string membershipId, Permissions permissions);
    
    // Lookups
    List<SelectListItem> GetOrganizationTypesSelectList(OrganizationType? selected = null);
    Task<List<SelectListItem>> GetServiceAreasSelectListAsync(string? selected = null);
    Task<List<ServiceAreaOption>> GetAvailableServiceAreasAsync();
    Task<List<SkillGroupViewModel>> GetSkillsGroupedByServiceAreaAsync(string? resourceId = null, List<string>? memberServiceAreaIds = null);
    
    // Permission queries
    Task<List<Resource>> GetResourcesForColumnAsync(string serviceAreaId, string columnName);
    Task<bool> HasPermissionAsync(string resourceId, string serviceAreaId, Permissions permission);
}
