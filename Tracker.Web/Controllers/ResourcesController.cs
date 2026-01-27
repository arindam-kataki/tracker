using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
[Route("resources")]
public class ResourcesController : BaseController
{
    private readonly IResourceService _resourceService;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(
        IAuthService authService,
        IResourceService resourceService,
        ILogger<ResourcesController> logger) : base(authService)
    {
        _resourceService = resourceService;
        _logger = logger;
    }

    /// <summary>
    /// List all resources with filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? orgType, string? serviceArea)
    {
        // Parse org type filter
        OrganizationType? orgTypeFilter = null;
        if (!string.IsNullOrEmpty(orgType) && Enum.TryParse<OrganizationType>(orgType, out var parsed))
        {
            orgTypeFilter = parsed;
        }

        var resources = await _resourceService.GetAllWithFiltersAsync(search, orgTypeFilter, serviceArea);

        var model = new ResourcesViewModel
        {
            Resources = resources,
            SearchTerm = search,
            OrgTypeFilter = orgTypeFilter,
            ServiceAreaFilter = serviceArea,
            OrganizationTypes = _resourceService.GetOrganizationTypesSelectList(),
            ServiceAreas = await _resourceService.GetServiceAreasSelectListAsync()
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "resources");
        return View("Resources", model);
    }

    /// <summary>
    /// Full-page Create resource form
    /// </summary>
    [HttpGet("create")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Create()
    {
        var model = new EditResourceViewModel
        {
            IsActive = true,
            OrganizationType = OrganizationType.Implementor,
            OrganizationTypeOptions = _resourceService.GetOrganizationTypesSelectList(OrganizationType.Implementor),
            AvailableServiceAreas = await _resourceService.GetAvailableServiceAreasAsync(),
            AvailableSkillsGrouped = await _resourceService.GetSkillsGroupedByServiceAreaAsync()
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "resources");
        return View("Edit", model);
    }

    /// <summary>
    /// Full-page Edit resource form
    /// </summary>
    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var model = await _resourceService.GetForEditAsync(id);
        if (model == null)
            return NotFound();

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "resources");
        return View("Edit", model);
    }

    /// <summary>
    /// Save resource (create or update)
    /// </summary>
    [HttpPost("save")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Save(EditResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            // Reload dropdown options
            model.OrganizationTypeOptions = _resourceService.GetOrganizationTypesSelectList(model.OrganizationType);
            model.AvailableServiceAreas = await _resourceService.GetAvailableServiceAreasAsync();
            model.AvailableSkillsGrouped = await _resourceService.GetSkillsGroupedByServiceAreaAsync(
                model.Id,
                model.ServiceAreaMemberships?.Select(sa => sa.ServiceAreaId).ToList());

            ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "resources");
            return View("Edit", model);
        }

        ResourceOperationResult result;

        if (model.IsNew)
        {
            result = await _resourceService.CreateResourceAsync(model);
            if (result.Success)
            {
                _logger.LogInformation("Resource {Name} created by {User}", model.Name, CurrentUserEmail);
                TempData["SuccessMessage"] = $"Resource '{model.Name}' created successfully.";
            }
        }
        else
        {
            result = await _resourceService.UpdateResourceAsync(model);
            if (result.Success)
            {
                _logger.LogInformation("Resource {Name} updated by {User}", model.Name, CurrentUserEmail);
                TempData["SuccessMessage"] = $"Resource '{model.Name}' updated successfully.";
            }
        }

        if (!result.Success)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }

            model.OrganizationTypeOptions = _resourceService.GetOrganizationTypesSelectList(model.OrganizationType);
            model.AvailableServiceAreas = await _resourceService.GetAvailableServiceAreasAsync();
            model.AvailableSkillsGrouped = await _resourceService.GetSkillsGroupedByServiceAreaAsync(
                model.Id,
                model.ServiceAreaMemberships?.Select(sa => sa.ServiceAreaId).ToList());

            ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "resources");
            return View("Edit", model);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// Delete resource
    /// </summary>
    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        var resource = await _resourceService.GetByIdAsync(id);
        if (resource == null)
            return NotFound();

        var result = await _resourceService.DeleteResourceAsync(id);
        
        if (result.Success)
        {
            _logger.LogInformation("Resource {Name} deleted by {User}", resource.Name, CurrentUserEmail);
            TempData["SuccessMessage"] = $"Resource '{resource.Name}' deleted successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors);
        }

        return RedirectToAction("Index");
    }

    /// <summary>
    /// AJAX: Get service area membership partial for dynamically adding
    /// </summary>
    [HttpGet("service-area-membership-template")]
    public async Task<IActionResult> GetServiceAreaMembershipTemplate(string serviceAreaId, int index)
    {
        var serviceAreas = await _resourceService.GetAvailableServiceAreasAsync();
        var sa = serviceAreas.FirstOrDefault(s => s.Id == serviceAreaId);
        
        if (sa == null)
            return NotFound();

        var model = new EditResourceServiceAreaViewModel
        {
            ServiceAreaId = serviceAreaId,
            ServiceAreaCode = sa.Code,
            ServiceAreaName = sa.Name,
            IsPrimary = false,
            // Default permissions for new membership
            ViewEnhancements = true,
            ViewResources = true
        };

        ViewData["Index"] = index;
        return PartialView("_ServiceAreaMembership", model);
    }

    /// <summary>
    /// AJAX: Get resources eligible for a specific enhancement column
    /// </summary>
    [HttpGet("for-column")]
    public async Task<IActionResult> GetResourcesForColumn(string serviceAreaId, string column)
    {
        var resources = await _resourceService.GetResourcesForColumnAsync(serviceAreaId, column);
        return Json(resources.Select(r => new { r.Id, r.Name, r.Email }));
    }
}
