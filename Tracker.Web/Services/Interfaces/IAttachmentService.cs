using Tracker.Web.Entities;

namespace Tracker.Web.Services.Interfaces;

public interface IAttachmentService
{
    Task<List<EnhancementAttachment>> GetByEnhancementIdAsync(string enhancementId);
    Task<EnhancementAttachment?> GetByIdAsync(string id);
    Task<EnhancementAttachment> UploadAsync(string enhancementId, Stream fileStream, string fileName, string contentType, long fileSize, string userId);
    Task<(Stream? stream, string? contentType, string? fileName)> DownloadAsync(string id);
    Task<bool> DeleteAsync(string id, string userId);
    
    /// <summary>
    /// Validates file before upload.
    /// </summary>
    (bool isValid, string? errorMessage) ValidateFile(string fileName, long fileSize);
}
