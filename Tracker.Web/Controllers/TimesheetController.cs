using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Entities;
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
    /// My Timesheet - List view (original)
    /// </summary>
    [HttpGet("")]
    [HttpGet("my")]
    public async Task<IActionResult> MyTimesheet(DateOnly? startDate = null, DateOnly? endDate = null)
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

        // Default to current week
        var today = DateOnly.FromDateTime(DateTime.Today);
        var start = startDate ?? today.AddDays(-(int)today.DayOfWeek);
        var end = endDate ?? start.AddDays(6);

        // Get entries for this resource
        var entries = await _timesheetService.GetEntriesForResourceAsync(resourceId, start, end);

        // Get available work phases
        var workPhases = await _workPhaseService.GetForTimeRecordingAsync();

        var model = new MyTimesheetViewModel
        {
            ResourceId = resourceId,
            ResourceName = CurrentUserName ?? "Unknown",
            StartDate = start.ToDateTime(TimeOnly.MinValue),
            EndDate = end.ToDateTime(TimeOnly.MinValue),
            Entries = entries,
            WorkPhases = workPhases,
            AvailableServiceAreas = permittedServiceAreas,
            TotalHours = entries.Sum(e => e.Hours),
            TotalContributedHours = entries.Sum(e => e.ContributedHours),
            EntriesByDate = entries
                .GroupBy(e => e.StartDate.ToString("yyyy-MM-dd"))
                .ToDictionary(g => g.Key, g => g.ToList()),
            HoursByPhase = entries
                .GroupBy(e => e.WorkPhase?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours))
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "timesheet");
        return View("MyTimesheet", model);
    }

    /// <summary>
    /// My Timesheet - Calendar view
    /// </summary>
    [HttpGet("calendar")]
    public async Task<IActionResult> MyTimesheetCalendar(int? year = null, int? month = null)
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

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "timesheet-calendar");
        return View("MyTimesheetCalendar", model);
    }

    /// <summary>
    /// Get calendar data (hours per day) with optional filtering.
    /// Uses DateOnly internally for timezone safety.
    /// </summary>
    [HttpGet("calendar-data")]
    public async Task<IActionResult> GetCalendarData(
        string? enhancementId = null,
        string? serviceAreaId = null,
        int? year = null,
        int? month = null)
    {
        var resourceId = CurrentUserId!;
        var targetYear = year ?? DateTime.Today.Year;
        var targetMonth = month ?? DateTime.Today.Month;

        var startDate = new DateOnly(targetYear, targetMonth, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Get entries based on filter level
        List<TimeEntry> entries;

        if (!string.IsNullOrEmpty(enhancementId))
        {
            entries = await _timesheetService.GetEntriesAsync(
                enhancementId: enhancementId,
                resourceId: resourceId,
                startDate: startDate,
                endDate: endDate);
        }
        else if (!string.IsNullOrEmpty(serviceAreaId))
        {
            entries = await _timesheetService.GetEntriesAsync(
                serviceAreaId: serviceAreaId,
                resourceId: resourceId,
                startDate: startDate,
                endDate: endDate);
        }
        else
        {
            entries = await _timesheetService.GetEntriesForResourceAsync(resourceId, startDate, endDate);
        }

        // Group by date - DateOnly.ToString("yyyy-MM-dd") is timezone-safe
        var result = entries
            .GroupBy(e => e.StartDate.ToString("yyyy-MM-dd"))
            .Select(g => new
            {
                date = g.Key,
                hours = g.Sum(e => e.Hours),
                contributed = g.Sum(e => e.ContributedHours)
            })
            .ToList();

        return Json(result);
    }

    /// <summary>
    /// Get monthly totals with optional filtering
    /// </summary>
    [HttpGet("monthly-totals")]
    public async Task<IActionResult> GetMonthlyTotals(
        string? enhancementId = null,
        string? serviceAreaId = null,
        int? year = null,
        int? month = null)
    {
        var resourceId = CurrentUserId!;
        var targetYear = year ?? DateTime.Today.Year;
        var targetMonth = month ?? DateTime.Today.Month;

        var startDate = new DateOnly(targetYear, targetMonth, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        List<TimeEntry> entries;

        if (!string.IsNullOrEmpty(enhancementId))
        {
            entries = await _timesheetService.GetEntriesAsync(
                enhancementId: enhancementId,
                resourceId: resourceId,
                startDate: startDate,
                endDate: endDate);
        }
        else if (!string.IsNullOrEmpty(serviceAreaId))
        {
            entries = await _timesheetService.GetEntriesAsync(
                serviceAreaId: serviceAreaId,
                resourceId: resourceId,
                startDate: startDate,
                endDate: endDate);
        }
        else
        {
            entries = await _timesheetService.GetEntriesForResourceAsync(resourceId, startDate, endDate);
        }

        var result = new
        {
            totalHours = entries.Sum(e => e.Hours),
            totalContributed = entries.Sum(e => e.ContributedHours),
            daysWorked = entries.Select(e => e.StartDate).Distinct().Count(),
            byWorkPhase = entries
                .GroupBy(e => e.WorkPhase?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours))
        };

        return Json(result);
    }

    /// <summary>
    /// Get monthly summary with work items breakdown
    /// </summary>
    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary(
        string? enhancementId = null,
        string? serviceAreaId = null,
        int? year = null,
        int? month = null)
    {
        var resourceId = CurrentUserId!;
        var targetYear = year ?? DateTime.Today.Year;
        var targetMonth = month ?? DateTime.Today.Month;

        var startDate = new DateOnly(targetYear, targetMonth, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        List<TimeEntry> entries;

        if (!string.IsNullOrEmpty(enhancementId))
        {
            entries = await _timesheetService.GetEntriesAsync(
                enhancementId: enhancementId,
                resourceId: resourceId,
                startDate: startDate,
                endDate: endDate);

            var totalHours = entries.Sum(e => e.Hours);
            var totalContributed = entries.Sum(e => e.ContributedHours);
            var byWorkPhase = entries
                .GroupBy(e => e.WorkPhase?.Name ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours));

            return Json(new
            {
                totalHours,
                totalContributed,
                items = new[] { new {
                    enhancementId,
                    workId = entries.FirstOrDefault()?.Enhancement?.WorkId ?? "Unknown",
                    description = entries.FirstOrDefault()?.Enhancement?.Description,
                    serviceAreaId = entries.FirstOrDefault()?.Enhancement?.ServiceAreaId,
                    serviceAreaCode = entries.FirstOrDefault()?.Enhancement?.ServiceArea?.Code,
                    serviceAreaName = entries.FirstOrDefault()?.Enhancement?.ServiceArea?.Name,
                    hours = totalHours,
                    contributedHours = totalContributed,
                    byWorkPhase
                }}
            });
        }
        else if (!string.IsNullOrEmpty(serviceAreaId))
        {
            entries = await _timesheetService.GetEntriesAsync(
                serviceAreaId: serviceAreaId,
                resourceId: resourceId,
                startDate: startDate,
                endDate: endDate);
        }
        else
        {
            entries = await _timesheetService.GetEntriesForResourceAsync(resourceId, startDate, endDate);
        }

        // Group by enhancement
        var groupedByEnhancement = entries
            .GroupBy(e => e.EnhancementId)
            .Select(g => new
            {
                enhancementId = g.Key,
                workId = g.First().Enhancement?.WorkId ?? "Unknown",
                description = g.First().Enhancement?.Description,
                serviceAreaId = g.First().Enhancement?.ServiceAreaId,
                serviceAreaCode = g.First().Enhancement?.ServiceArea?.Code,
                serviceAreaName = g.First().Enhancement?.ServiceArea?.Name,
                hours = g.Sum(e => e.Hours),
                contributedHours = g.Sum(e => e.ContributedHours),
                byWorkPhase = g
                    .GroupBy(e => e.WorkPhase?.Name ?? "Unknown")
                    .ToDictionary(wp => wp.Key, wp => wp.Sum(e => e.Hours))
            })
            .OrderBy(e => e.serviceAreaCode)
            .ThenBy(e => e.workId)
            .ToList();

        return Json(new
        {
            totalHours = entries.Sum(e => e.Hours),
            totalContributed = entries.Sum(e => e.ContributedHours),
            items = groupedByEnhancement
        });
    }

    /// <summary>
    /// Get enhancements for dropdown - filtered by service area
    /// </summary>
    [HttpGet("enhancements")]
    public async Task<IActionResult> GetEnhancements(string? serviceAreaId = null, string? search = null)
    {
        var resourceId = CurrentUserId!;

        var enhancements = await _timesheetService.GetEnhancementsForTimesheetAsync(
            resourceId,
            serviceAreaId,
            search);

        var result = enhancements.Select(e => new
        {
            id = e.Id,
            workId = e.WorkId,
            description = e.Description,
            status = e.Status,
            serviceAreaCode = e.ServiceArea?.Code
        }).ToList();

        return Json(result);
    }

    /// <summary>
    /// Get entries for a specific day and enhancement.
    /// Date parameter is parsed as DateOnly for timezone safety.
    /// </summary>
    [HttpGet("day-entries")]
    public async Task<IActionResult> GetDayEntries(string date, string enhancementId)
    {
        var resourceId = CurrentUserId!;

        // Parse as DateOnly - this is timezone-safe
        if (!DateOnly.TryParse(date, out var entryDate))
        {
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        }

        var entries = await _timesheetService.GetEntriesAsync(
            enhancementId: enhancementId,
            resourceId: resourceId,
            startDate: entryDate,
            endDate: entryDate);

        var result = entries.Select(e => new
        {
            id = e.Id,
            workPhaseId = e.WorkPhaseId,
            workPhaseName = e.WorkPhase?.Name ?? "Unknown",
            hours = e.Hours,
            contributedHours = e.ContributedHours,
            notes = e.Notes,
            isConsolidated = e.ConsolidationSources?.Any() == true
        }).ToList();

        return Json(result);
    }

    /// <summary>
    /// Save (create or update) a time entry.
    /// Date is parsed as DateOnly for timezone safety.
    /// </summary>
    [HttpPost("entry/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveEntry(
        string? id,
        string? enhancementId,
        string workPhaseId,
        string startDate,  // Accept as string for explicit parsing
        decimal hours,
        decimal contributedHours,
        string? notes)
    {
        var resourceId = CurrentUserId!;

        // Parse date as DateOnly - timezone safe
        if (!DateOnly.TryParse(startDate, out var entryDate))
        {
            return Json(new { success = false, message = "Invalid date format. Use YYYY-MM-DD." });
        }

        try
        {
            if (string.IsNullOrEmpty(id))
            {
                // Create new entry
                if (string.IsNullOrEmpty(enhancementId))
                {
                    return Json(new { success = false, message = "Enhancement is required." });
                }

                // Verify permission
                var canLog = await _timesheetService.CanLogTimeForEnhancementAsync(resourceId, enhancementId);
                if (!canLog && !IsSuperAdmin)
                {
                    return Json(new { success = false, message = "You don't have permission to log time for this enhancement." });
                }

                // Validate
                var validation = await _timesheetService.ValidateEntryAsync(
                    enhancementId, entryDate, entryDate, hours, contributedHours);
                if (!validation.isValid)
                {
                    return Json(new { success = false, message = validation.error });
                }

                var entry = await _timesheetService.CreateEntryAsync(
                    enhancementId,
                    resourceId,
                    workPhaseId,
                    entryDate,
                    entryDate,  // Single day entry
                    hours,
                    contributedHours,
                    notes,
                    resourceId);

                _logger.LogInformation("Time entry {Id} created by {User} for {Date}",
                    entry.Id, CurrentUserEmail, entryDate);

                return Json(new { success = true, id = entry.Id });
            }
            else
            {
                // Update existing entry
                var entry = await _timesheetService.UpdateEntryAsync(
                    id,
                    workPhaseId,
                    entryDate,
                    entryDate,
                    hours,
                    contributedHours,
                    notes,
                    resourceId);

                if (entry == null)
                {
                    return Json(new { success = false, message = "Entry not found." });
                }

                _logger.LogInformation("Time entry {Id} updated by {User}", id, CurrentUserEmail);

                return Json(new { success = true, id = entry.Id });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving time entry");
            return Json(new { success = false, message = "An error occurred while saving." });
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
        DateOnly? startDate = null,
        DateOnly? endDate = null)
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
            StartDate = startDate?.ToDateTime(TimeOnly.MinValue),
            EndDate = endDate?.ToDateTime(TimeOnly.MinValue),
            Entries = entries,
            TotalHours = entries.Sum(e => e.Hours),
            TotalContributedHours = entries.Sum(e => e.ContributedHours),
            AvailableServiceAreas = await _serviceAreaService.GetAllAsync()
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "timesheet-admin");
        return View("AdminTimesheet", model);
    }
}
