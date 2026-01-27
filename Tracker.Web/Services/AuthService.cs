using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

/// <summary>
/// Authentication service - now authenticates against Resources table
/// </summary>
public class AuthService : IAuthService
{
    private readonly TrackerDbContext _db;

    public AuthService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<Resource?> ValidateCredentialsAsync(string email, string password)
    {
        var resource = await _db.Resources
            .Include(r => r.ServiceAreas)
                .ThenInclude(rsa => rsa.ServiceArea)
            .FirstOrDefaultAsync(r => 
                r.Email != null && 
                r.Email.ToLower() == email.ToLower() && 
                r.HasLoginAccess && 
                r.IsActive);

        if (resource == null)
            return null;

        // Verify password
        if (string.IsNullOrEmpty(resource.PasswordHash))
            return null;
            
        if (!BCrypt.Net.BCrypt.Verify(password, resource.PasswordHash))
            return null;

        // Update last login
        resource.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return resource;
    }

    public async Task<Resource?> GetResourceByIdAsync(string resourceId)
    {
        return await _db.Resources
            .Include(r => r.ServiceAreas)
                .ThenInclude(rsa => rsa.ServiceArea)
            .FirstOrDefaultAsync(r => r.Id == resourceId);
    }

    public async Task<Resource?> GetResourceByEmailAsync(string email)
    {
        return await _db.Resources
            .Include(r => r.ServiceAreas)
                .ThenInclude(rsa => rsa.ServiceArea)
            .FirstOrDefaultAsync(r => r.Email != null && r.Email.ToLower() == email.ToLower());
    }

    public async Task<List<ServiceArea>> GetUserServiceAreasAsync(string resourceId)
    {
        var resource = await GetResourceByIdAsync(resourceId);
        
        if (resource == null)
            return new List<ServiceArea>();

        // SuperAdmin has access to all service areas
        if (resource.IsAdmin)
        {
            return await _db.ServiceAreas
                .Where(sa => sa.IsActive)
                .OrderBy(sa => sa.DisplayOrder)
                .ToListAsync();
        }

        // Regular users only have access to their assigned service areas
        return resource.ServiceAreas
            .Where(rsa => rsa.ServiceArea != null && rsa.ServiceArea.IsActive)
            .Select(rsa => rsa.ServiceArea!)
            .OrderBy(sa => sa.DisplayOrder)
            .ToList();
    }

    public async Task<bool> HasAccessToServiceAreaAsync(string resourceId, string serviceAreaId)
    {
        var resource = await GetResourceByIdAsync(resourceId);
        
        if (resource == null)
            return false;

        // SuperAdmin has access to all
        if (resource.IsAdmin)
            return true;

        return resource.ServiceAreas.Any(rsa => rsa.ServiceAreaId == serviceAreaId);
    }

    public async Task UpdateLastLoginAsync(string resourceId)
    {
        var resource = await _db.Resources.FindAsync(resourceId);
        if (resource != null)
        {
            resource.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}
