using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IEnhancementNoteService
{
    Task<List<Note>> GetByEnhancementIdAsync(string enhancementId);
    Task<Note?> GetByIdAsync(string id);
    Task<Note> CreateAsync(string enhancementId, string noteText, string userId);
    Task<Note?> UpdateAsync(string id, string noteText, string userId);
    Task<bool> DeleteAsync(string id, string userId);
}
