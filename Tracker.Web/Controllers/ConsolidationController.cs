using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
[Route("consolidation")]
public class ConsolidationController : BaseController
{
    private readonly IConsolidationService _consolidationService;
    private readonly ITimesheetService _timesheetService;
    private readonly IEnhancementService _enhancementService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<ConsolidationController> _logger;

    public ConsolidationController(
        IAuthService authService,
        IConsolidationService consolidationService,
        ITimesheetService timesheetService,
        IEnhancementService enhancementService,
        IServiceAreaService serviceAreaService,
        ILogger<ConsolidationController> logger) : base(authService)
    {
        _consolidationService = consolidationService;
        _timesheetService = timesheetService;
        _enhancementService = enhancementService;
        _serviceAreaService = serviceAreaService;
        _logger = logger;
    }

    /// <summary>
    /// Get accessible service areas for current user
    /// </summary>
    private async Task<List<ServiceArea>> GetAccessibleServiceAreasAsync()
    {
        return await AuthService.GetUserServiceAreasAsync(CurrentUserId!);
    }

    /// <summary>
    /// Consolidation list page
    /// </summary>
    [HttpGet("")]
    public async Task<IActionResult> Index(
        string? serviceAreaId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        ConsolidationStatus? status = null)
    {
        // Check access
        if (!IsSuperAdmin && string.IsNullOrEmpty(serviceAreaId))
        {
            // Default to first accessible service area
            var accessibleAreas = await GetAccessibleServiceAreasAsync();
            if (accessibleAreas.Any())
                serviceAreaId = accessibleAreas.First().Id;
        }

        var consolidations = await _consolidationService.GetConsolidationsAsync(
            serviceAreaId: serviceAreaId,
            startDate: startDate,
            endDate: endDate,
            status: status);

        var model = new ConsolidationListViewModel
        {
            ServiceAreaId = serviceAreaId,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            Consolidations = consolidations,
            TotalBillableHours = consolidations.Sum(c => c.BillableHours),
            DraftCount = consolidations.Count(c => c.Status == ConsolidationStatus.Draft),
            FinalizedCount = consolidations.Count(c => c.Status == ConsolidationStatus.Finalized),
            AvailableServiceAreas = IsSuperAdmin 
                ? await _serviceAreaService.GetAllAsync()
                : await GetAccessibleServiceAreasAsync()
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "consolidation");
        return View("Index", model);
    }

    /// <summary>
    /// New consolidation page with two tabs
    /// </summary>
    [HttpGet("new")]
    public async Task<IActionResult> New(string? serviceAreaId = null)
    {
        var serviceAreas = IsSuperAdmin 
            ? await _serviceAreaService.GetAllAsync()
            : await GetAccessibleServiceAreasAsync();

        // Default dates to current month
        var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var endDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 
            DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

        var model = new ConsolidationFromTimesheetsViewModel
        {
            ServiceAreaId = serviceAreaId,
            StartDate = startDate,
            EndDate = endDate,
            AvailableServiceAreas = serviceAreas
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "consolidation");
        return View("New", model);
    }

    /// <summary>
    /// Load enhancements with time entries for date range (AJAX)
    /// </summary>
    [HttpGet("enhancements")]
    public async Task<IActionResult> GetEnhancementsWithEntries(
        string? serviceAreaId,
        DateTime startDate,
        DateTime endDate)
    {
        // Validate date range
        var dateValidation = _consolidationService.ValidateDateRange(startDate, endDate);
        if (!dateValidation.isValid)
            return BadRequest(new { message = dateValidation.error });

        var enhancements = await _consolidationService.GetEnhancementsWithEntriesAsync(
            serviceAreaId, startDate, endDate);

        return Json(enhancements);
    }

