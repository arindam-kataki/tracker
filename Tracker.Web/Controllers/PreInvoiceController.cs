using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
[Route("invoicing/pre-invoice")]
public class PreInvoiceController : BaseController
{
    private readonly IPreInvoiceService _preInvoiceService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<PreInvoiceController> _logger;

    public PreInvoiceController(
        IAuthService authService,
        IPreInvoiceService preInvoiceService,
        IServiceAreaService serviceAreaService,
        ILogger<PreInvoiceController> logger) : base(authService)
    {
        _preInvoiceService = preInvoiceService;
        _serviceAreaService = serviceAreaService;
        _logger = logger;
    }

    /// <summary>
    /// Pre Invoice page with month selector and grouped report
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(string? month = null)
    {
        // Permission scoping: get accessible service area IDs
        var serviceAreaIds = await GetAccessibleServiceAreaIdsForInvoicingAsync();

        // Get available months
        var availableMonths = await _preInvoiceService.GetAvailableMonthsAsync(
            IsSuperAdmin ? null : serviceAreaIds);

        var model = new PreInvoiceViewModel
        {
            SelectedMonth = month,
            AvailableMonths = availableMonths
        };

        // If a month is selected, generate the report
        if (!string.IsNullOrEmpty(month) && TryParseMonth(month, out var year, out var monthNum))
        {
            model.Groups = await _preInvoiceService.GeneratePreInvoiceAsync(
                year, monthNum,
                IsSuperAdmin ? null : serviceAreaIds);
        }

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "pre-invoice");
        return View("Index", model);
    }

    /// <summary>
    /// AJAX endpoint to load pre-invoice data for a selected month
    /// </summary>
    [HttpGet("data")]
    public async Task<IActionResult> GetData(string month)
    {
        if (!TryParseMonth(month, out var year, out var monthNum))
            return Json(new { success = false, message = "Invalid month format. Use yyyy-MM." });

        var serviceAreaIds = await GetAccessibleServiceAreaIdsForInvoicingAsync();

        var groups = await _preInvoiceService.GeneratePreInvoiceAsync(
            year, monthNum,
            IsSuperAdmin ? null : serviceAreaIds);

        // Flatten for JSON response
        var result = groups.Select(g => new
        {
            signIT = g.SignITReference,
            groupApprovedEffort = g.GroupApprovedEffort,
            groupHoursToLastMonth = g.GroupHoursToLastMonth,
            groupHoursThisMonth = g.GroupHoursThisMonth,
            groupHoursToDate = g.GroupHoursToDate,
            groupEffortRemaining = g.GroupEffortRemaining,
            items = g.Items.Select(i => new
            {
                enhancementId = i.EnhancementId,
                signIT = i.SignITReference,
                projectTitle = i.ProjectTitle,
                workId = i.WorkId,
                approvedEffort = i.ApprovedEffort,
                serviceArea = i.ServiceAreaCode,
                chargeCode = i.ChargeCode,
                hoursToLastMonth = i.HoursConsumedToLastMonth,
                hoursThisMonth = i.HoursConsumedThisMonth,
                hoursToDate = i.HoursConsumedToDate,
                effortRemaining = i.EffortRemaining,
                status = i.Status
            })
        });

        return Json(new { success = true, data = result });
    }

    /// <summary>
    /// Get service area IDs the current user has invoicing access to.
    /// SuperAdmin sees all; others see only service areas with ViewInvoices permission.
    /// </summary>
    private async Task<List<string>> GetAccessibleServiceAreaIdsForInvoicingAsync()
    {
        if (IsSuperAdmin)
        {
            var allAreas = await _serviceAreaService.GetAllAsync();
            return allAreas.Select(sa => sa.Id).ToList();
        }

        // For non-admin users, get service areas where they have ViewInvoices permission
        var userServiceAreas = await AuthService.GetUserServiceAreasAsync(CurrentUserId!);
        return userServiceAreas.Select(sa => sa.Id).ToList();
    }

    /// <summary>
    /// Parse "yyyy-MM" string to year and month
    /// </summary>
    private static bool TryParseMonth(string monthStr, out int year, out int month)
    {
        year = 0;
        month = 0;

        if (string.IsNullOrEmpty(monthStr) || monthStr.Length != 7)
            return false;

        var parts = monthStr.Split('-');
        if (parts.Length != 2)
            return false;

        return int.TryParse(parts[0], out year)
            && int.TryParse(parts[1], out month)
            && month >= 1 && month <= 12;
    }
}
