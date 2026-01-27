using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Validate login credentials and return the resource if valid
    /// </summary>
    Task<Resource?> ValidateCredentialsAsync(string email, string password);
    
    /// <summary>
    /// Get resource by ID (for authenticated user lookup)
    /// </summary>
    Task<Resource?> GetResourceByIdAsync(string resourceId);
    
    /// <summary>
    /// Get resource by email
    /// </summary>
    Task<Resource?> GetResourceByEmailAsync(string email);
    
    /// <summary>
    /// Get service areas accessible by a resource
    /// </summary>
    Task<List<ServiceArea>> GetUserServiceAreasAsync(string resourceId);
    
    /// <summary>
    /// Check if resource has access to a specific service area
    /// </summary>
    Task<bool> HasAccessToServiceAreaAsync(string resourceId, string serviceAreaId);
    
    /// <summary>
    /// Update last login timestamp
    /// </summary>
    Task UpdateLastLoginAsync(string resourceId);
    
    // Legacy compatibility - redirect to new methods
    Task<Resource?> GetUserByIdAsync(string userId) => GetResourceByIdAsync(userId);
    Task<Resource?> GetUserByEmailAsync(string email) => GetResourceByEmailAsync(email);
}
