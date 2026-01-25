using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
[Route("Dashboard")]
public class DashboardController : BaseController
{
    private readonly TrackerDbContext _context;
    private readonly IServiceAreaService _serviceAreaService;

    public DashboardController(
        IAuthService authService,
        TrackerDbContext context,
        IServiceAreaService serviceAreaService) : base(authService)
    {
        _context = context;
        _serviceAreaService = serviceAreaService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        // Get accessible service areas for user
        List<string> accessibleServiceAreaIds;
        if (IsSuperAdmin)
        {
            accessibleServiceAreaIds = await _context.ServiceAreas
                .Where(sa => sa.IsActive)
                .Select(sa => sa.Id)
                .ToListAsync();
        }
        else
        {
            var userServiceAreas = await AuthService.GetUserServiceAreasAsync(CurrentUserId!);
            accessibleServiceAreaIds = userServiceAreas.Select(sa => sa.Id).ToList();
        }

        // Get all enhancements for accessible service areas
        var enhancements = await _context.Enhancements
            .Include(e => e.ServiceArea)
            .Where(e => accessibleServiceAreaIds.Contains(e.ServiceAreaId))
            .ToListAsync();

        // Build summary
        var model = new DashboardViewModel
        {
            TotalEnhancements = enhancements.Count,
            NewCount = enhancements.Count(e => e.Status == "New"),
            InProgressCount = enhancements.Count(e => e.Status == "In Progress"),
            OnHoldCount = enhancements.Count(e => e.Status == "On Hold"),
            CompletedCount = enhancements.Count(e => e.Status == "Completed"),
            CancelledCount = enhancements.Count(e => e.Status == "Cancelled"),
            TotalEstimatedHours = enhancements.Sum(e => e.EstimatedHours ?? 0),
            TotalReturnedHours = enhancements.Sum(e => e.ReturnedHours ?? 0),
        };

        // By Service Area
        var serviceAreas = await _context.ServiceAreas
            .Where(sa => accessibleServiceAreaIds.Contains(sa.Id))
            .OrderBy(sa => sa.DisplayOrder)
            .ThenBy(sa => sa.Name)
            .ToListAsync();

        model.ByServiceArea = serviceAreas.Select(sa =>
        {
            var saEnhancements = enhancements.Where(e => e.ServiceAreaId == sa.Id).ToList();
            return new ServiceAreaSummary
            {
                ServiceAreaId = sa.Id,
                ServiceAreaName = sa.Name,
                TotalCount = saEnhancements.Count,
                NewCount = saEnhancements.Count(e => e.Status == "New"),
                InProgressCount = saEnhancements.Count(e => e.Status == "In Progress"),
                CompletedCount = saEnhancements.Count(e => e.Status == "Completed"),
                EstimatedHours = saEnhancements.Sum(e => e.EstimatedHours ?? 0),
                ReturnedHours = saEnhancements.Sum(e => e.ReturnedHours ?? 0)
            };
        }).ToList();

        // Recent enhancements (last 10 modified)
        model.RecentEnhancements = enhancements
            .OrderByDescending(e => e.ModifiedAt ?? e.CreatedAt)
            .Take(10)
            .Select(e => new RecentEnhancement
            {
                Id = e.Id,
                WorkId = e.WorkId,
                Description = e.Description,
                Status = e.Status,
                ServiceAreaName = e.ServiceArea?.Name ?? "Unknown",
                ServiceAreaId = e.ServiceAreaId,
                ModifiedAt = e.ModifiedAt,
                CreatedAt = e.CreatedAt
            })
            .ToList();

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "dashboard");
        return View("Index", model);
    }
}
