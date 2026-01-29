using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public class AdminController : BaseController
{
    private readonly IResourceService _resourceService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAuthService authService,
        IResourceService resourceService,
        IServiceAreaService serviceAreaService,
        ILogger<AdminController> logger) : base(authService)
    {
        _resourceService = resourceService;
        _serviceAreaService = serviceAreaService;
        _logger = logger;
    }

    #region Users (Login-enabled Resources)

    public async Task<IActionResult> Users(string? search, string? role)
    {
        var allResources = await _resourceService.GetAllWithFiltersAsync(search, null, null);
        var loginResources = allResources.Where(r => r.HasLoginAccess).ToList();

        if (!string.IsNullOrEmpty(role))
        {
            if (role == "SuperAdmin")
                loginResources = loginResources.Where(r => r.IsAdmin).ToList();
            else if (role == "User")
                loginResources = loginResources.Where(r => !r.IsAdmin).ToList();
        }

        var model = new UsersViewModel
        {
            Resources = loginResources,
            SearchTerm = search,
            RoleFilter = role
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "users");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditUser(string? id)
    {
        var model = new EditUserViewModel
        {
            AvailableServiceAreas = await _serviceAreaService.GetAllAsync()
        };

        if (!string.IsNullOrEmpty(id))
        {
            var resource = await _resourceService.GetByIdAsync(id);
            if (resource == null)
                return NotFound();

            model.Id = resource.Id;
            model.Email = resource.Email ?? string.Empty;
            model.DisplayName = resource.Name;
            model.IsAdmin = resource.IsAdmin;
            model.IsActive = resource.IsActive;
            model.CanConsolidate = resource.CanConsolidate;
            model.SelectedServiceAreaIds = resource.ServiceAreas.Select(rsa => rsa.ServiceAreaId).ToList();
        }

        return PartialView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveUser(EditUserViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableServiceAreas = await _serviceAreaService.GetAllAsync();
            return PartialView("EditUser", model);
        }

        if (string.IsNullOrEmpty(model.Id))
        {
            // Create new user
            if (string.IsNullOrEmpty(model.Password))
            {
                ModelState.AddModelError("Password", "Password is required for new users.");
                model.AvailableServiceAreas = await _serviceAreaService.GetAllAsync();
                return PartialView("EditUser", model);
            }

            var createModel = new EditResourceViewModel
            {
                Name = model.DisplayName,
                Email = model.Email,
                OrganizationType = OrganizationType.Implementor,
                IsActive = true,
                HasLoginAccess = true,
                NewPassword = model.Password,  // Use NewPassword, not Password
                IsAdmin = model.IsAdmin,
                CanConsolidate = model.CanConsolidate,
                ServiceAreaMemberships = model.SelectedServiceAreaIds
                    .Select(saId => new EditResourceServiceAreaViewModel
                    {
                        ServiceAreaId = saId,
                        IsPrimary = false,
                        // Set individual permission booleans instead of Permissions property
                        ViewEnhancements = true,
                        LogTimesheet = true
                    }).ToList()
            };

            var result = await _resourceService.CreateResourceAsync(createModel);
            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error);
                model.AvailableServiceAreas = await _serviceAreaService.GetAllAsync();
                return PartialView("EditUser", model);
            }

            _logger.LogInformation("User {Email} created by {Admin}", model.Email, CurrentUserEmail);
        }
        else
        {
            // Update existing user
            var existingResource = await _resourceService.GetByIdAsync(model.Id);
            if (existingResource == null)
                return NotFound();

            var updateModel = new EditResourceViewModel
            {
                Id = model.Id,
                Name = model.DisplayName,
                Email = model.Email,
                OrganizationType = existingResource.OrganizationType,
                IsActive = model.IsActive,
                HasLoginAccess = true,
                IsAdmin = model.IsAdmin,
                CanConsolidate = model.CanConsolidate,
                ServiceAreaMemberships = model.SelectedServiceAreaIds
                    .Select(saId => {
                        var existingMembership = existingResource.ServiceAreas
                            .FirstOrDefault(sa => sa.ServiceAreaId == saId);
                        
                        var vm = new EditResourceServiceAreaViewModel
                        {
                            ServiceAreaId = saId,
                            IsPrimary = existingMembership?.IsPrimary ?? false
                        };
                        
                        // Preserve existing permissions or set defaults
                        if (existingMembership != null)
                        {
                            vm.FromPermissions(existingMembership.Permissions);
                        }
                        else
                        {
                            vm.ViewEnhancements = true;
                            vm.LogTimesheet = true;
                        }
                        
                        return vm;
                    }).ToList()
            };

            var result = await _resourceService.UpdateResourceAsync(updateModel);
            if (!result.Success)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error);
                model.AvailableServiceAreas = await _serviceAreaService.GetAllAsync();
                return PartialView("EditUser", model);
            }

            // Handle password reset if provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                var passwordResult = await _resourceService.ResetPasswordAsync(model.Id, model.Password);
                if (passwordResult.Success)
                {
                    _logger.LogInformation("Password reset for {Email} by {Admin}", model.Email, CurrentUserEmail);
                }
            }

            _logger.LogInformation("User {Email} updated by {Admin}", model.Email, CurrentUserEmail);
        }

        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        if (id == CurrentUserId)
            return BadRequest("Cannot delete your own account.");

        var resource = await _resourceService.GetByIdAsync(id);
        if (resource == null)
            return NotFound();

        var result = await _resourceService.DeleteResourceAsync(id);
        if (!result.Success)
            return BadRequest(string.Join(", ", result.Errors));

        _logger.LogInformation("User {Email} deleted by {Admin}", resource.Email, CurrentUserEmail);
        return Json(new { success = true });
    }

    #endregion

    #region Service Areas

    public async Task<IActionResult> ServiceAreas()
    {
        var serviceAreas = await _serviceAreaService.GetAllAsync(activeOnly: false);
        var model = new ServiceAreasViewModel { ServiceAreas = serviceAreas };
        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "serviceareas");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditServiceArea(string? id)
    {
        var model = new EditServiceAreaViewModel();

        if (!string.IsNullOrEmpty(id))
        {
            var serviceArea = await _serviceAreaService.GetByIdAsync(id);
            if (serviceArea == null)
                return NotFound();

            model.Id = serviceArea.Id;
            model.Name = serviceArea.Name;
            model.Code = serviceArea.Code;
            model.IsActive = serviceArea.IsActive;
            model.DisplayOrder = serviceArea.DisplayOrder;
        }

        return PartialView(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveServiceArea(EditServiceAreaViewModel model)
    {
        if (!ModelState.IsValid)
            return PartialView("EditServiceArea", model);

        if (string.IsNullOrEmpty(model.Id))
        {
            await _serviceAreaService.CreateAsync(model.Name, model.Code);
            _logger.LogInformation("Service area {Name} created by {Admin}", model.Name, CurrentUserEmail);
        }
        else
        {
            await _serviceAreaService.UpdateAsync(model.Id, model.Name, model.Code, model.IsActive, model.DisplayOrder);
            _logger.LogInformation("Service area {Name} updated by {Admin}", model.Name, CurrentUserEmail);
        }

        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteServiceArea(string id)
    {
        var serviceArea = await _serviceAreaService.GetByIdAsync(id);
        if (serviceArea == null)
            return NotFound();

        var deleted = await _serviceAreaService.DeleteAsync(id);
        if (!deleted)
            return BadRequest("Cannot delete service area with existing enhancements.");

        _logger.LogInformation("Service area {Name} deleted by {Admin}", serviceArea.Name, CurrentUserEmail);
        return Json(new { success = true });
    }

    #endregion
}