using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetAllUsersAsync();
    Task<User?> GetByIdAsync(string id);
    Task<User> CreateUserAsync(string email, string displayName, string password, UserRole role);
    Task<User?> UpdateUserAsync(string id, string displayName, UserRole role, bool isActive);
    Task<bool> ResetPasswordAsync(string userId, string newPassword);
    Task<bool> UpdateUserServiceAreasAsync(string userId, List<string> serviceAreaIds);
    Task<bool> DeleteUserAsync(string userId);
}
