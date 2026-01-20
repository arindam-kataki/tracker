using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class AuthService : IAuthService
{
    private readonly TrackerDbContext _db;

    public AuthService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

        if (user == null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        return await _db.Users
            .Include(u => u.ServiceAreas)
            .ThenInclude(usa => usa.ServiceArea)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _db.Users
            .Include(u => u.ServiceAreas)
            .ThenInclude(usa => usa.ServiceArea)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<List<ServiceArea>> GetUserServiceAreasAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
            return new List<ServiceArea>();

        // SuperAdmin has access to all service areas
        if (user.Role == UserRole.SuperAdmin)
        {
            return await _db.ServiceAreas
                .Where(sa => sa.IsActive)
                .OrderBy(sa => sa.DisplayOrder)
                .ToListAsync();
        }

        return user.ServiceAreas
            .Select(usa => usa.ServiceArea)
            .Where(sa => sa.IsActive)
            .OrderBy(sa => sa.DisplayOrder)
            .ToList();
    }

    public async Task<bool> HasAccessToServiceAreaAsync(string userId, string serviceAreaId)
    {
        var user = await GetUserByIdAsync(userId);
        
        if (user == null)
            return false;

        // SuperAdmin has access to all
        if (user.Role == UserRole.SuperAdmin)
            return true;

        return user.ServiceAreas.Any(usa => usa.ServiceAreaId == serviceAreaId);
    }
}
