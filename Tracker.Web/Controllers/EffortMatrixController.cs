using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
[Route("invoicing/effort-matrix")]
public class EffortMatrixController : BaseController
{
    private readonly IEffortMatrixService _effortMatrixService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<EffortMatrixController> _logger;

    public EffortMatrixController(
        IAuthService authService,
        IEffortMatrixService effortMatrixService,
        IServiceAreaService serviceAreaService,
        ILogger<EffortMatrixController> logger) : base(authService)
    {
        _effortMatrixService = effortMatrixService;
        _serviceAreaService = serviceAreaService;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int? year = null)
    {
        var serviceAreaIds = await GetAccessibleServiceAreaIdsForInvoicingAsync();

        var availableYears = await _effortMatrixService.GetAvailableYearsAsync(
            IsSuperAdmin ? null : serviceAreaIds);

        var selectedYear = year ?? (availableYears.Any() ? availableYears.First() : DateTime.Today.Year);

        var model = new EffortMatrixViewModel
        {
            SelectedYear = selectedYear,
            AvailableYears = availableYears
        };

        if (availableYears.Contains(selectedYear))
        {
            model.Rows = await _effortMatrixService.GenerateMatrixAsync(
                selectedYear, IsSuperAdmin ? null : serviceAreaIds);

            // Compute monthly totals
            for (int m = 0; m < 12; m++)
            {
                model.MonthlyTotals[m] = model.Rows.Sum(r => r.MonthlyHours[m]);
            }
            model.GrandTotal = model.Rows.Sum(r => r.RowTotal);
        }

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "effort-matrix");
        return View("Index", model);
    }

    /// <summary>
    /// AJAX endpoint for year change
    /// </summary>
    [HttpGet("data")]
    public async Task<IActionResult> GetData(int year)
    {
        var serviceAreaIds = await GetAccessibleServiceAreaIdsForInvoicingAsync();

        var rows = await _effortMatrixService.GenerateMatrixAsync(
            year, IsSuperAdmin ? null : serviceAreaIds);

        var monthlyTotals = new decimal[12];
        for (int m = 0; m < 12; m++)
            monthlyTotals[m] = rows.Sum(r => r.MonthlyHours[m]);

        var result = new
        {
            success = true,
            data = rows.Select(r => new
            {
                signIT = r.SignITReference,
                projectTitle = r.ProjectTitle,
                resourceName = r.ResourceName,
                serviceArea = r.ServiceAreaCode,
                chargeCode = r.ChargeCode,
                months = r.MonthlyHours,
                rowTotal = r.RowTotal
            }),
            monthlyTotals,
            grandTotal = rows.Sum(r => r.RowTotal)
        };

        return Json(result);
    }

    private async Task<List<string>> GetAccessibleServiceAreaIdsForInvoicingAsync()
    {
        if (IsSuperAdmin)
        {
            var allAreas = await _serviceAreaService.GetAllAsync();
            return allAreas.Select(sa => sa.Id).ToList();
        }

        var userServiceAreas = await AuthService.GetUserServiceAreasAsync(CurrentUserId!);
        return userServiceAreas.Select(sa => sa.Id).ToList();
    }
}
