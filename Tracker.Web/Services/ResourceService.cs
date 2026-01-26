using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

/// <summary>
/// Service for managing resources (users/contacts)
/// </summary>
public interface IResourceService
{
    // CRUD
    Task<List<Resource>> GetAllAsync(ResourceFilterViewModel? filter = null);
    Task<Resource?> GetByIdAsync(string id);
    Task<Resource?> GetByEmailAsync(string email);
    Task<Resource> CreateAsync(ResourceEditViewModel model);
    Task<Resource?> UpdateAsync(string id, ResourceEditViewModel model);
    Task<bool> DeleteAsync(string id);
    
    // Authentication
    Task<Resource?> AuthenticateAsync(string email, string password);
    Task<bool> ChangePasswordAsync(string resourceId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(string resourceId, string newPassword);
    
    // Service Area Management
    Task<List<ResourceServiceArea>> GetServiceAreasAsync(string resourceId);
    Task<ResourceServiceArea?> AddToServiceAreaAsync(string resourceId, string serviceAreaId, Permissions permissions, bool isPrimary = false);
    Task<bool> UpdateServiceAreaPermissionsAsync(string resourceId, string serviceAreaId, Permissions permissions);
    Task<bool> RemoveFromServiceAreaAsync(string resourceId, string serviceAreaId);
    
    // Queries
    Task<List<Resource>> GetByOrganizationTypeAsync(OrganizationType orgType, string? serviceAreaId = null);
    Task<List<Resource>> GetByServiceAreaAsync(string serviceAreaId);
    Task<bool> EmailExistsAsync(string email, string? excludeResourceId = null);
}

public class ResourceService : IResourceService
{
    private readonly TrackerDbContext _db;
    
    public ResourceService(TrackerDbContext db)
    {
        _db = db;
    }
    
    // === CRUD ===
    
    public async Task<List<Resource>> GetAllAsync(ResourceFilterViewModel? filter = null)
    {
        var query = _db.Resources
            .Include(r => r.ServiceAreas)
            .ThenInclude(sa => sa.ServiceArea)
            .AsQueryable();
        
        if (filter != null)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(r => r.Name.ToLower().Contains(search) ||
                                        (r.Email != null && r.Email.ToLower().Contains(search)));
            }
            
            if (filter.OrganizationType.HasValue)
            {
                query = query.Where(r => r.OrganizationType == filter.OrganizationType.Value);
            }
            
            if (filter.CanLogin.HasValue)
            {
                query = query.Where(r => r.CanLogin == filter.CanLogin.Value);
            }
            
            if (filter.IsActive.HasValue)
            {
                query = query.Where(r => r.IsActive == filter.IsActive.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(filter.ServiceAreaId))
            {
                query = query.Where(r => r.ServiceAreas.Any(sa => sa.ServiceAreaId == filter.ServiceAreaId));
            }
        }
        
        return await query
            .OrderBy(r => r.OrganizationType)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }
    
    public async Task<Resource?> GetByIdAsync(string id)
    {
        return await _db.Resources
            .Include(r => r.ServiceAreas)
            .ThenInclude(sa => sa.ServiceArea)
            .Include(r => r.Skills)
            .ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(r => r.Id == id);
    }
    
    public async Task<Resource?> GetByEmailAsync(string email)
    {
        return await _db.Resources
            .Include(r => r.ServiceAreas)
            .ThenInclude(sa => sa.ServiceArea)
            .FirstOrDefaultAsync(r => r.Email != null && r.Email.ToLower() == email.ToLower());
    }
    
    public async Task<Resource> CreateAsync(ResourceEditViewModel model)
    {
        var resource = new Resource
        {
            Name = model.Name.Trim(),
            Email = model.Email?.Trim(),
            Phone = model.Phone?.Trim(),
            OrganizationType = model.OrganizationType,
            CanLogin = model.CanLogin,
            IsAdmin = model.IsAdmin && model.OrganizationType == OrganizationType.Implementor,
            IsActive = model.IsActive
        };
        
        // Set password if can login
        if (model.CanLogin && !string.IsNullOrWhiteSpace(model.Password))
        {
            resource.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        }
        
        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();
        
        // Add service area memberships
        if (model.ServiceAreas != null && model.ServiceAreas.Any())
        {
            foreach (var saModel in model.ServiceAreas)
            {
                var rsa = new ResourceServiceArea
                {
                    ResourceId = resource.Id,
                    ServiceAreaId = saModel.ServiceAreaId,
                    Permissions = saModel.Permissions,
                    IsPrimary = saModel.IsPrimary
                };
                _db.ResourceServiceAreas.Add(rsa);
            }
            await _db.SaveChangesAsync();
        }
        
        return resource;
    }
    
    public async Task<Resource?> UpdateAsync(string id, ResourceEditViewModel model)
    {
        var resource = await _db.Resources
            .Include(r => r.ServiceAreas)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (resource == null) return null;
        
        resource.Name = model.Name.Trim();
        resource.Email = model.Email?.Trim();
        resource.Phone = model.Phone?.Trim();
        resource.OrganizationType = model.OrganizationType;
        resource.CanLogin = model.CanLogin;
        resource.IsAdmin = model.IsAdmin && model.OrganizationType == OrganizationType.Implementor;
        resource.IsActive = model.IsActive;
        
        // Update password if provided
        if (model.CanLogin && !string.IsNullOrWhiteSpace(model.Password))
        {
            resource.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        }
        
        // Clear password if login disabled
        if (!model.CanLogin)
        {
            resource.PasswordHash = null;
        }
        
        // Update service area memberships
        if (model.ServiceAreas != null)
        {
            // Remove existing
            _db.ResourceServiceAreas.RemoveRange(resource.ServiceAreas);
            
            // Add new
            foreach (var saModel in model.ServiceAreas)
            {
                var rsa = new ResourceServiceArea
                {
                    ResourceId = resource.Id,
                    ServiceAreaId = saModel.ServiceAreaId,
                    Permissions = saModel.Permissions,
                    IsPrimary = saModel.IsPrimary
                };
                _db.ResourceServiceAreas.Add(rsa);
            }
        }
        
        await _db.SaveChangesAsync();
        return resource;
    }
    
