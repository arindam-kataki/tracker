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
    /// Copies all notes and adds a sharing note.
    /// </summary>
    Task<Enhancement> ShareToServiceAreaAsync(string enhancementId, string targetServiceAreaId, string userId);
}
