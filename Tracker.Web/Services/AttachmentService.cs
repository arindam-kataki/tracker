using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class AttachmentService : IAttachmentService
{
    private readonly TrackerDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<AttachmentService> _logger;

    private const string AttachmentsFolder = "uploads/attachments";
    
    // Allowed file extensions (add more as needed)
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".rtf", ".odt", ".ods", ".odp",
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".svg", ".webp",
        ".zip", ".rar", ".7z", ".tar", ".gz",
        ".xml", ".json", ".html", ".htm", ".css", ".js",
        ".sql", ".md", ".log"
    };

    public AttachmentService(TrackerDbContext db, IWebHostEnvironment env, ILogger<AttachmentService> logger)
    {
        _db = db;
        _env = env;
        _logger = logger;
    }

    public async Task<List<EnhancementAttachment>> GetByEnhancementIdAsync(string enhancementId)
    {
        return await _db.EnhancementAttachments
            .Include(a => a.UploadedByResource)
            .Where(a => a.EnhancementId == enhancementId)
            .OrderByDescending(a => a.UploadedAt)
            .ToListAsync();
    }

    public async Task<EnhancementAttachment?> GetByIdAsync(string id)
    {
        return await _db.EnhancementAttachments
            .Include(a => a.UploadedByResource)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public (bool isValid, string? errorMessage) ValidateFile(string fileName, long fileSize)
    {
        // Check file size (max 100MB)
        if (fileSize > EnhancementAttachment.MaxFileSizeBytes)
        {
            return (false, $"File size exceeds maximum allowed ({EnhancementAttachment.MaxFileSizeBytes / (1024 * 1024)}MB).");
        }

        if (fileSize == 0)
        {
            return (false, "File is empty.");
        }

        // Check extension
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return (false, $"File type '{extension}' is not allowed.");
        }

        return (true, null);
    }

    public async Task<EnhancementAttachment> UploadAsync(
        string enhancementId, 
        Stream fileStream, 
        string fileName, 
        string contentType, 
        long fileSize, 
        string userId)
    {
        // Validate
        var validation = ValidateFile(fileName, fileSize);
        if (!validation.isValid)
        {
            throw new InvalidOperationException(validation.errorMessage);
        }

        // Create directory structure
        var enhancementFolder = Path.Combine(_env.WebRootPath, AttachmentsFolder, enhancementId);
        Directory.CreateDirectory(enhancementFolder);

        // Generate unique stored filename
        var extension = Path.GetExtension(fileName);
        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var storagePath = Path.Combine(AttachmentsFolder, enhancementId, storedFileName);
        var fullPath = Path.Combine(_env.WebRootPath, storagePath);

        // Save file
        try
        {
            using var fileStreamOut = new FileStream(fullPath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamOut);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save attachment file: {FileName}", fileName);
            throw new InvalidOperationException("Failed to save file.", ex);
        }

        // Create database record
        var attachment = new EnhancementAttachment
        {
            Id = Guid.NewGuid().ToString(),
            EnhancementId = enhancementId,
            FileName = fileName,
            StoredFileName = storedFileName,
            ContentType = contentType,
            FileSize = fileSize,
            StoragePath = storagePath,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow
        };

        _db.EnhancementAttachments.Add(attachment);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Attachment uploaded: {FileName} for Enhancement {EnhancementId}", fileName, enhancementId);

        return attachment;
    }

    public async Task<(Stream? stream, string? contentType, string? fileName)> DownloadAsync(string id)
    {
        var attachment = await _db.EnhancementAttachments.FindAsync(id);
        if (attachment == null)
        {
            return (null, null, null);
        }

        var fullPath = Path.Combine(_env.WebRootPath, attachment.StoragePath);
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("Attachment file not found on disk: {Path}", fullPath);
            return (null, null, null);
        }

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return (stream, attachment.ContentType, attachment.FileName);
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var attachment = await _db.EnhancementAttachments.FindAsync(id);
        if (attachment == null)
        {
            return false;
        }

        // Delete file from disk
        var fullPath = Path.Combine(_env.WebRootPath, attachment.StoragePath);
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete attachment file: {Path}", fullPath);
                // Continue with database deletion even if file deletion fails
            }
        }

        // Delete database record
        _db.EnhancementAttachments.Remove(attachment);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Attachment deleted: {FileName} by {UserId}", attachment.FileName, userId);

        return true;
    }
}