    public async Task<bool> DeleteAsync(string id)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null) return false;
        
        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();
        return true;
    }
    
    // === AUTHENTICATION ===
    
    public async Task<Resource?> AuthenticateAsync(string email, string password)
    {
        var resource = await _db.Resources
            .Include(r => r.ServiceAreas)
            .ThenInclude(sa => sa.ServiceArea)
            .FirstOrDefaultAsync(r => r.Email != null && 
                                      r.Email.ToLower() == email.ToLower() && 
                                      r.CanLogin && 
                                      r.IsActive);
        
        if (resource == null || string.IsNullOrEmpty(resource.PasswordHash))
            return null;
        
        if (!BCrypt.Net.BCrypt.Verify(password, resource.PasswordHash))
            return null;
        
        // Update last login
        resource.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        return resource;
    }
    
    public async Task<bool> ChangePasswordAsync(string resourceId, string currentPassword, string newPassword)
    {
        var resource = await _db.Resources.FindAsync(resourceId);
        if (resource == null || !resource.CanLogin || string.IsNullOrEmpty(resource.PasswordHash))
            return false;
        
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, resource.PasswordHash))
            return false;
        
        resource.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> ResetPasswordAsync(string resourceId, string newPassword)
    {
        var resource = await _db.Resources.FindAsync(resourceId);
        if (resource == null || !resource.CanLogin)
            return false;
        
        resource.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();
        return true;
    }
    
    // === SERVICE AREA MANAGEMENT ===
    
    public async Task<List<ResourceServiceArea>> GetServiceAreasAsync(string resourceId)
    {
        return await _db.ResourceServiceAreas
            .Include(rsa => rsa.ServiceArea)
            .Where(rsa => rsa.ResourceId == resourceId)
            .OrderByDescending(rsa => rsa.IsPrimary)
            .ThenBy(rsa => rsa.ServiceArea.Name)
            .ToListAsync();
    }
    
    public async Task<ResourceServiceArea?> AddToServiceAreaAsync(string resourceId, string serviceAreaId, Permissions permissions, bool isPrimary = false)
    {
        // Check if already exists
        var existing = await _db.ResourceServiceAreas
            .FirstOrDefaultAsync(rsa => rsa.ResourceId == resourceId && rsa.ServiceAreaId == serviceAreaId);
        
        if (existing != null) return existing;
        
        var rsa = new ResourceServiceArea
        {
            ResourceId = resourceId,
            ServiceAreaId = serviceAreaId,
            Permissions = permissions,
            IsPrimary = isPrimary
        };
        
        _db.ResourceServiceAreas.Add(rsa);
        await _db.SaveChangesAsync();
        
        return rsa;
    }
    
    public async Task<bool> UpdateServiceAreaPermissionsAsync(string resourceId, string serviceAreaId, Permissions permissions)
    {
        var rsa = await _db.ResourceServiceAreas
            .FirstOrDefaultAsync(r => r.ResourceId == resourceId && r.ServiceAreaId == serviceAreaId);
        
        if (rsa == null) return false;
        
        rsa.Permissions = permissions;
        await _db.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> RemoveFromServiceAreaAsync(string resourceId, string serviceAreaId)
    {
        var rsa = await _db.ResourceServiceAreas
            .FirstOrDefaultAsync(r => r.ResourceId == resourceId && r.ServiceAreaId == serviceAreaId);
        
        if (rsa == null) return false;
        
        _db.ResourceServiceAreas.Remove(rsa);
        await _db.SaveChangesAsync();
        return true;
    }
    
    // === QUERIES ===
    
    public async Task<List<Resource>> GetByOrganizationTypeAsync(OrganizationType orgType, string? serviceAreaId = null)
    {
        var query = _db.Resources
            .Where(r => r.IsActive && r.OrganizationType == orgType);
        
        if (!string.IsNullOrWhiteSpace(serviceAreaId))
        {
            query = query.Where(r => r.ServiceAreas.Any(sa => sa.ServiceAreaId == serviceAreaId));
        }
        
        return await query.OrderBy(r => r.Name).ToListAsync();
    }
    
    public async Task<List<Resource>> GetByServiceAreaAsync(string serviceAreaId)
    {
        return await _db.ResourceServiceAreas
            .Where(rsa => rsa.ServiceAreaId == serviceAreaId)
            .Include(rsa => rsa.Resource)
            .Where(rsa => rsa.Resource.IsActive)
            .Select(rsa => rsa.Resource)
            .OrderBy(r => r.OrganizationType)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }
    
    public async Task<bool> EmailExistsAsync(string email, string? excludeResourceId = null)
    {
        var query = _db.Resources.Where(r => r.Email != null && r.Email.ToLower() == email.ToLower());
        
        if (!string.IsNullOrWhiteSpace(excludeResourceId))
        {
            query = query.Where(r => r.Id != excludeResourceId);
        }
        
        return await query.AnyAsync();
    }
}
