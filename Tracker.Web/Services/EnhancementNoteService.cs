using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class EnhancementNoteService : IEnhancementNoteService
{
    private readonly TrackerDbContext _db;

    public EnhancementNoteService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<List<Note>> GetByEnhancementIdAsync(string enhancementId)
    {
        return await _db.EnhancementNotes
            .Include(n => n.CreatedByUser)
            .Where(n => n.EnhancementId == enhancementId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Note?> GetByIdAsync(string id)
    {
        return await _db.EnhancementNotes
            .Include(n => n.CreatedByUser)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task<Note> CreateAsync(string enhancementId, string noteText, string userId)
    {
        var note = new Note
        {
            Id = Guid.NewGuid().ToString(),
            EnhancementId = enhancementId,
            NoteText = noteText,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.EnhancementNotes.Add(note);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        return (await GetByIdAsync(note.Id))!;
    }

    public async Task<Note?> UpdateAsync(string id, string noteText, string userId)
    {
        var note = await _db.EnhancementNotes.FindAsync(id);
        if (note == null)
            return null;

        note.NoteText = noteText;
        note.ModifiedBy = userId;
        note.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var note = await _db.EnhancementNotes.FindAsync(id);
        if (note == null)
            return false;

        _db.EnhancementNotes.Remove(note);
        await _db.SaveChangesAsync();
        return true;
    }
}
