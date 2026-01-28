using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

public interface IResourceService
{
    #region Resource CRUD

    Task<List<ResourceListItem>> GetAllAsync(string? search = null, string? typeFilter = null);
    Task<List<ResourceListItem>> GetAllWithFiltersAsync(string? search, OrganizationType? orgType, string? serviceAreaId);
    Task<Resource?> GetByIdAsync(string id);
    Task<EditResourceViewModel?> GetForEditAsync(string id);
    Task<ResourceOperationResult> CreateResourceAsync(EditResourceViewModel model);
    Task<ResourceOperationResult> UpdateResourceAsync(EditResourceViewModel model);
    Task<ResourceOperationResult> DeleteResourceAsync(string id);

    // Legacy methods
    Task<Resource> CreateAsync(string name, string? email, string? resourceTypeId, List<string>? skillIds = null);
    Task UpdateAsync(string id, string name, string? email, string? resourceTypeId, bool isActive, List<string>? skillIds = null);
    Task DeleteAsync(string id);
    Task<List<Resource>> GetActiveAsync();
    Task<List<Resource>> GetByResourceTypeNameAsync(string typeName);

    #endregion

    #region Authentication & Password Management

    /// <summary>
    /// Set password for a resource (creates login access)
    /// </summary>
    Task<ResourceOperationResult> SetPasswordAsync(string resourceId, string password);

    /// <summary>
    /// Reset password for a resource
    /// </summary>
    Task<ResourceOperationResult> ResetPasswordAsync(string resourceId, string newPassword);

    /// <summary>
    /// Change password (requires current password verification)
    /// </summary>
    Task<ResourceOperationResult> ChangePasswordAsync(string resourceId, string currentPassword, string newPassword);

    /// <summary>
    /// Enable or disable login access for a resource
    /// </summary>
    Task<ResourceOperationResult> SetLoginAccessAsync(string resourceId, bool hasAccess);

    /// <summary>
    /// Set admin status for a resource (with last-admin protection)
    /// </summary>
    Task<ResourceOperationResult> SetAdminStatusAsync(string resourceId, bool isAdmin);

    #endregion

    #region Service Area Memberships

    Task<ResourceOperationResult> AddServiceAreaMembershipAsync(string resourceId, string serviceAreaId, bool isPrimary, Permissions permissions);
    Task<ResourceOperationResult> RemoveServiceAreaMembershipAsync(string membershipId);
    Task<ResourceOperationResult> UpdateServiceAreaPermissionsAsync(string membershipId, Permissions permissions);

    #endregion

    #region Skills

    Task<List<SelectListItem>> GetSkillsSelectListAsync(List<string>? selectedIds = null);
    Task<List<string>> GetResourceSkillIdsAsync(string resourceId);
    Task<List<SkillGroupViewModel>> GetSkillsGroupedByServiceAreaAsync(string? resourceId = null, List<string>? memberServiceAreaIds = null);

    #endregion

    #region Lookups

    Task<List<SelectListItem>> GetResourceTypesSelectListAsync();
    List<SelectListItem> GetOrganizationTypesSelectList(OrganizationType? selected = null);
    Task<List<SelectListItem>> GetServiceAreasSelectListAsync(string? selected = null);
    Task<List<ServiceAreaOption>> GetAvailableServiceAreasAsync();

    #endregion

    #region Permission Queries

    Task<List<Resource>> GetResourcesForColumnAsync(string serviceAreaId, string columnName);
    Task<bool> HasPermissionAsync(string resourceId, string serviceAreaId, Permissions permission);

    #endregion

    #region Admin Queries

    /// <summary>
    /// Get count of active administrators
    /// </summary>
    Task<int> GetActiveAdminCountAsync();

    /// <summary>
    /// Check if resource is the last active admin
    /// </summary>
    Task<bool> IsLastAdminAsync(string resourceId);

    #endregion

    #region Reporting Hierarchy

    /// <summary>
    /// Gets resources that can be selected as a manager for a given service area.
    /// Excludes the specified resource (can't report to yourself).
    /// Only includes active resources with membership in the specified service area.
    /// </summary>
    /// <param name="serviceAreaId">The service area to find managers for</param>
    /// <param name="excludeResourceId">Resource to exclude from results (typically the resource being edited)</param>
    /// <returns>SelectListItems for dropdown population</returns>
    Task<List<SelectListItem>> GetPotentialManagersAsync(string serviceAreaId, string? excludeResourceId = null);

    /// <summary>
    /// Validates that a ReportsTo assignment doesn't create a circular reference.
    /// </summary>
    /// <param name="resourceId">The resource who will report to someone</param>
    /// <param name="serviceAreaId">The service area context</param>
    /// <param name="reportsToResourceId">The proposed manager</param>
    /// <returns>True if the assignment is valid (no circular reference), false otherwise</returns>
    Task<bool> ValidateReportsToAsync(string resourceId, string serviceAreaId, string? reportsToResourceId);

    /// <summary>
    /// Gets the direct reports for a resource within a service area.
    /// </summary>
    /// <param name="resourceId">The manager's resource ID</param>
    /// <param name="serviceAreaId">The service area context</param>
    /// <returns>List of resources who report to this manager in the specified service area</returns>
    Task<List<ResourceListItem>> GetDirectReportsAsync(string resourceId, string serviceAreaId);

    /// <summary>
    /// Gets the full reporting chain (hierarchy) for a resource within a service area.
    /// Returns list from immediate manager up to top of hierarchy.
    /// </summary>
    /// <param name="resourceId">The resource to get the chain for</param>
    /// <param name="serviceAreaId">The service area context</param>
    /// <returns>List of (ResourceId, Name) tuples representing the management chain</returns>
    Task<List<(string ResourceId, string Name)>> GetReportingChainAsync(string resourceId, string serviceAreaId);

    #endregion

    // Add these methods to IResourceService.cs in the #region Reporting Hierarchy section:

    /// <summary>
    /// Gets all direct reports for a resource across all service areas.
    /// </summary>
    /// <param name="resourceId">The manager's resource ID</param>
    /// <returns>List of resources who report directly to this manager</returns>
    Task<List<Resource>> GetAllDirectReportsAsync(string resourceId);

    /// <summary>
    /// Gets all resources in the reporting chain downwards (direct + indirect reports) recursively.
    /// Includes the manager themselves.
    /// </summary>
    /// <param name="resourceId">The manager's resource ID</param>
    /// <param name="serviceAreaIds">Optional: limit to specific service areas</param>
    /// <returns>List of all resources in the reporting chain (including self)</returns>
    Task<List<Resource>> GetReportingChainDownwardsAsync(string resourceId, List<string>? serviceAreaIds = null);

    /// <summary>
    /// Checks if a resource has any direct reports in any service area.
    /// Used for access control to Team Timesheet view.
    /// </summary>
    /// <param name="resourceId">The resource ID to check</param>
    /// <returns>True if the resource has at least one direct report</returns>
    Task<bool> HasDirectReportsAsync(string resourceId);

    /// <summary>
    /// Checks if a resource has any direct reports in the specified service areas.
    /// </summary>
    /// <param name="resourceId">The resource ID to check</param>
    /// <param name="serviceAreaIds">Service areas to check within</param>
    /// <returns>True if the resource has at least one direct report in any of the specified service areas</returns>
    Task<bool> HasDirectReportsInServiceAreasAsync(string resourceId, List<string> serviceAreaIds);
}
