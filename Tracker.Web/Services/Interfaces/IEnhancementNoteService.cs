using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IEnhancementNoteService
{
    Task<List<EnhancementNote>> GetByEnhancementIdAsync(string enhancementId);
    Task<EnhancementNote?> GetByIdAsync(string id);
    Task<EnhancementNote> CreateAsync(string enhancementId, string noteText, string userId);
    Task<EnhancementNote?> UpdateAsync(string id, string noteText, string userId);
    Task<bool> DeleteAsync(string id, string userId);
}
