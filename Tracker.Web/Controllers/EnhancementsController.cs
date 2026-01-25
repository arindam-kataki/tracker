using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
public class EnhancementsController : BaseController
{
    private readonly IEnhancementService _enhancementService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly IResourceService _resourceService;
    private readonly ISavedFilterService _savedFilterService;

    public EnhancementsController(
        IAuthService authService,
        IEnhancementService enhancementService, 
        IServiceAreaService serviceAreaService,
        IResourceService resourceService,
        ISavedFilterService savedFilterService) : base(authService)
    {
        _enhancementService = enhancementService;
        _serviceAreaService = serviceAreaService;
        _resourceService = resourceService;
        _savedFilterService = savedFilterService;
    }

    public async Task<IActionResult> Index(
        string serviceAreaId, 
        string? filterId = null, 
        [FromQuery] EnhancementFilterViewModel? filter = null)
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
            return NotFound();

        // Load saved filter if specified
        EnhancementFilterViewModel activeFilter = filter ?? new EnhancementFilterViewModel();
        SavedFilter? loadedFilter = null;

        if (!string.IsNullOrEmpty(filterId))
        {
            loadedFilter = await _savedFilterService.GetByIdAsync(filterId);
            if (loadedFilter != null && loadedFilter.UserId == CurrentUserId)
            {
                activeFilter = EnhancementFilterViewModel.FromJson(loadedFilter.FilterJson);
                // Preserve page/sort from query if provided
                if (filter?.Page > 0) activeFilter.Page = filter.Page;
                if (filter?.PageSize > 0) activeFilter.PageSize = filter.PageSize;
                if (!string.IsNullOrEmpty(filter?.SortColumn)) activeFilter.SortColumn = filter.SortColumn;
                if (!string.IsNullOrEmpty(filter?.SortOrder)) activeFilter.SortOrder = filter.SortOrder;
            }
        }
        else if (filter == null || !filter.HasAnyFilter())
        {
            // Try to load default filter
            var defaultFilter = await _savedFilterService.GetDefaultFilterAsync(CurrentUserId!, serviceAreaId);
            if (defaultFilter != null)
            {
                loadedFilter = defaultFilter;
                activeFilter = EnhancementFilterViewModel.FromJson(defaultFilter.FilterJson);
            }
        }

        // Set defaults for paging/sorting
        if (activeFilter.Page < 1) activeFilter.Page = 1;
        if (activeFilter.PageSize < 1) activeFilter.PageSize = 25;
        if (string.IsNullOrEmpty(activeFilter.SortColumn)) activeFilter.SortColumn = "workIdDescription";
        if (string.IsNullOrEmpty(activeFilter.SortOrder)) activeFilter.SortOrder = "asc";

        // Get enhancements with filter, paging, sorting
        var pagedResult = await _enhancementService.GetByServiceAreaPagedAsync(serviceAreaId, activeFilter);

        // Get filter dropdown options
        var statuses = await _savedFilterService.GetDistinctStatusesAsync(serviceAreaId);
        var infStatuses = await _savedFilterService.GetDistinctInfStatusesAsync(serviceAreaId);
        var serviceLines = await _savedFilterService.GetDistinctServiceLinesAsync(serviceAreaId);
        var savedFilters = await _savedFilterService.GetUserFiltersAsync(CurrentUserId!, serviceAreaId);

        // Get column preferences
        var visibleColumns = await _savedFilterService.GetUserColumnsAsync(CurrentUserId!, serviceAreaId);

        // Get resources for bulk edit by type
        var sponsors = await _resourceService.GetByResourceTypeNameAsync("Client");
        var spocs = await _resourceService.GetByResourceTypeNameAsync("SPOC");
        var resources = await _resourceService.GetByResourceTypeNameAsync("Internal");

