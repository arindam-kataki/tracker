using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
[Route("timesheet")]
public class TimesheetController : BaseController
{
    private readonly ITimesheetService _timesheetService;
    private readonly IWorkPhaseService _workPhaseService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<TimesheetController> _logger;

    public TimesheetController(
        IAuthService authService,
        ITimesheetService timesheetService,
        IWorkPhaseService workPhaseService,
        IServiceAreaService serviceAreaService,
        ILogger<TimesheetController> logger) : base(authService)
    {
        _timesheetService = timesheetService;
        _workPhaseService = workPhaseService;
        _serviceAreaService = serviceAreaService;
        _logger = logger;
    }

    /// <summary>
    /// My Timesheet - Calendar view
    /// </summary>
    [HttpGet("")]
    [HttpGet("my")]
    public async Task<IActionResult> MyTimesheet(int? year = null, int? month = null)
    {
        var resourceId = CurrentUserId!;
        
        // Check if user has timesheet permission for any service area
        var permittedServiceAreas = await _timesheetService.GetServiceAreasWithTimesheetPermissionAsync(resourceId);
        
        if (!permittedServiceAreas.Any() && !IsSuperAdmin)
        {
            ViewBag.Message = "You do not have permission to log timesheets. Please contact an administrator.";
            ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "timesheet");
            return View("NoPermission");
        }

        var targetDate = new DateTime(year ?? DateTime.Today.Year, month ?? DateTime.Today.Month, 1);
        
        // Get available work phases
        var workPhases = await _workPhaseService.GetForTimeRecordingAsync();

