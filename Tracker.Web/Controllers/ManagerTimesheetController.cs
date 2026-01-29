using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

/// <summary>
/// Controller for the Manager Timesheet feature.
/// Allows managers to view timesheets for their reporting hierarchy.
/// </summary>
[Authorize]
[Route("manager-timesheet")]
public class ManagerTimesheetController : BaseController
{
    private readonly IManagerTimesheetService _managerTimesheetService;
    private readonly IResourceService _resourceService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<ManagerTimesheetController> _logger;

    public ManagerTimesheetController(
        IAuthService authService,
        IManagerTimesheetService managerTimesheetService,
        IResourceService resourceService,
        IServiceAreaService serviceAreaService,
        ILogger<ManagerTimesheetController> logger) : base(authService)
    {
        _managerTimesheetService = managerTimesheetService;
        _resourceService = resourceService;
        _serviceAreaService = serviceAreaService;
        _logger = logger;
    }

    /// <summary>
    /// Main page - displays hierarchy tree on left, calendar + work items on right
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(int? year = null, int? month = null, string? selectedNode = null)
    {
        var resourceId = CurrentUserId!;
        var targetYear = year ?? DateTime.Today.Year;
        var targetMonth = month ?? DateTime.Today.Month;

        // Get service areas where user has ViewAllTimesheets permission
        var viewableServiceAreas = await GetViewableServiceAreasAsync(resourceId);
        var serviceAreaIds = viewableServiceAreas.Select(sa => sa.Id).ToList();

        if (!serviceAreaIds.Any())
        {
            ViewBag.Message = "You do not have permission to view team timesheets. You need ViewAllTimesheets permission in at least one service area.";
            ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "manager-timesheet");
            return View("NoPermission");
        }

        // Check if user has any reportees
        var hasReportees = await _managerTimesheetService.HasReporteesAsync(resourceId, serviceAreaIds);
        
        if (!hasReportees && !IsSuperAdmin)
        {
            ViewBag.Message = "You do not have any team members reporting to you in service areas where you have ViewAllTimesheets permission.";
            ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "manager-timesheet");
            return View("NoPermission");
        }

        // Get the view model
        var model = await _managerTimesheetService.GetViewModelAsync(
            resourceId,
            CurrentUserName ?? "Unknown",
            serviceAreaIds,
            viewableServiceAreas,
            targetYear,
            targetMonth,
            selectedNode);

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "manager-timesheet");
        return View("Index", model);
    }

    /// <summary>
    /// AJAX endpoint - get calendar data for a selected node
    /// </summary>
    [HttpGet("calendar-data")]
    public async Task<IActionResult> GetCalendarData(
        string nodeId,
        int year,
        int month)
    {
        var resourceId = CurrentUserId!;
        var serviceAreaIds = await GetViewableServiceAreaIdsAsync(resourceId);

        if (!serviceAreaIds.Any())
            return Json(new { success = false, error = "No permission" });

        var resourceIds = await _managerTimesheetService.GetResourceIdsForNodeAsync(
            nodeId, resourceId, serviceAreaIds);

        var calendarDays = await _managerTimesheetService.GetCalendarDataAsync(
            resourceIds, serviceAreaIds, year, month);

        return Json(new
        {
            success = true,
            data = calendarDays.Select(d => new
            {
                date = d.Date.ToString("yyyy-MM-dd"),
                hours = d.Hours,
                contributed = d.ContributedHours,
                isWeekend = d.IsWeekend,
                isToday = d.IsToday,
                phases = d.WorkPhaseBreakdown.Select(p => new
                {
                    code = p.WorkPhaseCode,
                    name = p.WorkPhaseName,
                    hours = p.Hours,
                    contributed = p.ContributedHours,
                    badgeClass = p.BadgeClass
                })
            })
        });
    }

    /// <summary>
    /// AJAX endpoint - get work items for a selected node
    /// </summary>
    [HttpGet("work-items")]
    public async Task<IActionResult> GetWorkItems(
        string nodeId,
        int year,
        int month)
    {
        var resourceId = CurrentUserId!;
        var serviceAreaIds = await GetViewableServiceAreaIdsAsync(resourceId);

        if (!serviceAreaIds.Any())
            return Json(new { success = false, error = "No permission" });

        var resourceIds = await _managerTimesheetService.GetResourceIdsForNodeAsync(
            nodeId, resourceId, serviceAreaIds);

        var serviceAreaGroups = await _managerTimesheetService.GetWorkItemsByServiceAreaAsync(
            resourceIds, serviceAreaIds, year, month);

        var totalHours = serviceAreaGroups.Sum(g => g.TotalHours);
        var totalContributed = serviceAreaGroups.Sum(g => g.TotalContributedHours);

        return Json(new
        {
            success = true,
            totalHours,
            totalContributed,
            resourceCount = resourceIds.Count,
            groups = serviceAreaGroups.Select(g => new
            {
                serviceAreaCode = g.ServiceAreaCode,
                serviceAreaName = g.ServiceAreaName,
                totalHours = g.TotalHours,
                totalContributed = g.TotalContributedHours,
                workItems = g.WorkItems.Select(wi => new
                {
                    workId = wi.WorkId,
                    description = wi.Description,
                    totalHours = wi.TotalHours,
                    totalContributed = wi.TotalContributedHours,
                    hasMultipleResources = wi.HasMultipleResources,
                    resources = wi.ResourceBreakdown.Select(r => new
                    {
                        name = r.ResourceName,
                        hours = r.Hours,
                        contributed = r.ContributedHours
                    })
                })
            })
        });
    }

    /// <summary>
    /// AJAX endpoint - get hierarchy tree data
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<IActionResult> GetHierarchy(int year, int month)
    {
        var resourceId = CurrentUserId!;
        var serviceAreaIds = await GetViewableServiceAreaIdsAsync(resourceId);

        if (!serviceAreaIds.Any())
            return Json(new { success = false, error = "No permission" });

        var tree = await _managerTimesheetService.BuildHierarchyTreeAsync(
            resourceId, serviceAreaIds, year, month);

        return Json(new
        {
            success = true,
            tree = SerializeTree(tree)
        });
    }

    private object SerializeTree(List<HierarchyNodeViewModel> nodes)
    {
        return nodes.Select(n => new
        {
            nodeId = n.NodeId,
            resourceId = n.ResourceId,
            name = n.Name,
            isRollup = n.IsRollup,
            isSelf = n.IsSelf,
            totalHours = n.TotalHours,
            resourceCount = n.ResourceCount,
            level = n.Level,
            children = SerializeTree(n.Children)
        });
    }

    #region Helper Methods

    private async Task<List<ServiceArea>> GetViewableServiceAreasAsync(string resourceId)
    {
        var allServiceAreas = await _serviceAreaService.GetAllAsync();
        var viewable = new List<ServiceArea>();

        foreach (var sa in allServiceAreas)
        {
            if (IsSuperAdmin || await _resourceService.HasPermissionAsync(resourceId, sa.Id, Permissions.ViewAllTimesheets))
            {
                viewable.Add(sa);
            }
        }

        return viewable;
    }

    private async Task<List<string>> GetViewableServiceAreaIdsAsync(string resourceId)
    {
        var serviceAreas = await GetViewableServiceAreasAsync(resourceId);
        return serviceAreas.Select(sa => sa.Id).ToList();
    }

    #endregion
}