    /// <summary>
    /// Load time entries for an enhancement (AJAX)
    /// </summary>
    [HttpGet("entries")]
    public async Task<IActionResult> GetTimeEntries(
        string enhancementId,
        DateTime startDate,
        DateTime endDate)
    {
        var entries = await _consolidationService.GetAvailableEntriesAsync(
            enhancementId, startDate, endDate);

        var viewModels = entries.Select(e => new TimeEntryForConsolidationViewModel
        {
            Id = e.Id,
            ResourceId = e.ResourceId,
            ResourceName = e.ResourceName,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            WorkPhaseName = e.WorkPhaseName,
            Hours = e.Hours,
            ContributedHours = e.ContributedHours,
            AlreadyPulledHours = e.AlreadyPulledHours,
            AvailableHours = e.AvailableHours,
            PullHours = e.AvailableHours, // Default to available
            Notes = e.Notes,
            IsSelected = e.AvailableHours > 0
        }).ToList();

        return Json(viewModels);
    }

    /// <summary>
    /// Create consolidation from timesheets
    /// </summary>
    [HttpPost("create-from-timesheets")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFromTimesheets([FromBody] CreateConsolidationFromTimesheetsRequest request)
    {
        // Validate date range
        var dateValidation = _consolidationService.ValidateDateRange(request.StartDate, request.EndDate);
        if (!dateValidation.isValid)
            return BadRequest(new { success = false, message = dateValidation.error });

        // Validate request
        if (string.IsNullOrEmpty(request.EnhancementId))
            return BadRequest(new { success = false, message = "Enhancement is required." });

        if (request.BillableHours <= 0)
            return BadRequest(new { success = false, message = "Billable hours must be greater than 0." });

        if (request.Sources == null || !request.Sources.Any())
            return BadRequest(new { success = false, message = "At least one time entry must be selected." });

        try
        {
            var sources = request.Sources
                .Where(s => s.PulledHours > 0)
                .Select(s => new ConsolidationSourceInput
                {
                    TimeEntryId = s.TimeEntryId,
                    PulledHours = s.PulledHours
                }).ToList();

            var consolidation = await _consolidationService.CreateFromTimesheetsAsync(
                request.EnhancementId,
                request.StartDate,
                request.EndDate,
                sources,
                request.BillableHours,
                request.Notes,
                CurrentUserId!);

            _logger.LogInformation("Consolidation {Id} created from timesheets by {User}", 
                consolidation.Id, CurrentUserEmail);

            return Json(new { success = true, id = consolidation.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating consolidation from timesheets");
            return Json(new { success = false, message = "An error occurred creating the consolidation." });
        }
    }

    /// <summary>
    /// Create manual consolidation
    /// </summary>
    [HttpPost("create-manual")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateManual([FromBody] CreateManualConsolidationRequest request)
    {
        // Validate date range
        var dateValidation = _consolidationService.ValidateDateRange(request.StartDate, request.EndDate);
        if (!dateValidation.isValid)
            return BadRequest(new { success = false, message = dateValidation.error });

        // Validate request
        if (string.IsNullOrEmpty(request.EnhancementId))
            return BadRequest(new { success = false, message = "Enhancement is required." });

        if (request.BillableHours <= 0)
            return BadRequest(new { success = false, message = "Billable hours must be greater than 0." });

        if (string.IsNullOrWhiteSpace(request.Notes))
            return BadRequest(new { success = false, message = "Notes are required for manual consolidation." });

        try
        {
            var consolidation = await _consolidationService.CreateManualAsync(
                request.EnhancementId,
                request.StartDate,
                request.EndDate,
                request.BillableHours,
                request.Notes,
                CurrentUserId!);

            _logger.LogInformation("Manual consolidation {Id} created by {User}", 
                consolidation.Id, CurrentUserEmail);

            return Json(new { success = true, id = consolidation.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating manual consolidation");
            return Json(new { success = false, message = "An error occurred creating the consolidation." });
        }
    }

    /// <summary>
    /// View consolidation details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id)
    {
        var consolidation = await _consolidationService.GetByIdAsync(id);
        if (consolidation == null)
            return NotFound();

        var model = new ConsolidationDetailViewModel
        {
            Id = consolidation.Id,
            EnhancementId = consolidation.EnhancementId,
            EnhancementWorkId = consolidation.Enhancement?.WorkId ?? "",
            EnhancementDescription = consolidation.Enhancement?.Description ?? "",
            ServiceAreaId = consolidation.ServiceAreaId,
            ServiceAreaName = consolidation.ServiceArea?.Name ?? "",
            StartDate = consolidation.StartDate,
            EndDate = consolidation.EndDate,
            BillableHours = consolidation.BillableHours,
            SourceHours = consolidation.SourceHours,
            Status = consolidation.Status,
            Notes = consolidation.Notes,
            IsManual = consolidation.IsManual,
            CreatedByName = consolidation.CreatedBy?.DisplayName,
            CreatedAt = consolidation.CreatedAt,
            ModifiedByName = consolidation.ModifiedBy?.DisplayName,
            ModifiedAt = consolidation.ModifiedAt,
            Sources = consolidation.Sources.Select(s => new ConsolidationSourceViewModel
            {
                TimeEntryId = s.TimeEntryId,
                ResourceName = s.TimeEntry?.Resource?.Name ?? "Unknown",
                StartDate = s.TimeEntry?.StartDate ?? DateTime.MinValue,
                EndDate = s.TimeEntry?.EndDate ?? DateTime.MinValue,
                WorkPhaseName = s.TimeEntry?.WorkPhase?.Name ?? "Unknown",
                OriginalHours = s.TimeEntry?.Hours ?? 0,
                OriginalContributedHours = s.TimeEntry?.ContributedHours ?? 0,
                PulledHours = s.PulledHours
            }).ToList()
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "consolidation");
        return View("Details", model);
    }

    /// <summary>
    /// Finalize a consolidation
    /// </summary>
    [HttpPost("{id}/finalize")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finalize(string id)
    {
        var result = await _consolidationService.ChangeStatusAsync(
            id, ConsolidationStatus.Finalized, CurrentUserId!);

        if (!result.success)
            return Json(new { success = false, message = result.error });

        _logger.LogInformation("Consolidation {Id} finalized by {User}", id, CurrentUserEmail);
        return Json(new { success = true });
    }

    /// <summary>
    /// Revert a finalized consolidation to draft
    /// </summary>
    [HttpPost("{id}/revert")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Revert(string id)
    {
        var result = await _consolidationService.ChangeStatusAsync(
            id, ConsolidationStatus.Draft, CurrentUserId!);

        if (!result.success)
            return Json(new { success = false, message = result.error });

        _logger.LogInformation("Consolidation {Id} reverted to draft by {User}", id, CurrentUserEmail);
        return Json(new { success = true });
    }

    /// <summary>
    /// Delete a consolidation
    /// </summary>
    [HttpPost("{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _consolidationService.DeleteAsync(id);

        if (!result.success)
            return Json(new { success = false, message = result.error });

        _logger.LogInformation("Consolidation {Id} deleted by {User}", id, CurrentUserEmail);
        return Json(new { success = true });
    }

    /// <summary>
    /// Get all enhancements for a service area (AJAX)
    /// </summary>
    [HttpGet("enhancements/all")]
    public async Task<IActionResult> GetAllEnhancements(string? serviceAreaId)
    {
        List<Enhancement> enhancements;
        
        if (string.IsNullOrEmpty(serviceAreaId))
        {
            // Get from all accessible service areas
            var serviceAreas = await GetAccessibleServiceAreasAsync();
            var allEnhancements = new List<Enhancement>();
            foreach (var sa in serviceAreas)
            {
                var saEnhancements = await _enhancementService.GetByServiceAreaAsync(sa.Id);
                allEnhancements.AddRange(saEnhancements);
            }
            enhancements = allEnhancements;
        }
        else
        {
            enhancements = await _enhancementService.GetByServiceAreaAsync(serviceAreaId);
        }

        return Json(enhancements.Select(e => new { e.Id, e.WorkId, e.Description }));
    }
}

// Request DTOs
public class CreateConsolidationFromTimesheetsRequest
{
    public string EnhancementId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BillableHours { get; set; }
    public string? Notes { get; set; }
    public List<SourceInput> Sources { get; set; } = new();
}

public class SourceInput
{
    public string TimeEntryId { get; set; } = string.Empty;
    public decimal PulledHours { get; set; }
}

public class CreateManualConsolidationRequest
{
    public string EnhancementId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal BillableHours { get; set; }
    public string Notes { get; set; } = string.Empty;
}