        var model = new MyTimesheetViewModel
        {
            ResourceId = resourceId,
            ResourceName = CurrentUserName ?? "Unknown",
            StartDate = targetDate,
            EndDate = targetDate.AddMonths(1).AddDays(-1),
            WorkPhases = workPhases,
            AvailableServiceAreas = permittedServiceAreas,
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "timesheet");
        return View("MyTimesheetCalendar", model);
    }

    /// <summary>
    /// Get calendar grid partial for a specific month
    /// </summary>
    [HttpGet("calendar-grid")]
    public async Task<IActionResult> GetCalendarGrid(int year, int month)
    {
        var model = new MyTimesheetViewModel
        {
            StartDate = new DateTime(year, month, 1)
        };
        return PartialView("_CalendarGrid", model);
    }

    /// <summary>
    /// Get calendar data (hours per day) for an enhancement in a month
    /// </summary>
    [HttpGet("calendar-data")]
    public async Task<IActionResult> GetCalendarData(string enhancementId, int year, int month)
    {
        var resourceId = CurrentUserId!;
        
        // Verify permission
        var canLog = await _timesheetService.CanLogTimeForEnhancementAsync(resourceId, enhancementId);
        if (!canLog && !IsSuperAdmin)
            return Forbid();

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        var entries = await _timesheetService.GetEntriesAsync(
            enhancementId: enhancementId,
            resourceId: resourceId,
            startDate: startDate,
            endDate: endDate);

        // Group by date
        var dayData = entries
            .GroupBy(e => e.StartDate.ToString("yyyy-MM-dd"))
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    hours = g.Sum(e => e.Hours),
                    contributed = g.Sum(e => e.ContributedHours),
                    count = g.Count()
                });

        // Summary
        var summary = new
        {
            totalHours = entries.Sum(e => e.Hours),
            contributedHours = entries.Sum(e => e.ContributedHours),
            daysWorked = dayData.Count,
            byWorkType = entries
                .GroupBy(e => e.WorkPhase?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours))
        };

        return Json(new { dayData, summary });
    }

    /// <summary>
    /// Get entries for a specific day and enhancement
    /// </summary>
    [HttpGet("day-entries")]
    public async Task<IActionResult> GetDayEntries(string enhancementId, DateTime date)
    {
        var resourceId = CurrentUserId!;
        
        var entries = await _timesheetService.GetEntriesAsync(
            enhancementId: enhancementId,
            resourceId: resourceId,
            startDate: date.Date,
            endDate: date.Date);

        return Json(entries.Select(e => new
        {
            id = e.Id,
            workPhaseId = e.WorkPhaseId,
            workPhaseName = e.WorkPhase?.Name,
            hours = e.Hours,
            contributedHours = e.ContributedHours,
            notes = e.Notes,
            isConsolidated = e.ConsolidationSources?.Any() == true
        }));
    }

    /// <summary>
    /// Get month overview (all enhancements for current user)
    /// </summary>
    [HttpGet("month-overview")]
    public async Task<IActionResult> GetMonthOverview(int year, int month)
    {
        var resourceId = CurrentUserId!;
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var entries = await _timesheetService.GetEntriesForResourceAsync(resourceId, startDate, endDate);

        var byEnhancement = entries
            .GroupBy(e => new { e.EnhancementId, e.Enhancement?.WorkId, e.Enhancement?.Description })
            .Select(g => new
            {
                id = g.Key.EnhancementId,
                workId = g.Key.WorkId,
                description = g.Key.Description,
                hours = g.Sum(e => e.Hours)
            })
            .OrderByDescending(e => e.hours)
            .ToList();

        return Json(new
        {
            enhancements = byEnhancement,
            totalHours = entries.Sum(e => e.Hours)
        });
    }

    /// <summary>
    /// Get enhancements for dropdown (AJAX) - filtered by resource's permissions
    /// </summary>
    [HttpGet("enhancements")]
    public async Task<IActionResult> GetEnhancements(
        string? serviceAreaId = null,
        string? status = null,
        string? workIdSearch = null,
        string? descriptionSearch = null,
        string? tagSearch = null,
        DateTime? startDateFrom = null,
        DateTime? startDateTo = null)
    {
        var resourceId = CurrentUserId!;
        
        var enhancements = await _timesheetService.GetEnhancementsForTimesheetAsync(
            resourceId,
            serviceAreaId,
            status,
            workIdSearch,
            descriptionSearch,
            tagSearch,
            startDateFrom,
            startDateTo);

        return Json(enhancements.Select(e => new 
        {
            id = e.Id,
            workId = e.WorkId,
            description = e.Description?.Length > 80 ? e.Description.Substring(0, 80) + "..." : e.Description,
            serviceAreaCode = e.ServiceArea?.Code,
            status = e.Status ?? e.InfStatus,
            tags = e.Tags
        }));
    }

    /// <summary>
    /// Get the add/edit time entry modal (legacy)
    /// </summary>
    [HttpGet("entry/edit")]
    public async Task<IActionResult> EditEntry(string? id = null, string? enhancementId = null)
    {
        var resourceId = CurrentUserId!;

        // Get service areas with permission
        var permittedServiceAreas = await _timesheetService.GetServiceAreasWithTimesheetPermissionAsync(resourceId);
        
        if (!permittedServiceAreas.Any() && !IsSuperAdmin)
            return BadRequest("You do not have permission to log timesheets.");

        // Get available work phases
        var workPhases = await _workPhaseService.GetForTimeRecordingAsync();

        var model = new TimeEntryEditViewModel
        {
            ResourceId = resourceId,
            AvailableServiceAreas = permittedServiceAreas,
            AvailableWorkPhases = workPhases,
            StartDate = DateTime.Today,
            EndDate = DateTime.Today
        };

        if (!string.IsNullOrEmpty(id))
        {
            var entry = await _timesheetService.GetEntryByIdAsync(id);
            if (entry == null)
                return NotFound();

            // Can only edit own entries (unless SuperAdmin)
            if (entry.ResourceId != resourceId && !IsSuperAdmin)
                return Forbid();

            model.Id = entry.Id;
            model.EnhancementId = entry.EnhancementId;
            model.ServiceAreaId = entry.Enhancement?.ServiceAreaId;
            model.WorkPhaseId = entry.WorkPhaseId;
            model.StartDate = entry.StartDate;
            model.EndDate = entry.EndDate;
            model.Hours = entry.Hours;
            model.ContributedHours = entry.ContributedHours;
            model.Notes = entry.Notes;
            model.EnhancementWorkId = entry.Enhancement?.WorkId;
            model.EnhancementDescription = entry.Enhancement?.Description;
            model.WorkPhaseName = entry.WorkPhase?.Name;
            model.IsConsolidated = entry.ConsolidationSources?.Any() == true;
        }
        else if (!string.IsNullOrEmpty(enhancementId))
        {
            // Pre-select enhancement if provided
            var canLog = await _timesheetService.CanLogTimeForEnhancementAsync(resourceId, enhancementId);
            if (!canLog && !IsSuperAdmin)
                return BadRequest("You do not have permission to log time for this enhancement.");
            
            model.EnhancementId = enhancementId;
        }

        return PartialView("_EditTimeEntry", model);
    }

    /// <summary>
    /// Save a time entry
    /// </summary>
    [HttpPost("entry/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEntry(TimeEntryEditViewModel model)
    {
        var resourceId = CurrentUserId!;

        // Check permission for the enhancement
        var canLog = await _timesheetService.CanLogTimeForEnhancementAsync(resourceId, model.EnhancementId);
        if (!canLog && !IsSuperAdmin)
        {
            return Json(new { success = false, message = "You do not have permission to log time for this enhancement." });
        }

        // Default contributed hours if not provided
        if (model.ContributedHours == 0 && model.Hours > 0)
        {
            var workPhase = await _workPhaseService.GetByIdAsync(model.WorkPhaseId);
            model.ContributedHours = model.Hours * (workPhase?.DefaultContributionPercent ?? 100) / 100;
        }

        // Validate
        var validation = await _timesheetService.ValidateEntryAsync(
            model.EnhancementId,
            model.StartDate,
            model.EndDate,
            model.Hours,
            model.ContributedHours);

        if (!validation.isValid)
        {
            return Json(new { success = false, message = validation.error });
        }

        try
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                // Create new entry
                await _timesheetService.CreateEntryAsync(
                    model.EnhancementId,
                    resourceId,
                    model.WorkPhaseId,
                    model.StartDate,
                    model.EndDate,
                    model.Hours,
                    model.ContributedHours,
                    model.Notes,
                    resourceId);

                _logger.LogInformation("Time entry created by {Resource} for enhancement {Enhancement}", 
                    CurrentUserEmail, model.EnhancementId);
            }
            else
            {
                // Update existing entry
                var entry = await _timesheetService.GetEntryByIdAsync(model.Id);
                if (entry == null)
                    return Json(new { success = false, message = "Entry not found." });

                // Can only edit own entries (unless SuperAdmin)
                if (entry.ResourceId != resourceId && !IsSuperAdmin)
                    return Json(new { success = false, message = "Cannot edit another user's entry." });

                await _timesheetService.UpdateEntryAsync(
                    model.Id,
                    model.WorkPhaseId,
                    model.StartDate,
                    model.EndDate,
                    model.Hours,
                    model.ContributedHours,
                    model.Notes,
                    resourceId);

                _logger.LogInformation("Time entry {Id} updated by {Resource}", model.Id, CurrentUserEmail);
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving time entry");
            return Json(new { success = false, message = "An error occurred saving the entry." });
        }
    }

    /// <summary>
    /// Delete a time entry
    /// </summary>
    [HttpPost("entry/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEntry(string id)
    {
        var resourceId = CurrentUserId!;
        
        var entry = await _timesheetService.GetEntryByIdAsync(id);
        if (entry == null)
            return Json(new { success = false, message = "Entry not found." });

        // Can only delete own entries (unless SuperAdmin)
        if (entry.ResourceId != resourceId && !IsSuperAdmin)
            return Json(new { success = false, message = "Cannot delete another user's entry." });

        var result = await _timesheetService.DeleteEntryAsync(id);
        
        if (!result.success)
            return Json(new { success = false, message = result.error });

        _logger.LogInformation("Time entry {Id} deleted by {Resource}", id, CurrentUserEmail);
        return Json(new { success = true });
    }

    /// <summary>
    /// Get work phase contribution percent (for AJAX)
    /// </summary>
    [HttpGet("workphase/{id}/contribution")]
    public async Task<IActionResult> GetWorkPhaseContribution(string id)
    {
        var workPhase = await _workPhaseService.GetByIdAsync(id);
        if (workPhase == null)
            return NotFound();

        return Json(new { contributionPercent = workPhase.DefaultContributionPercent });
    }

    /// <summary>
    /// Admin view of all timesheet entries
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> AdminView(
        string? serviceAreaId = null,
        string? resourceId = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var entries = await _timesheetService.GetEntriesAsync(
            serviceAreaId: serviceAreaId,
            resourceId: resourceId,
            startDate: startDate,
            endDate: endDate);

        var model = new TimesheetEntriesViewModel
        {
            ServiceAreaId = serviceAreaId,
            ResourceId = resourceId,
            StartDate = startDate,
            EndDate = endDate,
            Entries = entries,
            TotalHours = entries.Sum(e => e.Hours),
            TotalContributedHours = entries.Sum(e => e.ContributedHours),
            AvailableServiceAreas = await _serviceAreaService.GetAllAsync()
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "timesheet-admin");
        return View("AdminTimesheet", model);
    }
}