        var model = new EnhancementsViewModel
        {
            ServiceAreaId = serviceAreaId,
            ServiceAreaName = serviceArea.Name,
            Enhancements = pagedResult.Items,
            
            // Paging
            PageNumber = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalItems = pagedResult.TotalCount,
            
            // Sorting
            SortColumn = activeFilter.SortColumn ?? "workIdDescription",
            SortOrder = activeFilter.SortOrder ?? "asc",
            
            Filter = activeFilter,
            AvailableStatuses = statuses.Any() ? statuses : new List<string> { "New", "In Progress", "On Hold", "Completed", "Cancelled" },
            AvailableInfStatuses = infStatuses,
            AvailableServiceLines = serviceLines,
            SavedFilters = savedFilters.Select(f => new SavedFilterViewModel
            {
                Id = f.Id,
                Name = f.Name,
                IsDefault = f.IsDefault
            }).ToList(),
            CurrentFilterId = loadedFilter?.Id,
            CurrentFilterName = loadedFilter?.Name,
            AllColumns = ColumnDefinition.GetAllColumns(),
            VisibleColumns = visibleColumns,
            AvailableSponsors = sponsors,
            AvailableSpocs = spocs,
            AvailableResources = resources,
            AvailableContacts = sponsors // Legacy compatibility
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(serviceAreaId);
        return View("Enhancements", model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string? id, string serviceAreaId)
    {
        var enhancement = id != null ? await _enhancementService.GetByIdAsync(id) : null;
        var serviceAreas = await _serviceAreaService.GetAllAsync();
        var clientResources = await _resourceService.GetByResourceTypeNameAsync("Client");
        var spocResources = await _resourceService.GetByResourceTypeNameAsync("SPOC");
        var internalResources = await _resourceService.GetByResourceTypeNameAsync("Internal");

        var model = new EditEnhancementViewModel
        {
            Id = enhancement?.Id,
            WorkId = enhancement?.WorkId ?? string.Empty,
            Description = enhancement?.Description ?? string.Empty,
            Notes = enhancement?.Notes,
            ServiceAreaId = enhancement?.ServiceAreaId ?? serviceAreaId,
            EstimatedHours = enhancement?.EstimatedHours,
            EstimatedStartDate = enhancement?.EstimatedStartDate,
            EstimatedEndDate = enhancement?.EstimatedEndDate,
            EstimationNotes = enhancement?.EstimationNotes,
            Status = enhancement?.Status ?? "New",
            ServiceLine = enhancement?.ServiceLine,
            ReturnedHours = enhancement?.ReturnedHours,
            StartDate = enhancement?.StartDate,
            EndDate = enhancement?.EndDate,
            InfStatus = enhancement?.InfStatus,
            InfServiceLine = enhancement?.InfServiceLine,
            TimeW1 = enhancement?.TimeW1,
            TimeW2 = enhancement?.TimeW2,
            TimeW3 = enhancement?.TimeW3,
            TimeW4 = enhancement?.TimeW4,
            TimeW5 = enhancement?.TimeW5,
            TimeW6 = enhancement?.TimeW6,
            TimeW7 = enhancement?.TimeW7,
            TimeW8 = enhancement?.TimeW8,
            TimeW9 = enhancement?.TimeW9,
            AvailableServiceAreas = serviceAreas,
            AvailableContacts = clientResources,
            AvailableResources = internalResources,
            SelectedContactIds = enhancement?.Contacts?.Select(c => c.ResourceId).ToList() ?? new List<string>(),
            SelectedResourceIds = enhancement?.Resources?.Select(r => r.ResourceId).ToList() ?? new List<string>()
        };

        return PartialView("EditEnhancement", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(EditEnhancementViewModel model)
    {
        var enhancement = new Enhancement
        {
            Id = model.Id ?? Guid.NewGuid().ToString(),
            WorkId = model.WorkId,
            Description = model.Description,
            Notes = model.Notes,
            ServiceAreaId = model.ServiceAreaId,
            EstimatedHours = model.EstimatedHours,
            EstimatedStartDate = model.EstimatedStartDate,
            EstimatedEndDate = model.EstimatedEndDate,
            EstimationNotes = model.EstimationNotes,
            Status = model.Status,
            ServiceLine = model.ServiceLine,
            ReturnedHours = model.ReturnedHours,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            InfStatus = model.InfStatus,
            InfServiceLine = model.InfServiceLine,
            TimeW1 = model.TimeW1,
            TimeW2 = model.TimeW2,
            TimeW3 = model.TimeW3,
            TimeW4 = model.TimeW4,
            TimeW5 = model.TimeW5,
            TimeW6 = model.TimeW6,
            TimeW7 = model.TimeW7,
            TimeW8 = model.TimeW8,
            TimeW9 = model.TimeW9
        };

        if (string.IsNullOrEmpty(model.Id))
        {
            await _enhancementService.CreateAsync(enhancement, CurrentUserId!);
        }
        else
        {
            await _enhancementService.UpdateAsync(enhancement, CurrentUserId!);
        }

        return Json(new { success = true, serviceAreaId = enhancement.ServiceAreaId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id, string serviceAreaId)
    {
        await _enhancementService.DeleteAsync(id, CurrentUserId!);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkActionViewModel model)
    {
        if (model.Action == "status" && !string.IsNullOrEmpty(model.Value))
        {
            await _enhancementService.BulkUpdateStatusAsync(model.SelectedIds, model.Value, CurrentUserId!);
        }
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BulkUpdate([FromBody] BulkUpdateRequest request)
    {
        await _enhancementService.BulkUpdateAsync(request, CurrentUserId!);
        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetBreakdown(string enhancementId)
    {
        var enhancement = await _enhancementService.GetByIdAsync(enhancementId);
        if (enhancement == null)
            return NotFound();

        var breakdown = await _enhancementService.GetBreakdownAsync(enhancementId);

        var model = new EstimationBreakdownViewModel
        {
            EnhancementId = enhancementId,
            WorkId = enhancement.WorkId,
            RequirementsAndEstimation = breakdown?.RequirementsAndEstimation ?? 0,
            RequirementsAndEstimationNotes = breakdown?.RequirementsAndEstimationNotes,
            VendorCoordination = breakdown?.VendorCoordination ?? 0,
            VendorCoordinationNotes = breakdown?.VendorCoordinationNotes,
            DesignFunctionalTechnical = breakdown?.DesignFunctionalTechnical ?? 0,
            DesignFunctionalTechnicalNotes = breakdown?.DesignFunctionalTechnicalNotes,
            TestingSTI = breakdown?.TestingSTI ?? 0,
            TestingSTINotes = breakdown?.TestingSTINotes,
            TestingUAT = breakdown?.TestingUAT ?? 0,
            TestingUATNotes = breakdown?.TestingUATNotes,
            GoLiveDeploymentValidation = breakdown?.GoLiveDeploymentValidation ?? 0,
            GoLiveDeploymentValidationNotes = breakdown?.GoLiveDeploymentValidationNotes,
            Hypercare = breakdown?.Hypercare ?? 0,
            HypercareNotes = breakdown?.HypercareNotes,
            Documentation = breakdown?.Documentation ?? 0,
            DocumentationNotes = breakdown?.DocumentationNotes,
            PMLead = breakdown?.PMLead ?? 0,
            PMLeadNotes = breakdown?.PMLeadNotes,
            Contingency = breakdown?.Contingency ?? 0,
            ContingencyNotes = breakdown?.ContingencyNotes
        };

        return PartialView("_EstimationBreakdown", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveBreakdown(EstimationBreakdownViewModel model)
    {
        var breakdown = new EstimationBreakdown
        {
            EnhancementId = model.EnhancementId,
            RequirementsAndEstimation = model.RequirementsAndEstimation,
            RequirementsAndEstimationNotes = model.RequirementsAndEstimationNotes,
            VendorCoordination = model.VendorCoordination,
            VendorCoordinationNotes = model.VendorCoordinationNotes,
            DesignFunctionalTechnical = model.DesignFunctionalTechnical,
            DesignFunctionalTechnicalNotes = model.DesignFunctionalTechnicalNotes,
            TestingSTI = model.TestingSTI,
            TestingSTINotes = model.TestingSTINotes,
            TestingUAT = model.TestingUAT,
            TestingUATNotes = model.TestingUATNotes,
            GoLiveDeploymentValidation = model.GoLiveDeploymentValidation,
            GoLiveDeploymentValidationNotes = model.GoLiveDeploymentValidationNotes,
            Hypercare = model.Hypercare,
            HypercareNotes = model.HypercareNotes,
            Documentation = model.Documentation,
            DocumentationNotes = model.DocumentationNotes,
            PMLead = model.PMLead,
            PMLeadNotes = model.PMLeadNotes,
            Contingency = model.Contingency,
            ContingencyNotes = model.ContingencyNotes
        };

        await _enhancementService.SaveBreakdownAsync(breakdown);
        return Json(new { success = true });
    }

    // ===== Saved Filters =====

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFilter([FromBody] SaveFilterRequest request)
    {
        try
        {
            var saved = await _savedFilterService.SaveFilterAsync(CurrentUserId!, request);
            return Json(new { success = true, id = saved.Id, name = saved.Name });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFilter(string id)
    {
        var result = await _savedFilterService.DeleteFilterAsync(id, CurrentUserId!);
        return Json(new { success = result });
    }

    [HttpGet]
    public async Task<IActionResult> GetFilter(string id)
    {
        var filter = await _savedFilterService.GetByIdAsync(id);
        if (filter == null || filter.UserId != CurrentUserId)
            return NotFound();

        return Json(new
        {
            id = filter.Id,
            name = filter.Name,
            isDefault = filter.IsDefault,
            filter = EnhancementFilterViewModel.FromJson(filter.FilterJson)
        });
    }

    // ===== Column Preferences =====

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveColumns([FromBody] SaveColumnsRequest request)
    {
        await _savedFilterService.SaveUserColumnsAsync(CurrentUserId!, request.ServiceAreaId, request.Columns);
        return Json(new { success = true });
    }


    
}
