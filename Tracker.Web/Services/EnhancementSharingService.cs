using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class EnhancementSharingService : IEnhancementSharingService
{
    private readonly TrackerDbContext _db;
    private readonly IEnhancementService _enhancementService;
    private readonly IEnhancementNoteService _noteService;
    private readonly IResourceService _resourceService;

    public EnhancementSharingService(
        TrackerDbContext db, 
        IEnhancementService enhancementService,
        IEnhancementNoteService noteService,
        IResourceService resourceService)
    {
        _db = db;
        _enhancementService = enhancementService;
        _noteService = noteService;
        _resourceService = resourceService;
    }

    public async Task<bool> ExistsInServiceAreaAsync(string workId, string serviceAreaId)
    {
        return await _db.Enhancements
            .AnyAsync(e => e.WorkId == workId && e.ServiceAreaId == serviceAreaId);
    }

    public async Task<List<ServiceArea>> GetAvailableTargetServiceAreasAsync(string enhancementId, List<string> accessibleServiceAreaIds)
    {
        var enhancement = await _db.Enhancements.FindAsync(enhancementId);
        if (enhancement == null)
            return new List<ServiceArea>();

        // Get service areas where this WorkId doesn't exist yet
        var existingServiceAreaIds = await _db.Enhancements
            .Where(e => e.WorkId == enhancement.WorkId)
            .Select(e => e.ServiceAreaId)
            .ToListAsync();

        // Filter to accessible service areas that don't have this WorkId
        return await _db.ServiceAreas
            .Where(sa => sa.IsActive)
            .Where(sa => accessibleServiceAreaIds.Contains(sa.Id))
            .Where(sa => !existingServiceAreaIds.Contains(sa.Id))
            .OrderBy(sa => sa.DisplayOrder)
            .ThenBy(sa => sa.Name)
            .ToListAsync();
    }

    public async Task<Enhancement> ShareToServiceAreaAsync(string enhancementId, string targetServiceAreaId, string userId, string sharingNote)
    {
        // Validate sharing note
        if (string.IsNullOrWhiteSpace(sharingNote) || sharingNote.Trim().Length < 10)
        {
            throw new InvalidOperationException("A sharing note is required (minimum 10 characters).");
        }

        var source = await _db.Enhancements
            .Include(e => e.ServiceArea)
            .FirstOrDefaultAsync(e => e.Id == enhancementId);

        if (source == null)
            throw new InvalidOperationException("Source enhancement not found.");

        // Check if source is already a shared copy - don't allow sharing from shared copies
        if (!string.IsNullOrEmpty(source.SpawnedFromId))
        {
            throw new InvalidOperationException("Cannot share from a shared copy. Please share from the original work item.");
        }

        // Check for duplicate
        if (await ExistsInServiceAreaAsync(source.WorkId, targetServiceAreaId))
            throw new InvalidOperationException($"Enhancement with WorkId '{source.WorkId}' already exists in the target service area.");

        // Get target service area
        var targetServiceArea = await _db.ServiceAreas.FindAsync(targetServiceAreaId);
        if (targetServiceArea == null)
            throw new InvalidOperationException("Target service area not found.");

        // Get user info for the system sharing note
        var user = await _resourceService.GetByIdAsync(userId);
        var userName = user?.DisplayName ?? "Unknown User";

        // Create the copy with SpawnedFromId set to track origin
        var copy = new Enhancement
        {
            Id = Guid.NewGuid().ToString(),
            SpawnedFromId = source.Id,  // Track the original enhancement
            WorkId = source.WorkId,
            Description = source.Description,
            Notes = source.Notes,
            ServiceAreaId = targetServiceAreaId,
            EstimatedHours = source.EstimatedHours,
            EstimatedStartDate = source.EstimatedStartDate,
            EstimatedEndDate = source.EstimatedEndDate,
            EstimationNotes = source.EstimationNotes,
            Status = "New", // Reset status
            ServiceLine = source.ServiceLine,
            // Don't copy actual hours/dates - those are specific to original
            ReturnedHours = null,
            StartDate = null,
            EndDate = null,
            InfStatus = null,
            InfServiceLine = null,
            // Reset time allocations
            TimeW1 = null, TimeW2 = null, TimeW3 = null,
            TimeW4 = null, TimeW5 = null, TimeW6 = null,
            TimeW7 = null, TimeW8 = null, TimeW9 = null,
            // Audit
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Enhancements.Add(copy);
        await _db.SaveChangesAsync();

        // Copy notes from source (in chronological order)
        var sourceNotes = await _db.EnhancementNotes
            .Where(n => n.EnhancementId == enhancementId)
            .OrderBy(n => n.CreatedAt)
            .ToListAsync();

        foreach (var note in sourceNotes)
        {
            var noteCopy = new Note
            {
                Id = Guid.NewGuid().ToString(),
                EnhancementId = copy.Id,
                NoteText = note.NoteText,
                CreatedBy = note.CreatedBy,
                CreatedAt = note.CreatedAt,
                ModifiedBy = note.ModifiedBy,
                ModifiedAt = note.ModifiedAt
            };
            _db.EnhancementNotes.Add(noteCopy);
        }

        // Add system sharing note (records the share action)
        var systemSharingNote = new Note
        {
            Id = Guid.NewGuid().ToString(),
            EnhancementId = copy.Id,
            NoteText = $"[Shared from {source.ServiceArea.Name}] This work item was shared by {userName} on {DateTime.UtcNow:g} UTC.",
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow.AddMilliseconds(1) // Ensure it comes after copied notes
        };
        _db.EnhancementNotes.Add(systemSharingNote);

        // Add user's sharing note (the required note from the form)
        var userSharingNote = new Note
        {
            Id = Guid.NewGuid().ToString(),
            EnhancementId = copy.Id,
            NoteText = sharingNote.Trim(),
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow.AddMilliseconds(2) // Ensure it's the latest note
        };
        _db.EnhancementNotes.Add(userSharingNote);

        // Copy notification recipients from source
        var sourceRecipients = await _db.EnhancementNotificationRecipients
            .Where(r => r.EnhancementId == enhancementId)
            .ToListAsync();

        foreach (var recipient in sourceRecipients)
        {
            var recipientCopy = new EnhancementNotificationRecipient
            {
                Id = Guid.NewGuid().ToString(),
                EnhancementId = copy.Id,
                ResourceId = recipient.ResourceId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };
            _db.EnhancementNotificationRecipients.Add(recipientCopy);
        }

        await _db.SaveChangesAsync();

        return copy;
    }
}
