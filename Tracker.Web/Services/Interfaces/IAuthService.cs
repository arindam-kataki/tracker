using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IAuthService
{
    Task<User?> ValidateCredentialsAsync(string email, string password);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<List<ServiceArea>> GetUserServiceAreasAsync(string userId);
    Task<bool> HasAccessToServiceAreaAsync(string userId, string serviceAreaId);
}
