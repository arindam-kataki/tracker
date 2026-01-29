using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Tracker.Web.Controllers;

/// <summary>
/// Controller for Enhancement Details page (tabbed interface).
/// Handles ticket details, attachments, notes, sharing, and time recording.
/// </summary>
[Authorize]
[Route("Enhancements")]
public class EnhancementDetailsController : BaseController
{
    private readonly IEnhancementService _enhancementService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly IResourceService _resourceService;
    private readonly ISkillService _skillService;
    private readonly IEnhancementNoteService _noteService;
    private readonly IAttachmentService _attachmentService;
    private readonly ITimeRecordingService _timeRecordingService;
    private readonly IEnhancementSharingService _sharingService;
    //private readonly IUserService _userService;
    private readonly TrackerDbContext _context;

    public EnhancementDetailsController(
        IAuthService authService,
        IEnhancementService enhancementService,
        IServiceAreaService serviceAreaService,
        IResourceService resourceService,
        ISkillService skillService,
        IEnhancementNoteService noteService,
        IAttachmentService attachmentService,
        ITimeRecordingService timeRecordingService,
        IEnhancementSharingService sharingService,
        // IUserService userService,
        TrackerDbContext context) : base(authService)
    {
        _enhancementService = enhancementService;
        _serviceAreaService = serviceAreaService;
        _resourceService = resourceService;
        _skillService = skillService;
        _noteService = noteService;
        _attachmentService = attachmentService;
        _timeRecordingService = timeRecordingService;
        _sharingService = sharingService;
        // _userService = userService;
        _context = context;
    }

    #region Main Details Page

    [HttpGet("Details/{id?}")]
    public async Task<IActionResult> Details(string? id, string serviceAreaId, string tab = "details")
    {
        if (string.IsNullOrEmpty(serviceAreaId))
            return RedirectToAction("Index", "Home");

        // Check access
        if (!IsSuperAdmin)
        {
            var hasAccess = await AuthService.HasAccessToServiceAreaAsync(CurrentUserId!, serviceAreaId);
            if (!hasAccess)
                return RedirectToAction("AccessDenied", "Account");
        }

        var serviceArea = await _serviceAreaService.GetByIdAsync(serviceAreaId);
        if (serviceArea == null)
            return NotFound("Service area not found");

        Enhancement? enhancement = null;
        if (!string.IsNullOrEmpty(id))
        {
            enhancement = await _enhancementService.GetByIdAsync(id);
            if (enhancement == null)
                return NotFound("Enhancement not found");
        }

        var model = await BuildDetailsViewModelAsync(enhancement, serviceArea, tab);

        ViewBag.Sidebar = await GetSidebarViewModelAsync(serviceAreaId);
        return View("Details", model);
    }

    [HttpGet("New")]
    public async Task<IActionResult> New(string serviceAreaId)
    {
        return await Details(null, serviceAreaId);
    }

