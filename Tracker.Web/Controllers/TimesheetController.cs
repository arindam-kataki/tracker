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
    private readonly IEnhancementService _enhancementService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<TimesheetController> _logger;

    public TimesheetController(
        IAuthService authService,
        ITimesheetService timesheetService,
        IWorkPhaseService workPhaseService,
        IEnhancementService enhancementService,
        IServiceAreaService serviceAreaService,
        ILogger<TimesheetController> logger) : base(authService)
    {
        _timesheetService = timesheetService;
        _workPhaseService = workPhaseService;
        _enhancementService = enhancementService;
        _serviceAreaService = serviceAreaService;
        _logger = logger;
    }

    /// <summary>
    /// My Timesheet - for the logged-in resource to view and enter time
    /// </summary>
    [HttpGet("")]
    [HttpGet("my")]
    public async Task<IActionResult> MyTimesheet(DateTime? startDate = null, DateTime? endDate = null)
    {
        // Get resource for current user
        var resource = await _timesheetService.GetResourceForUserAsync(CurrentUserId!);
        
        if (resource == null)
        {
            // User is not a resource - show message
            ViewBag.Message = "Your account is not linked to a resource. Please contact an administrator.";
            ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "timesheet");
            return View("NoResource");
        }

        // Default to current week
        var start = startDate ?? DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        var end = endDate ?? start.AddDays(6);

        var entries = await _timesheetService.GetEntriesForResourceAsync(resource.Id, start, end);
        var assignedEnhancements = await _timesheetService.GetAssignedEnhancementsAsync(resource.Id);
        var workPhases = await _workPhaseService.GetForTimeRecordingAsync();

        var model = new MyTimesheetViewModel
        {
            ResourceId = resource.Id,
            ResourceName = resource.Name,
            StartDate = start,
            EndDate = end,
            Entries = entries,
            AssignedEnhancements = assignedEnhancements,
            WorkPhases = workPhases,
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
    /// Get the add/edit time entry modal
    /// </summary>
    [HttpGet("entry/edit")]
    public async Task<IActionResult> EditEntry(string? id = null, string? enhancementId = null)
    {
        var resource = await _timesheetService.GetResourceForUserAsync(CurrentUserId!);
        if (resource == null)
            return BadRequest("User is not linked to a resource.");

        var model = new TimeEntryEditViewModel
        {
            ResourceId = resource.Id,
            AvailableEnhancements = await _timesheetService.GetAssignedEnhancementsAsync(resource.Id),
            AvailableWorkPhases = await _workPhaseService.GetForTimeRecordingAsync()
        };

        if (!string.IsNullOrEmpty(id))
        {
            var entry = await _timesheetService.GetEntryByIdAsync(id);
            if (entry == null)
                return NotFound();

            // Can only edit own entries
            if (entry.ResourceId != resource.Id && !IsSuperAdmin)
                return Forbid();

            model.Id = entry.Id;
            model.EnhancementId = entry.EnhancementId;
            model.WorkPhaseId = entry.WorkPhaseId;
            model.StartDate = entry.StartDate;
            model.EndDate = entry.EndDate;
            model.Hours = entry.Hours;
            model.ContributedHours = entry.ContributedHours;
            model.Notes = entry.Notes;
            model.EnhancementWorkId = entry.Enhancement?.WorkId;
            model.WorkPhaseName = entry.WorkPhase?.Name;
            model.IsConsolidated = entry.ConsolidationSources?.Any() == true;
        }
        else if (!string.IsNullOrEmpty(enhancementId))
        {
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
        var resource = await _timesheetService.GetResourceForUserAsync(CurrentUserId!);
        if (resource == null)
            return BadRequest(new { success = false, message = "User is not linked to a resource." });

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
                    resource.Id,
                    model.WorkPhaseId,
                    model.StartDate,
                    model.EndDate,
                    model.Hours,
                    model.ContributedHours,
                    model.Notes,
                    CurrentUserId!);

                _logger.LogInformation("Time entry created by {User} for enhancement {Enhancement}", 
                    CurrentUserEmail, model.EnhancementId);
            }
            else
            {
                // Update existing entry
                var entry = await _timesheetService.GetEntryByIdAsync(model.Id);
                if (entry == null)
                    return Json(new { success = false, message = "Entry not found." });

                // Can only edit own entries
                if (entry.ResourceId != resource.Id && !IsSuperAdmin)
                    return Json(new { success = false, message = "Cannot edit another user's entry." });

                await _timesheetService.UpdateEntryAsync(
                    model.Id,
                    model.WorkPhaseId,
                    model.StartDate,
                    model.EndDate,
                    model.Hours,
                    model.ContributedHours,
                    model.Notes,
                    CurrentUserId!);

                _logger.LogInformation("Time entry {Id} updated by {User}", model.Id, CurrentUserEmail);
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
        var resource = await _timesheetService.GetResourceForUserAsync(CurrentUserId!);
        
        var entry = await _timesheetService.GetEntryByIdAsync(id);
        if (entry == null)
            return Json(new { success = false, message = "Entry not found." });

        // Can only delete own entries (unless admin)
        if (resource != null && entry.ResourceId != resource.Id && !IsSuperAdmin)
            return Json(new { success = false, message = "Cannot delete another user's entry." });

        var result = await _timesheetService.DeleteEntryAsync(id);
        
        if (!result.success)
            return Json(new { success = false, message = result.error });

        _logger.LogInformation("Time entry {Id} deleted by {User}", id, CurrentUserEmail);
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
