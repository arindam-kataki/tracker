using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
public class UploadController : BaseController
{
    private readonly IUploadService _uploadService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(
        IAuthService authService,
        IUploadService uploadService,
        IServiceAreaService serviceAreaService,
        ILogger<UploadController> logger) : base(authService)
    {
        _uploadService = uploadService;
        _serviceAreaService = serviceAreaService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? serviceAreaId)
    {
        var serviceAreas = await AuthService.GetUserServiceAreasAsync(CurrentUserId!);
        
        if (string.IsNullOrEmpty(serviceAreaId) && serviceAreas.Any())
        {
            serviceAreaId = serviceAreas.First().Id;
        }

        var serviceArea = serviceAreaId != null 
            ? await _serviceAreaService.GetByIdAsync(serviceAreaId) 
            : null;

        var model = new UploadViewModel
        {
            ServiceAreaId = serviceAreaId ?? string.Empty,
            ServiceAreaName = serviceArea?.Name ?? string.Empty
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(serviceAreaId, "upload");
        ViewBag.ServiceAreas = serviceAreas;
        return View("Upload", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Parse(IFormFile file, string serviceAreaId)
    {
        // Check access
        if (!IsSuperAdmin && !await AuthService.HasAccessToServiceAreaAsync(CurrentUserId!, serviceAreaId))
        {
            return Forbid();
        }

        if (file == null || file.Length == 0)
        {
            return Json(new { success = false, error = "Please select a file to upload." });
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return Json(new { success = false, error = "Only CSV files are supported." });
        }

        using var stream = file.OpenReadStream();
        var result = await _uploadService.ParseCsvAsync(stream, serviceAreaId);

        if (!result.Success)
        {
            return Json(new { success = false, error = result.ErrorMessage });
        }

        // Number the rows
        for (int i = 0; i < result.Rows.Count; i++)
        {
            result.Rows[i].RowNumber = i + 1;
        }

        _logger.LogInformation("CSV file parsed with {Count} rows by {User}", result.Rows.Count, CurrentUserEmail);

        return Json(new { success = true, rows = result.Rows });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import([FromBody] ImportRequest request)
    {
        // Check access
        if (!IsSuperAdmin && !await AuthService.HasAccessToServiceAreaAsync(CurrentUserId!, request.ServiceAreaId))
        {
            return Forbid();
        }

        var imported = await _uploadService.ImportEnhancementsAsync(request.Rows, request.ServiceAreaId, CurrentUserId!);
        _logger.LogInformation("Imported {Count} enhancements to service area {ServiceArea} by {User}", 
            imported, request.ServiceAreaId, CurrentUserEmail);

        return Json(new { success = true, importedCount = imported });
    }
}

public class ImportRequest
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public List<UploadRowViewModel> Rows { get; set; } = new();
}
