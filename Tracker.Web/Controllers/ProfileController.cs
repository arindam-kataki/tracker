using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

/// <summary>
/// Controller for user profile management
/// </summary>
public class ProfileController : BaseController
{
    private readonly IResourceService _resourceService;
    private readonly IPermissionService _permissionService;
    
    public ProfileController(
        IResourceService resourceService,
        IPermissionService permissionService)
    {
        _resourceService = resourceService;
        _permissionService = permissionService;
    }
    
    /// <summary>
    /// View current user's profile
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var resource = await _resourceService.GetByIdAsync(CurrentResourceId);
        if (resource == null)
        {
            return RedirectToAction("Login", "Account");
        }
        
        var model = ProfileViewModel.FromEntity(resource);
        return View(model);
    }
    
    /// <summary>
    /// Change password page
    /// </summary>
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }
    
    /// <summary>
    /// Process password change
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var success = await _resourceService.ChangePasswordAsync(
            CurrentResourceId, 
            model.CurrentPassword, 
            model.NewPassword);
        
        if (!success)
        {
            ModelState.AddModelError("CurrentPassword", "Current password is incorrect");
            return View(model);
        }
        
        TempData["Success"] = "Password changed successfully";
        return RedirectToAction(nameof(Index));
    }
}