    private async Task<EnhancementDetailsViewModel> BuildDetailsViewModelAsync(
        Enhancement? enhancement,
        ServiceArea serviceArea,
        string activeTab)
    {
        var isNew = enhancement == null;
        var enhancementId = enhancement?.Id ?? string.Empty;

        // Get dropdown data
        var serviceAreas = await _serviceAreaService.GetAllAsync();
        var sponsors = await _resourceService.GetByResourceTypeNameAsync("Client");
        var spocs = await _resourceService.GetByResourceTypeNameAsync("SPOC");
        var resources = await _resourceService.GetByResourceTypeNameAsync("Internal");
        var skills = await _skillService.GetActiveByServiceAreaAsync(serviceArea.Id);
        var timeCategories = await _timeRecordingService.GetAllCategoriesAsync();

        // Get distinct values for dropdowns
        //var allEnhancements = await _enhancementService.GetByServiceAreaAsync(serviceArea.Id, new EnhancementFilterViewModel { PageSize = 10000 });

        //var allEnhancements = await _enhancementService.GetByServiceAreaAsync(serviceArea.Id);

        //var serviceLines = allEnhancements.Items.Select(e => e.ServiceLine).Where(s => !string.IsNullOrEmpty(s)).Distinct().OrderBy(s => s).ToList();
        //var infStatuses = allEnhancements.Items.Select(e => e.InfStatus).Where(s => !string.IsNullOrEmpty(s)).Distinct().OrderBy(s => s).ToList();

        // Fixed - GetByServiceAreaAsync returns List<Enhancement>, not PagedResult
        var allEnhancements = await _enhancementService.GetByServiceAreaAsync(serviceArea.Id);

        var serviceLines = allEnhancements.Select(e => e.ServiceLine).Where(s => !string.IsNullOrEmpty(s)).Distinct().OrderBy(s => s).ToList();
        var infStatuses = allEnhancements.Select(e => e.InfStatus).Where(s => !string.IsNullOrEmpty(s)).Distinct().OrderBy(s => s).ToList();

        // Build ticket details
        var ticketDetails = new TicketDetailsViewModel
        {
            Id = enhancement?.Id,
            WorkId = enhancement?.WorkId ?? string.Empty,
            Description = enhancement?.Description ?? string.Empty,
            Notes = enhancement?.Notes,
            ServiceAreaId = serviceArea.Id,

            // Sizing - L3H fields
            EstimatedHours = enhancement?.EstimatedHours,
            EstimatedStartDate = enhancement?.EstimatedStartDate,
            EstimatedEndDate = enhancement?.EstimatedEndDate,
            EstimationNotes = enhancement?.EstimationNotes,
            Status = enhancement?.Status ?? "New",
            ServiceLine = enhancement?.ServiceLine,
            LaborType = enhancement?.LaborType,           // ADD THIS
            Priority = enhancement?.Priority,              // ADD THIS

            // Actual/Core Info - Inf* fields
            ReturnedHours = enhancement?.ReturnedHours,
            StartDate = enhancement?.StartDate,
            EndDate = enhancement?.EndDate,
            InfStatus = enhancement?.InfStatus,
            InfServiceLine = enhancement?.InfServiceLine,
            InfLaborType = enhancement?.InfLaborType,     // ADD THIS
            InfPriority = enhancement?.InfPriority,       // ADD THIS

            // Legacy time allocations
            TimeW1 = enhancement?.TimeW1,
            TimeW2 = enhancement?.TimeW2,
            TimeW3 = enhancement?.TimeW3,
            TimeW4 = enhancement?.TimeW4,
            TimeW5 = enhancement?.TimeW5,
            TimeW6 = enhancement?.TimeW6,
            TimeW7 = enhancement?.TimeW7,
            TimeW8 = enhancement?.TimeW8,
            TimeW9 = enhancement?.TimeW9,

            // Audit
            CreatedBy = enhancement?.CreatedBy,
            CreatedAt = enhancement?.CreatedAt,
            ModifiedBy = enhancement?.ModifiedBy,
            ModifiedAt = enhancement?.ModifiedAt,

            // Dropdown options
            AvailableServiceAreas = serviceAreas.ToList(),
            AvailableSponsors = sponsors.ToList(),
            AvailableSpocs = spocs.ToList(),
            AvailableResources = resources.ToList(),
            AvailableSkills = skills.ToList(),
            AvailableTimeCategories = timeCategories,
            AvailableServiceLines = serviceLines!,
            AvailableInfStatuses = infStatuses!,
            // AvailableLaborTypes, AvailablePriorities, AvailableSizingStatuses 
            // use default values from ViewModel - no need to set here

            // Selected IDs
            SelectedSponsorIds = enhancement?.Sponsors?.Select(s => s.ResourceId).ToList() ?? new(),
            SelectedSpocIds = enhancement?.Spocs?.Select(s => s.ResourceId).ToList() ?? new(),
            SelectedResourceIds = enhancement?.Resources?.Select(r => r.ResourceId).ToList() ?? new(),
            SelectedSkillIds = enhancement?.Skills?.Select(s => s.SkillId).ToList() ?? new(),
            SelectedTimeCategoryIds = !isNew ? (await _timeRecordingService.GetCategoriesForEnhancementAsync(enhancementId)).Select(c => c.Id).ToList() : new()
        };

        // Build model
        var model = new EnhancementDetailsViewModel
        {
            Id = enhancementId,
            ServiceAreaId = serviceArea.Id,
            ServiceAreaName = serviceArea.Name,
            TicketDetails = ticketDetails,
            ActiveTab = activeTab
        };

        // Load additional data for existing enhancements
        if (!isNew)
        {
            // Attachments
            var attachments = await _attachmentService.GetByEnhancementIdAsync(enhancementId);
            model.Attachments = attachments.Select(a => new AttachmentViewModel
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize,
                UploadedBy = a.UploadedBy,
                UploadedByName = a.UploadedByResource?.DisplayName ?? a.UploadedBy,
                UploadedAt = a.UploadedAt
            }).ToList();

            // Notes
            var notes = await _noteService.GetByEnhancementIdAsync(enhancementId);
            model.Notes = notes.Select(n => new NoteViewModel
            {
                Id = n.Id,
                NoteText = n.NoteText,
                CreatedBy = n.CreatedBy,
                CreatedByName = n.CreatedByResource?.DisplayName ?? n.CreatedBy,
                CreatedAt = n.CreatedAt,
                ModifiedBy = n.ModifiedBy,
                ModifiedAt = n.ModifiedAt
            }).ToList();

            // Sharing
            var accessibleServiceAreaIds = IsSuperAdmin
                ? (await _serviceAreaService.GetAllAsync()).Select(sa => sa.Id).ToList()
                : (await AuthService.GetUserServiceAreasAsync(CurrentUserId!)).Select(sa => sa.Id).ToList();

            var availableTargets = await _sharingService.GetAvailableTargetServiceAreasAsync(enhancementId, accessibleServiceAreaIds);
            var existingAreas = await GetExistingServiceAreasForWorkIdAsync(enhancement!.WorkId, serviceArea.Id);

            model.Sharing = new SharingViewModel
            {
                EnhancementId = enhancementId,
                WorkId = enhancement.WorkId,
                CurrentServiceAreaName = serviceArea.Name,
                AvailableTargetServiceAreas = availableTargets.Select(sa => new ServiceAreaOption { Id = sa.Id, Name = sa.Name }).ToList(),
                ExistingServiceAreaNames = existingAreas
            };

            // Time Recording
            var selectedCategories = await _timeRecordingService.GetCategoriesForEnhancementAsync(enhancementId);
            var timeEntries = await _timeRecordingService.GetEntriesForEnhancementAsync(enhancementId);
            var totalsByCategory = await _timeRecordingService.GetTotalHoursByCategoryAsync(enhancementId);

            model.TimeRecording = new TimeRecordingViewModel
            {
                EnhancementId = enhancementId,
                SelectedCategories = selectedCategories.Select(c => new TimeRecordingCategoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    DisplayOrder = c.DisplayOrder
                }).ToList(),
                Entries = timeEntries.Select(e => new TimeEntryViewModel
                {
                    Id = e.Id,
                    PeriodStart = e.PeriodStart,
                    PeriodEnd = e.PeriodEnd,
                    Hours = e.GetHours(),
                    Notes = e.Notes,
                    CreatedBy = e.CreatedBy,
                    CreatedAt = e.CreatedAt,
                    ModifiedBy = e.ModifiedBy,
                    ModifiedAt = e.ModifiedAt
                }).ToList(),
                TotalsByCategory = totalsByCategory
            };
        }
        else
        {
            model.Sharing = new SharingViewModel();
            model.TimeRecording = new TimeRecordingViewModel { EnhancementId = enhancementId };
        }

        // Notifications
        if (!isNew)
        {
            var notificationRecipients = await _context.EnhancementNotificationRecipients
                .Include(r => r.Resource)
                .ThenInclude(r => r.ResourceType)
                .Where(r => r.EnhancementId == enhancementId)
                .ToListAsync();

            // Get all active resources (all types) for notification selection
            var allActiveResources = await _resourceService.GetActiveAsync();

            model.Notifications = new NotificationsViewModel
            {
                EnhancementId = enhancementId,
                SelectedRecipientIds = notificationRecipients.Select(r => r.ResourceId).ToList(),
                AvailableResources = allActiveResources.Select(r => new NotificationRecipientOption
                {
                    Id = r.Id,
                    Name = r.Name,
                    Email = r.Email,
                    ResourceType = r.ResourceType?.Name
                }).ToList(),
                CurrentRecipients = notificationRecipients.Select(r => new NotificationRecipientViewModel
                {
                    Id = r.Id,
                    ResourceId = r.ResourceId,
                    ResourceName = r.Resource?.Name ?? "Unknown",
                    Email = r.Resource?.Email,
                    ResourceType = r.Resource?.ResourceType?.Name,
                    AddedAt = r.CreatedAt,
                    AddedBy = r.CreatedBy
                }).ToList()
            };
        }
        else
        {
            model.Notifications = new NotificationsViewModel();
        }







        return model;
    }

    private async Task<List<string>> GetExistingServiceAreasForWorkIdAsync(string workId, string excludeServiceAreaId)
    {
        var allServiceAreas = await _serviceAreaService.GetAllAsync();
        var result = new List<string>();

        foreach (var sa in allServiceAreas.Where(s => s.Id != excludeServiceAreaId))
        {
            if (await _sharingService.ExistsInServiceAreaAsync(workId, sa.Id))
            {
                result.Add(sa.Name);
            }
        }

        return result;
    }

    #endregion

    #region Save Ticket Details

    [HttpPost("SaveDetails")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDetails([FromBody] SaveTicketDetailsRequest request)
    {
        try
        {
            var isNew = string.IsNullOrEmpty(request.Id);

            var enhancement = new Enhancement
            {
                Id = request.Id ?? Guid.NewGuid().ToString(),
                WorkId = request.WorkId,
                Description = request.Description,
                Notes = request.Notes,
                ServiceAreaId = request.ServiceAreaId,

                // Sizing - L3H fields
                EstimatedHours = request.EstimatedHours,
                EstimatedStartDate = request.EstimatedStartDate,
                EstimatedEndDate = request.EstimatedEndDate,
                EstimationNotes = request.EstimationNotes,
                Status = request.Status,
                ServiceLine = request.ServiceLine,
                LaborType = request.LaborType,           // ADD THIS
                Priority = request.Priority,              // ADD THIS

                // Actual/Core Info - Inf* fields
                ReturnedHours = request.ReturnedHours,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                InfStatus = request.InfStatus,
                InfServiceLine = request.InfServiceLine,
                InfLaborType = request.InfLaborType,     // ADD THIS
                InfPriority = request.InfPriority,       // ADD THIS

                // Legacy time allocations
                TimeW1 = request.TimeW1,
                TimeW2 = request.TimeW2,
                TimeW3 = request.TimeW3,
                TimeW4 = request.TimeW4,
                TimeW5 = request.TimeW5,
                TimeW6 = request.TimeW6,
                TimeW7 = request.TimeW7,
                TimeW8 = request.TimeW8,
                TimeW9 = request.TimeW9
            };

            if (isNew)
            {
                await _enhancementService.CreateAsync(enhancement, CurrentUserId!);
            }
            else
            {
                await _enhancementService.UpdateAsync(enhancement, CurrentUserId!);
            }

            // Update resources
            await _enhancementService.UpdateSponsorsAsync(enhancement.Id, request.SponsorIds, CurrentUserId!);
            await _enhancementService.UpdateSpocsAsync(enhancement.Id, request.SpocIds, CurrentUserId!);
            await _enhancementService.UpdateResourcesAsync(enhancement.Id, request.ResourceIds, CurrentUserId!);

            // Update skills
            await _skillService.UpdateEnhancementSkillsAsync(enhancement.Id, request.SkillIds);

            // Update time categories
            await _timeRecordingService.SetCategoriesForEnhancementAsync(enhancement.Id, request.TimeCategoryIds);

            return Json(new { success = true, id = enhancement.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Attachments

    [HttpPost("UploadAttachment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachment(string enhancementId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "No file provided." });

            var validation = _attachmentService.ValidateFile(file.FileName, file.Length);
            if (!validation.isValid)
                return Json(new { success = false, message = validation.errorMessage });

            using var stream = file.OpenReadStream();
            var attachment = await _attachmentService.UploadAsync(
                enhancementId,
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                CurrentUserId!);

            return Json(new { success = true, id = attachment.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet("DownloadAttachment")]
    public async Task<IActionResult> DownloadAttachment(string id)
    {
        var (stream, contentType, fileName) = await _attachmentService.DownloadAsync(id);
        if (stream == null)
            return NotFound();

        return File(stream, contentType ?? "application/octet-stream", fileName);
    }

    [HttpPost("DeleteAttachment")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment([FromBody] DeleteRequest request)
    {
        var result = await _attachmentService.DeleteAsync(request.Id, CurrentUserId!);
        return Json(new { success = result });
    }

    #endregion

    #region Notes

    [HttpPost("SaveNote")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveNote([FromBody] SaveNoteRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Id))
            {
                await _noteService.CreateAsync(request.EnhancementId, request.NoteText, CurrentUserId!);
            }
            else
            {
                await _noteService.UpdateAsync(request.Id, request.NoteText, CurrentUserId!);
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("DeleteNote")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteNote([FromBody] DeleteRequest request)
    {
        var result = await _noteService.DeleteAsync(request.Id, CurrentUserId!);
        return Json(new { success = result });
    }

    #endregion

    #region Sharing

    [HttpPost("Share")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Share([FromBody] ShareEnhancementRequest request)
    {
        try
        {
            var newEnhancement = await _sharingService.ShareToServiceAreaAsync(
                request.EnhancementId,
                request.TargetServiceAreaId,
                CurrentUserId!);

            return Json(new
            {
                success = true,
                newEnhancementId = newEnhancement.Id,
                serviceAreaId = newEnhancement.ServiceAreaId
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Time Recording

    [HttpPost("SaveTimeEntry")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTimeEntry([FromBody] SaveTimeEntryRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Id))
            {
                await _timeRecordingService.CreateEntryAsync(
                    request.EnhancementId,
                    request.PeriodStart,
                    request.PeriodEnd,
                    request.Hours,
                    request.Notes,
                    CurrentUserId!);
            }
            else
            {
                await _timeRecordingService.UpdateEntryAsync(
                    request.Id,
                    request.PeriodStart,
                    request.PeriodEnd,
                    request.Hours,
                    request.Notes,
                    CurrentUserId!);
            }

            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost("DeleteTimeEntry")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTimeEntry([FromBody] DeleteRequest request)
    {
        var result = await _timeRecordingService.DeleteEntryAsync(request.Id, CurrentUserId!);
        return Json(new { success = result });
    }

    #endregion

    // Add these methods to EnhancementDetailsController.cs

    #region Notification Recipients

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNotificationRecipient([FromBody] AddNotificationRecipientRequest request)
    {
        if (string.IsNullOrEmpty(request.EnhancementId) || string.IsNullOrEmpty(request.ResourceId))
            return Json(new { success = false, message = "Invalid request" });

        var enhancement = await _enhancementService.GetByIdAsync(request.EnhancementId);
        if (enhancement == null)
            return Json(new { success = false, message = "Enhancement not found" });

        // Check if already exists
        var existing = await _context.EnhancementNotificationRecipients
            .FirstOrDefaultAsync(r => r.EnhancementId == request.EnhancementId && r.ResourceId == request.ResourceId);

        if (existing != null)
            return Json(new { success = false, message = "Recipient already added" });

        var recipient = new EnhancementNotificationRecipient
        {
            EnhancementId = request.EnhancementId,
            ResourceId = request.ResourceId,
            CreatedBy = CurrentUserId
        };

        _context.EnhancementNotificationRecipients.Add(recipient);
        await _context.SaveChangesAsync();

        return Json(new { success = true, recipientId = recipient.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveNotificationRecipient([FromBody] RemoveNotificationRecipientRequest request)
    {
        if (string.IsNullOrEmpty(request.RecipientId))
            return Json(new { success = false, message = "Invalid request" });

        var recipient = await _context.EnhancementNotificationRecipients.FindAsync(request.RecipientId);
        if (recipient == null)
            return Json(new { success = false, message = "Recipient not found" });

        _context.EnhancementNotificationRecipients.Remove(recipient);
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    #endregion
}

public class DeleteRequest
{
    public string Id { get; set; } = string.Empty;
}


// Request models - add to ViewModels folder or at the end of the controller file
public class AddNotificationRecipientRequest
{
    public string EnhancementId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
}

public class RemoveNotificationRecipientRequest
{
    public string RecipientId { get; set; } = string.Empty;
}
