using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

/// <summary>
/// Controller for managing users (resources with login capability)
/// </summary>
public class UsersController : BaseController
{
    private readonly TrackerDbContext _db;
    private readonly IResourceService _resourceService;
    private readonly IPermissionService _permissionService;
    
    public UsersController(
        TrackerDbContext db,
        IResourceService resourceService,
        IPermissionService permissionService)
    {
        _db = db;
        _resourceService = resourceService;
        _permissionService = permissionService;
    }
    
    /// <summary>
    /// List all resources (users/contacts)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(ResourceFilterViewModel filter)
    {
        // Only admins can manage users
        if (!CurrentResource.IsAdmin)
        {
            return Forbid();
        }
        
        var resources = await _resourceService.GetAllAsync(filter);
        var serviceAreas = await _db.ServiceAreas
            .Where(sa => sa.IsActive)
            .OrderBy(sa => sa.DisplayOrder)
            .ThenBy(sa => sa.Name)
            .ToListAsync();
        
        var model = new ResourceListViewModel
        {
            Resources = resources.Select(ResourceListItemViewModel.FromEntity).ToList(),
            Filter = filter,
            ServiceAreas = serviceAreas
        };
        
        return View(model);
    }
    
    /// <summary>
    /// Create new resource page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!CurrentResource.IsAdmin)
        {
            return Forbid();
        }
        
        var model = new ResourceEditViewModel
        {
            AvailableServiceAreas = await GetActiveServiceAreasAsync()
        };
        
        return View("Edit", model);
    }
    
    /// <summary>
    /// Edit resource page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (!CurrentResource.IsAdmin)
        {
            return Forbid();
        }
        
        var resource = await _resourceService.GetByIdAsync(id);
        if (resource == null)
        {
            return NotFound();
        }
        
        var model = ResourceEditViewModel.FromEntity(resource);
        model.AvailableServiceAreas = await GetActiveServiceAreasAsync();
        
        return View(model);
    }
    
    /// <summary>
    /// Save resource (create or update)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ResourceEditViewModel model)
    {
        if (!CurrentResource.IsAdmin)
        {
            return Forbid();
        }
        
        // Validate email uniqueness
        if (!string.IsNullOrWhiteSpace(model.Email))
        {
            if (await _resourceService.EmailExistsAsync(model.Email, model.Id))
            {
                ModelState.AddModelError("Email", "Email is already in use");
            }
        }
        
        // Validate password for new login users
        if (model.IsNew && model.CanLogin && string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError("Password", "Password is required for users who can login");
        }
        
        // Validate admin only for Implementor
        if (model.IsAdmin && model.OrganizationType != OrganizationType.Implementor)
        {
            ModelState.AddModelError("IsAdmin", "Only Implementor resources can be admins");
        }
        
        // Validate login only for non-Client
        if (model.CanLogin && model.OrganizationType == OrganizationType.Client)
        {
            ModelState.AddModelError("CanLogin", "Client resources cannot have login access");
        }
        
        if (!ModelState.IsValid)
        {
            model.AvailableServiceAreas = await GetActiveServiceAreasAsync();
            return View("Edit", model);
        }
        
        try
        {
            if (model.IsNew)
            {
                await _resourceService.CreateAsync(model);
                TempData["Success"] = "User created successfully";
            }
            else
            {
                await _resourceService.UpdateAsync(model.Id!, model);
                TempData["Success"] = "User updated successfully";
            }
            
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error saving user: {ex.Message}");
            model.AvailableServiceAreas = await GetActiveServiceAreasAsync();
            return View("Edit", model);
        }
    }
    
    /// <summary>
    /// Delete resource
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (!CurrentResource.IsAdmin)
        {
            return Forbid();
        }
        
        // Prevent self-deletion
        if (id == CurrentResourceId)
        {
            TempData["Error"] = "You cannot delete your own account";
            return RedirectToAction(nameof(Index));
        }
        
        var success = await _resourceService.DeleteAsync(id);
        if (success)
        {
            TempData["Success"] = "User deleted successfully";
        }
        else
        {
            TempData["Error"] = "User not found";
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    /// <summary>
    /// Reset password for a user
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string id, string newPassword)
    {
        if (!CurrentResource.IsAdmin)
        {
            return Forbid();
        }
        
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            TempData["Error"] = "Password must be at least 6 characters";
            return RedirectToAction(nameof(Edit), new { id });
        }
        
        var success = await _resourceService.ResetPasswordAsync(id, newPassword);
        if (success)
        {
            TempData["Success"] = "Password reset successfully";
        }
        else
        {
            TempData["Error"] = "Failed to reset password";
        }
        
        return RedirectToAction(nameof(Edit), new { id });
    }
    
    /// <summary>
    /// AJAX: Add service area membership
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddServiceArea(string resourceId, string serviceAreaId, string template)
    {
        if (!CurrentResource.IsAdmin)
        {
            return Forbid();
        }
        
        var permissions = template?.ToLower() switch
        {
            "spoc" => Permissions.SPOC,
            "developer" => Permissions.Developer,
            "tester" => Permissions.Tester,
            "analyst" => Permissions.Analyst,
            "finance-view" => Permissions.FinanceView,
            "finance-approve" => Permissions.FinanceApprove,
            "hr" => Permissions.HR,
            "reporting" => Permissions.Reporting,
            _ => Permissions.BasicView
        };
        
        var rsa = await _resourceService.AddToServiceAreaAsync(resourceId, serviceAreaId, permissions);
        
        if (rsa == null)
        {
            return Json(new { success = false, message = "Already a member" });
        }
        
        return Json(new { success = true });
    }
    
    /// <summary>
    /// AJAX: Remove service area membership
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> RemoveServiceArea(string resourceId, string serviceAreaId)
    {
        if (!CurrentResource.IsAdmin)
        {
            return Forbid();
        }
        
        var success = await _resourceService.RemoveFromServiceAreaAsync(resourceId, serviceAreaId);
        return Json(new { success });
    }
    
    /// <summary>
    /// Get active service areas for dropdowns
    /// </summary>
    private async Task<List<ServiceArea>> GetActiveServiceAreasAsync()
    {
        return await _db.ServiceAreas
            .Where(sa => sa.IsActive)
            .OrderBy(sa => sa.DisplayOrder)
            .ThenBy(sa => sa.Name)
            .ToListAsync();
    }
}
