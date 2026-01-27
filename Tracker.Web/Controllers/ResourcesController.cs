using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? type)
    {
        var resources = await _resourceService.GetAllAsync(search, type);
        var resourceTypes = await _resourceService.GetResourceTypesSelectListAsync();

        var model = new ResourcesViewModel
        {
            Resources = resources,
            SearchTerm = search,
            TypeFilter = type,
            ResourceTypes = resourceTypes
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "resources");
        return View("Resources", model);
    }

    [HttpGet("edit")]
    public async Task<IActionResult> Edit(string? id)
    {
        var resourceTypes = await _resourceService.GetResourceTypesSelectListAsync();
        var allSkills = await _resourceService.GetSkillsSelectListAsync();

        var model = new EditResourceViewModel
        {
            ResourceTypes = resourceTypes,
            AvailableSkills = allSkills
        };

        if (!string.IsNullOrEmpty(id))
        {
            var resource = await _resourceService.GetByIdAsync(id);
            if (resource == null)
                return NotFound();

            var selectedSkillIds = await _resourceService.GetResourceSkillIdsAsync(id);

            model.Id = resource.Id;
            model.Name = resource.Name;
            model.Email = resource.Email;
            model.ResourceTypeId = resource.ResourceTypeId;
            model.IsActive = resource.IsActive;
            model.SkillIds = selectedSkillIds;

            // Update selection state
            model.AvailableSkills = await _resourceService.GetSkillsSelectListAsync(selectedSkillIds);
        }

        return PartialView("EditResource", model);
    }

    [HttpPost("save")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Save(EditResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.ResourceTypes = await _resourceService.GetResourceTypesSelectListAsync();
            model.AvailableSkills = await _resourceService.GetSkillsSelectListAsync(model.SkillIds);
            return PartialView("EditResource", model);
        }

        if (string.IsNullOrEmpty(model.Id))
        {
            await _resourceService.CreateAsync(model.Name, model.Email, model.ResourceTypeId, model.SkillIds);
            _logger.LogInformation("Resource {Name} created by {User}", model.Name, CurrentUserEmail);
        }
        else
        {
            await _resourceService.UpdateAsync(model.Id, model.Name, model.Email, model.ResourceTypeId, model.IsActive, model.SkillIds);
            _logger.LogInformation("Resource {Name} updated by {User}", model.Name, CurrentUserEmail);
        }

        return Json(new { success = true });
    }

    [HttpPost("delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Delete(string id)
    {
        var resource = await _resourceService.GetByIdAsync(id);
        if (resource == null)
            return NotFound();

        await _resourceService.DeleteAsync(id);
        _logger.LogInformation("Resource {Name} deleted by {User}", resource.Name, CurrentUserEmail);

        return Json(new { success = true });
    }
}
