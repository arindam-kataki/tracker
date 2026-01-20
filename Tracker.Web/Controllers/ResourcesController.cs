using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize]
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

    public async Task<IActionResult> Index(string? search, string? type)
    {
        bool? isClientFilter = type switch
        {
            "client" => true,
            "internal" => false,
            _ => null
        };

        var resources = await _resourceService.GetAllAsync(isClientFilter);

        if (!string.IsNullOrEmpty(search))
        {
            var term = search.ToLower();
            resources = resources
                .Where(r => r.Name.ToLower().Contains(term) || 
                           (r.Email?.ToLower().Contains(term) ?? false))
                .ToList();
        }

        var model = new ResourcesViewModel
        {
            Resources = resources,
            SearchTerm = search,
            TypeFilter = type
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "resources");
        return View("Resources", model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string? id)
    {
        var model = new EditResourceViewModel();

        if (!string.IsNullOrEmpty(id))
        {
            var resource = await _resourceService.GetByIdAsync(id);
            if (resource == null)
                return NotFound();

            model.Id = resource.Id;
            model.Name = resource.Name;
            model.Email = resource.Email;
            model.IsClientResource = resource.IsClientResource;
            model.IsActive = resource.IsActive;
        }

        return PartialView("EditResource", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Save(EditResourceViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("EditResource", model);
        }

        if (string.IsNullOrEmpty(model.Id))
        {
            await _resourceService.CreateAsync(model.Name, model.Email, model.IsClientResource);
            _logger.LogInformation("Resource {Name} created by {User}", model.Name, CurrentUserEmail);
        }
        else
        {
            await _resourceService.UpdateAsync(model.Id, model.Name, model.Email, model.IsClientResource, model.IsActive);
            _logger.LogInformation("Resource {Name} updated by {User}", model.Name, CurrentUserEmail);
        }

        return Json(new { success = true });
    }

    [HttpPost]
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
