using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

public interface IEnhancementService
{
    Task<List<Enhancement>> GetByServiceAreaAsync(string serviceAreaId, string? statusFilter = null, string? searchTerm = null);
    Task<PagedResult<Enhancement>> GetByServiceAreaPagedAsync(string serviceAreaId, EnhancementFilterViewModel filter);
    Task<Enhancement?> GetByIdAsync(string id);
    Task<Enhancement?> GetByWorkIdAsync(string workId);
    Task<List<Enhancement>> FindMatchesAsync(string workId, string? description, string serviceAreaId);
    Task<Enhancement> CreateAsync(Enhancement enhancement, string userId);
    Task<Enhancement?> UpdateAsync(Enhancement enhancement, string userId);
    Task<bool> DeleteAsync(string id, string userId);
    Task<bool> BulkUpdateStatusAsync(List<string> ids, string status, string userId);
    Task<bool> BulkUpdateAsync(BulkUpdateRequest request, string userId);
    Task<EstimationBreakdown?> GetBreakdownAsync(string enhancementId);
    Task<EstimationBreakdown> SaveBreakdownAsync(EstimationBreakdown breakdown);
    Task<List<string>> GetDistinctStatusesAsync();
    Task UpdateSponsorsAsync(string enhancementId, List<string>? sponsorIds, string userId);
    Task UpdateSpocsAsync(string enhancementId, List<string>? spocIds, string userId);
    Task UpdateResourcesAsync(string enhancementId, List<string>? resourceIds, string userId);
    Task<int> UpdateNotificationRecipientsAsync(string enhancementId, List<string> recipientIds, string userId);

}
