using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class UserService : IUserService
{
    private readonly TrackerDbContext _db;

    public UserService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _db.Users
            .Include(u => u.ServiceAreas)
            .ThenInclude(usa => usa.ServiceArea)
            .OrderBy(u => u.DisplayName)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _db.Users
            .Include(u => u.ServiceAreas)
            .ThenInclude(usa => usa.ServiceArea)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> CreateUserAsync(string email, string displayName, string password, UserRole role)
    {
        var user = new User
        {
            Email = email,
            DisplayName = displayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return user;
    }

    public async Task<User?> UpdateUserAsync(string id, string displayName, UserRole role, bool isActive)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return null;

        user.DisplayName = displayName;
        user.Role = role;
        user.IsActive = isActive;

        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> ResetPasswordAsync(string userId, string newPassword)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateUserServiceAreasAsync(string userId, List<string> serviceAreaIds)
    {
        var user = await _db.Users
            .Include(u => u.ServiceAreas)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return false;

        // Remove existing
        _db.UserServiceAreas.RemoveRange(user.ServiceAreas);

        // Add new
        foreach (var saId in serviceAreaIds)
        {
            _db.UserServiceAreas.Add(new UserServiceArea
            {
                UserId = userId,
                ServiceAreaId = saId
            });
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return false;

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return true;
    }
}
