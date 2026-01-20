using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
public class AdminController : BaseController
{
    private readonly IUserService _userService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAuthService authService,
        IUserService userService,
        IServiceAreaService serviceAreaService,
        ILogger<AdminController> logger) : base(authService)
    {
        _userService = userService;
        _serviceAreaService = serviceAreaService;
        _logger = logger;
    }

    #region Users

    public async Task<IActionResult> Users(string? search, string? role)
    {
        var users = await _userService.GetAllUsersAsync();

        if (!string.IsNullOrEmpty(search))
        {
            var term = search.ToLower();
            users = users
                .Where(u => u.DisplayName.ToLower().Contains(term) || 
                           u.Email.ToLower().Contains(term))
                .ToList();
        }

        if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, out var roleEnum))
        {
            users = users.Where(u => u.Role == roleEnum).ToList();
        }

        var model = new UsersViewModel
        {
            Users = users,
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
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return NotFound();

            model.Id = user.Id;
            model.Email = user.Email;
            model.DisplayName = user.DisplayName;
            model.Role = user.Role;
            model.IsActive = user.IsActive;
            model.SelectedServiceAreaIds = user.ServiceAreas.Select(usa => usa.ServiceAreaId).ToList();
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

            var user = await _userService.CreateUserAsync(model.Email, model.DisplayName, model.Password, model.Role);
            await _userService.UpdateUserServiceAreasAsync(user.Id, model.SelectedServiceAreaIds);
            _logger.LogInformation("User {Email} created by {Admin}", model.Email, CurrentUserEmail);
        }
        else
        {
            // Update existing user
            await _userService.UpdateUserAsync(model.Id, model.DisplayName, model.Role, model.IsActive);
            await _userService.UpdateUserServiceAreasAsync(model.Id, model.SelectedServiceAreaIds);

            if (!string.IsNullOrEmpty(model.Password))
            {
                await _userService.ResetPasswordAsync(model.Id, model.Password);
                _logger.LogInformation("Password reset for {Email} by {Admin}", model.Email, CurrentUserEmail);
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
        {
            return BadRequest("Cannot delete your own account.");
        }

        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        await _userService.DeleteUserAsync(id);
        _logger.LogInformation("User {Email} deleted by {Admin}", user.Email, CurrentUserEmail);

        return Json(new { success = true });
    }

    #endregion

    #region Service Areas

    public async Task<IActionResult> ServiceAreas()
    {
        var serviceAreas = await _serviceAreaService.GetAllAsync(activeOnly: false);

        var model = new ServiceAreasViewModel
        {
            ServiceAreas = serviceAreas
        };

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
        {
            return PartialView("EditServiceArea", model);
        }

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
        {
            return BadRequest("Cannot delete service area with existing enhancements.");
        }

        _logger.LogInformation("Service area {Name} deleted by {Admin}", serviceArea.Name, CurrentUserEmail);

        return Json(new { success = true });
    }

    #endregion
}
