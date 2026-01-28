using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
public class ReportsController : BaseController
{
    private readonly IReportService _reportService;
    private readonly IExcelExportService _excelExportService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ISavedFilterService _savedFilterService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IAuthService authService,
        IReportService reportService,
        IExcelExportService excelExportService,
        IServiceAreaService serviceAreaService,
        ISavedFilterService savedFilterService,
        ILogger<ReportsController> logger) : base(authService)
    {
        _reportService = reportService;
        _excelExportService = excelExportService;
        _serviceAreaService = serviceAreaService;
        _savedFilterService = savedFilterService;
        _logger = logger;
    }

    /// <summary>
    /// Index page showing list of user's named reports.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var accessibleServiceAreaIds = await GetAccessibleServiceAreaIdsAsync();
        var reports = await _reportService.GetAccessibleReportsAsync(CurrentUserId!, IsSuperAdmin, accessibleServiceAreaIds);
        var serviceAreas = await GetAccessibleServiceAreasAsync();

        // Map service area IDs to names for display
        var serviceAreaDict = serviceAreas.ToDictionary(sa => sa.Id, sa => sa.Name);

        var model = new ReportsIndexViewModel
        {
            Reports = reports.Select(r => 
            {
                var saIds = DeserializeList(r.ServiceAreaIdsJson);
                var columns = DeserializeList(r.ColumnsJson);
                
                return new NamedReportListItem
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    ServiceAreaNames = saIds
                        .Where(id => serviceAreaDict.ContainsKey(id))
                        .Select(id => serviceAreaDict[id])
                        .ToList(),
                    ColumnCount = columns.Count,
                    CreatedAt = r.CreatedAt,
                    ModifiedAt = r.ModifiedAt
                };
            }).ToList(),
            AvailableServiceAreas = serviceAreas,
            IsSuperAdmin = IsSuperAdmin
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(null, "reports");
        return View(model);
    }

    /// <summary>
    /// Create new report - show edit form.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var serviceAreas = await GetAccessibleServiceAreasAsync();
        
        var model = new NamedReportEditViewModel
        {
            AvailableServiceAreas = serviceAreas.Select(sa => new ServiceAreaOption
            {
                Id = sa.Id,
                Name = sa.Name,
                IsSelected = false
            }).ToList(),
            AvailableColumns = NamedReportEditViewModel.GetAllReportColumns(),
            SelectedColumns = NamedReportEditViewModel.GetDefaultColumnKeys()
        };

        // Mark default columns as selected
        foreach (var col in model.AvailableColumns)
        {
            col.IsSelected = model.SelectedColumns.Contains(col.Key);
        }

        // Get filter options (aggregated across all accessible service areas)
        await PopulateFilterOptions(model, serviceAreas.Select(sa => sa.Id).ToList());

        ViewBag.Sidebar = await GetSidebarViewModelAsync(null, "reports");
        return View("Edit", model);
    }

    /// <summary>
    /// Edit existing report.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var report = await _reportService.GetByIdAsync(id);
        if (report == null)
            return NotFound();

        // Check access
        var accessibleServiceAreaIds = await GetAccessibleServiceAreaIdsAsync();
        if (!await _reportService.UserCanAccessReportAsync(id, CurrentUserId!, IsSuperAdmin, accessibleServiceAreaIds))
            return RedirectToAction("AccessDenied", "Account");

        var serviceAreas = await GetAccessibleServiceAreasAsync();
        var model = NamedReportEditViewModel.FromReport(report, serviceAreas);

        // Update available service areas based on what user can access
        model.AvailableServiceAreas = serviceAreas.Select(sa => new ServiceAreaOption
        {
            Id = sa.Id,
            Name = sa.Name,
            IsSelected = model.SelectedServiceAreaIds.Contains(sa.Id)
        }).ToList();

        // Get filter options
        await PopulateFilterOptions(model, serviceAreas.Select(sa => sa.Id).ToList());

        ViewBag.Sidebar = await GetSidebarViewModelAsync(null, "reports");
        return View(model);
    }

    /// <summary>
    /// Save (create or update) a report.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] SaveReportRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Json(new { success = false, error = "Report name is required." });
            }

            if (request.ServiceAreaIds == null || !request.ServiceAreaIds.Any())
            {
                return Json(new { success = false, error = "At least one service area must be selected." });
            }

            if (request.Columns == null || !request.Columns.Any())
            {
                return Json(new { success = false, error = "At least one column must be selected." });
            }

            // Validate user has access to selected service areas
            var accessibleServiceAreaIds = await GetAccessibleServiceAreaIdsAsync();
            if (!IsSuperAdmin)
            {
                var invalidAreas = request.ServiceAreaIds.Where(id => !accessibleServiceAreaIds.Contains(id)).ToList();
                if (invalidAreas.Any())
                {
                    return Json(new { success = false, error = "You don't have access to one or more selected service areas." });
                }
            }

            var model = new NamedReportEditViewModel
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                SelectedServiceAreaIds = request.ServiceAreaIds,
                Filter = request.Filter ?? new EnhancementFilterViewModel(),
                SelectedColumns = request.Columns
            };

            if (string.IsNullOrEmpty(request.Id))
            {
                // Create new
                var report = await _reportService.CreateReportAsync(CurrentUserId!, model);
                _logger.LogInformation("Report '{Name}' created by user {UserId}", request.Name, CurrentUserId);
                return Json(new { success = true, id = report.Id, message = "Report created successfully." });
            }
            else
            {
                // Update existing
                var existingReport = await _reportService.GetByIdAsync(request.Id);
                if (existingReport == null || existingReport.ResourceId != CurrentUserId)
                {
                    return Json(new { success = false, error = "Report not found or access denied." });
                }

                await _reportService.UpdateReportAsync(request.Id, model);
                _logger.LogInformation("Report '{Name}' updated by user {UserId}", request.Name, CurrentUserId);
                return Json(new { success = true, id = request.Id, message = "Report updated successfully." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving report");
            return Json(new { success = false, error = "An error occurred while saving the report." });
        }
    }

    /// <summary>
    /// Delete a report.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _reportService.DeleteReportAsync(id, CurrentUserId!);
        if (!result)
        {
            return Json(new { success = false, error = "Report not found or access denied." });
        }

        _logger.LogInformation("Report {ReportId} deleted by user {UserId}", id, CurrentUserId);
        return Json(new { success = true });
    }

    /// <summary>
    /// Run a report - view results.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Run(string id, int page = 1, int pageSize = 25, string? sortColumn = null, string? sortOrder = null)
    {
        var accessibleServiceAreaIds = await GetAccessibleServiceAreaIdsAsync();
        
        if (!await _reportService.UserCanAccessReportAsync(id, CurrentUserId!, IsSuperAdmin, accessibleServiceAreaIds))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var options = new ReportRunOptions
        {
            Page = page,
            PageSize = pageSize,
            SortColumn = sortColumn,
            SortOrder = sortOrder
        };

        var result = await _reportService.RunReportAsync(id, CurrentUserId!, IsSuperAdmin, accessibleServiceAreaIds, options);

        ViewBag.Sidebar = await GetSidebarViewModelAsync(null, "reports");
        return View(result);
    }

    /// <summary>
    /// Export report to Excel.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Export(string id)
    {
        var accessibleServiceAreaIds = await GetAccessibleServiceAreaIdsAsync();
        
        if (!await _reportService.UserCanAccessReportAsync(id, CurrentUserId!, IsSuperAdmin, accessibleServiceAreaIds))
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        // Run report with no paging to get all results
        var options = new ReportRunOptions
        {
            Page = 1,
            PageSize = int.MaxValue
        };

        var result = await _reportService.RunReportAsync(id, CurrentUserId!, IsSuperAdmin, accessibleServiceAreaIds, options);

        // Generate Excel
        var excelBytes = _excelExportService.ExportToExcel(result.Items, result.Columns, result.IncludeServiceAreaColumn);
        var filename = _excelExportService.GetExportFilename(result.ReportName);

        _logger.LogInformation("Report '{Name}' exported by user {UserId}, {Count} rows", result.ReportName, result.Items.Count, CurrentUserId);

        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    // Helper methods

    private async Task<List<Entities.ServiceArea>> GetAccessibleServiceAreasAsync()
    {
        if (IsSuperAdmin)
        {
            return await _serviceAreaService.GetAllAsync();
        }
        return await AuthService.GetUserServiceAreasAsync(CurrentUserId!);
    }

    private async Task<List<string>> GetAccessibleServiceAreaIdsAsync()
    {
        var areas = await GetAccessibleServiceAreasAsync();
        return areas.Select(sa => sa.Id).ToList();
    }

    private async Task PopulateFilterOptions(NamedReportEditViewModel model, List<string> serviceAreaIds)
    {
        // Get distinct values across all accessible service areas
        var allStatuses = new HashSet<string>();
        var allInfStatuses = new HashSet<string>();
        var allServiceLines = new HashSet<string>();

        foreach (var saId in serviceAreaIds)
        {
            var statuses = await _savedFilterService.GetDistinctStatusesAsync(saId);
            var infStatuses = await _savedFilterService.GetDistinctInfStatusesAsync(saId);
            var serviceLines = await _savedFilterService.GetDistinctServiceLinesAsync(saId);

            foreach (var s in statuses) allStatuses.Add(s);
            foreach (var s in infStatuses) allInfStatuses.Add(s);
            foreach (var s in serviceLines) allServiceLines.Add(s);
        }

        model.AvailableStatuses = allStatuses.OrderBy(s => s).ToList();
        model.AvailableInfStatuses = allInfStatuses.OrderBy(s => s).ToList();
        model.AvailableServiceLines = allServiceLines.OrderBy(s => s).ToList();

        // Ensure we have some default statuses if none exist
        if (!model.AvailableStatuses.Any())
        {
            model.AvailableStatuses = new List<string> { "New", "In Progress", "On Hold", "Completed", "Cancelled" };
        }
    }

    private static List<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
