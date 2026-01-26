using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;

namespace Tracker.Web.Services;

/// <summary>
/// Service for checking and managing permissions
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// Get permissions for a resource in a specific service area
    /// </summary>
    Task<ResourceServiceArea?> GetPermissionsAsync(string resourceId, string serviceAreaId);
    
    /// <summary>
    /// Get all service area IDs accessible by a resource
    /// </summary>
    Task<List<string>> GetAccessibleServiceAreaIdsAsync(string resourceId);
    
    /// <summary>
    /// Get all service areas accessible by a resource
    /// </summary>
    Task<List<ServiceArea>> GetAccessibleServiceAreasAsync(string resourceId);
    
    /// <summary>
    /// Check if a resource has a specific permission in a service area
    /// </summary>
    Task<bool> HasPermissionAsync(string resourceId, string serviceAreaId, Permissions permission);
    
    /// <summary>
    /// Check if a resource is an admin
    /// </summary>
    Task<bool> IsAdminAsync(string resourceId);
    
    /// <summary>
    /// Get effective permissions (considering admin status)
    /// </summary>
    Task<EffectivePermissions> GetEffectivePermissionsAsync(string resourceId, string serviceAreaId);
}

/// <summary>
/// Represents effective permissions for a resource in a service area
/// </summary>
public class EffectivePermissions
{
    public string ResourceId { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsMember { get; set; }
    public Permissions Permissions { get; set; } = Permissions.None;
    
    // === ENHANCEMENT ===
    public bool CanViewEnhancements => IsAdmin || Permissions.HasFlag(Entities.Permissions.ViewEnhancements);
    public bool CanEditEnhancements => IsAdmin || Permissions.HasFlag(Entities.Permissions.EditEnhancements);
    public bool CanUploadEnhancements => IsAdmin || Permissions.HasFlag(Entities.Permissions.UploadEnhancements);
    
    // === TIMESHEET ===
    public bool CanLogTimesheet => IsAdmin || Permissions.HasFlag(Entities.Permissions.LogTimesheet);
    public bool CanViewAllTimesheets => IsAdmin || Permissions.HasFlag(Entities.Permissions.ViewAllTimesheets);
    public bool CanApproveTimesheets => IsAdmin || Permissions.HasFlag(Entities.Permissions.ApproveTimesheets);
    
    // === CONSOLIDATION ===
    public bool CanViewConsolidation => IsAdmin || Permissions.HasFlag(Entities.Permissions.ViewConsolidation);
    public bool CanCreateConsolidation => IsAdmin || Permissions.HasFlag(Entities.Permissions.CreateConsolidation);
    public bool CanFinalizeConsolidation => IsAdmin || Permissions.HasFlag(Entities.Permissions.FinalizeConsolidation);
    
    // === RESOURCES ===
    public bool CanViewResources => IsAdmin || Permissions.HasFlag(Entities.Permissions.ViewResources);
    public bool CanManageResources => IsAdmin || Permissions.HasFlag(Entities.Permissions.ManageResources);
    
    // === REPORTS ===
    public bool CanViewReports => IsAdmin || Permissions.HasFlag(Entities.Permissions.ViewReports);
    
    // === ADMIN ONLY ===
    public bool CanManageUsers => IsAdmin;
    public bool CanManageServiceAreas => IsAdmin;
    public bool CanManageLookups => IsAdmin;
}

public class PermissionService : IPermissionService
{
    private readonly TrackerDbContext _db;
    
    public PermissionService(TrackerDbContext db)
    {
        _db = db;
    }
    
    public async Task<ResourceServiceArea?> GetPermissionsAsync(string resourceId, string serviceAreaId)
    {
        return await _db.ResourceServiceAreas
            .Include(rsa => rsa.ServiceArea)
            .FirstOrDefaultAsync(rsa => rsa.ResourceId == resourceId && rsa.ServiceAreaId == serviceAreaId);
    }
    
    public async Task<List<string>> GetAccessibleServiceAreaIdsAsync(string resourceId)
    {
        var resource = await _db.Resources.FindAsync(resourceId);
        if (resource == null) return new List<string>();
        
        // Admin can access all active service areas
        if (resource.IsAdmin)
        {
            return await _db.ServiceAreas
                .Where(sa => sa.IsActive)
                .Select(sa => sa.Id)
                .ToListAsync();
        }
        
        // Otherwise, only service areas they are members of
        return await _db.ResourceServiceAreas
            .Where(rsa => rsa.ResourceId == resourceId)
            .Select(rsa => rsa.ServiceAreaId)
            .ToListAsync();
    }
    
    public async Task<List<ServiceArea>> GetAccessibleServiceAreasAsync(string resourceId)
    {
        var resource = await _db.Resources.FindAsync(resourceId);
        if (resource == null) return new List<ServiceArea>();
        
        // Admin can access all active service areas
        if (resource.IsAdmin)
        {
            return await _db.ServiceAreas
                .Where(sa => sa.IsActive)
                .OrderBy(sa => sa.DisplayOrder)
                .ThenBy(sa => sa.Name)
                .ToListAsync();
        }
        
        // Otherwise, only service areas they are members of
        return await _db.ResourceServiceAreas
            .Where(rsa => rsa.ResourceId == resourceId)
            .Include(rsa => rsa.ServiceArea)
            .Select(rsa => rsa.ServiceArea)
            .Where(sa => sa.IsActive)
            .OrderBy(sa => sa.DisplayOrder)
            .ThenBy(sa => sa.Name)
            .ToListAsync();
    }
    
    public async Task<bool> HasPermissionAsync(string resourceId, string serviceAreaId, Permissions permission)
    {
        var resource = await _db.Resources.FindAsync(resourceId);
        if (resource == null) return false;
        
        // Admin has all permissions
        if (resource.IsAdmin) return true;
        
        var rsa = await GetPermissionsAsync(resourceId, serviceAreaId);
        return rsa?.HasPermission(permission) ?? false;
    }
    
    public async Task<bool> IsAdminAsync(string resourceId)
    {
        var resource = await _db.Resources.FindAsync(resourceId);
        return resource?.IsAdmin ?? false;
    }
    
    public async Task<EffectivePermissions> GetEffectivePermissionsAsync(string resourceId, string serviceAreaId)
    {
        var resource = await _db.Resources.FindAsync(resourceId);
        if (resource == null)
        {
            return new EffectivePermissions
            {
                ResourceId = resourceId,
                ServiceAreaId = serviceAreaId,
                IsAdmin = false,
                IsMember = false,
                Permissions = Permissions.None
            };
        }
        
        // Admin gets full permissions
        if (resource.IsAdmin)
        {
            return new EffectivePermissions
            {
                ResourceId = resourceId,
                ServiceAreaId = serviceAreaId,
                IsAdmin = true,
                IsMember = true,
                Permissions = (Permissions)(-1) // All flags set
            };
        }
        
        // Get SA-specific permissions
        var rsa = await GetPermissionsAsync(resourceId, serviceAreaId);
        
        return new EffectivePermissions
        {
            ResourceId = resourceId,
            ServiceAreaId = serviceAreaId,
            IsAdmin = false,
            IsMember = rsa != null,
            Permissions = rsa?.Permissions ?? Permissions.None
        };
    }
}
