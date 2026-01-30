using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IEnhancementSharingService
{
    /// <summary>
    /// Checks if an enhancement with the same WorkId already exists in the target service area.
    /// </summary>
    Task<bool> ExistsInServiceAreaAsync(string workId, string serviceAreaId);
    
    /// <summary>
    /// Gets list of service areas where the enhancement can be shared to.
    /// Excludes the current service area and areas where WorkId already exists.
    /// </summary>
    Task<List<ServiceArea>> GetAvailableTargetServiceAreasAsync(string enhancementId, List<string> accessibleServiceAreaIds);
    
    /// <summary>
    /// Creates a copy of the enhancement in the target service area.
    /// Copies all notes, notification recipients, and adds a user-provided sharing note.
    /// </summary>
    /// <param name="enhancementId">Source enhancement ID</param>
    /// <param name="targetServiceAreaId">Target service area ID</param>
    /// <param name="userId">User performing the share</param>
    /// <param name="sharingNote">Required note from the user explaining the share</param>
    /// <returns>The newly created enhancement</returns>
    Task<Enhancement> ShareToServiceAreaAsync(string enhancementId, string targetServiceAreaId, string userId, string sharingNote);
}
