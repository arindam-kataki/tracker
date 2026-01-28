namespace Tracker.Web.Entities;

/// <summary>
/// Represents a file attachment for an enhancement.
/// Files are stored in wwwroot/uploads/attachments/{enhancementId}/
/// </summary>
public class EnhancementAttachment
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string EnhancementId { get; set; } = string.Empty;
    
    /// <summary>
    /// Original filename as uploaded by the user.
    /// </summary>
    public string FileName { get; set; } = string.Empty;
    
    /// <summary>
    /// Stored filename (may be different to avoid conflicts).
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;
    
    /// <summary>
    /// MIME content type (e.g., "application/pdf", "image/png").
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
    
    /// <summary>
    /// File size in bytes. Max allowed: 100MB (104857600 bytes).
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Relative path to file from wwwroot.
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;
    
    public string? UploadedBy { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual Resource? UploadedByResource { get; set; }
    
    // Constants
    public const long MaxFileSizeBytes = 104857600; // 100MB
}
