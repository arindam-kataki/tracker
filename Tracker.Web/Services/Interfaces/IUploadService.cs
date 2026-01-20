using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

public interface IUploadService
{
    Task<UploadPreviewResult> ParseCsvAsync(Stream fileStream, string serviceAreaId);
    Task<int> ImportEnhancementsAsync(List<UploadRowViewModel> rows, string serviceAreaId, string userId);
}

public class UploadPreviewResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<UploadRowViewModel> Rows { get; set; } = new();
}
