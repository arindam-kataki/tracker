using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

/// <summary>
/// Base controller with common functionality for authenticated controllers.
/// Uses ASP.NET Core Identity claims for user identification.
/// </summary>
public abstract class BaseController : Controller
{
    protected readonly IAuthService AuthService;

    protected BaseController(IAuthService authService)
    {
        AuthService = authService;
    }

    /// <summary>
    /// Current user's ID from claims
    /// </summary>
    protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Current user's email from claims
    /// </summary>
    protected string? CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email);

    /// <summary>
    /// Current user's display name from claims
    /// </summary>
    protected string? CurrentUserName => User.FindFirstValue(ClaimTypes.Name);

    /// <summary>
    /// Whether current user is a SuperAdmin
    /// </summary>
    protected bool IsSuperAdmin => User.IsInRole("SuperAdmin");

    /// <summary>
    /// Get sidebar view model for layout
    /// </summary>
    protected async Task<SidebarViewModel> GetSidebarViewModelAsync(string? currentServiceAreaId = null, string? currentPage = null)
    {
        var serviceAreas = new List<ServiceArea>();
        
        if (CurrentUserId != null)
        {
            serviceAreas = await AuthService.GetUserServiceAreasAsync(CurrentUserId);
        }

        return new SidebarViewModel
        {
            ServiceAreas = serviceAreas,
            CurrentServiceAreaId = currentServiceAreaId,
            CurrentPage = currentPage,
            IsSuperAdmin = IsSuperAdmin
        };
    }

    /// <summary>
    /// Get list of service area IDs accessible by current user
    /// </summary>
    protected async Task<List<string>> GetAccessibleServiceAreaIdsAsync()
    {
        if (CurrentUserId == null)
            return new List<string>();

        var serviceAreas = await AuthService.GetUserServiceAreasAsync(CurrentUserId);
        return serviceAreas.Select(sa => sa.Id).ToList();
    }

    /// <summary>
    /// Get list of service areas accessible by current user
    /// </summary>
    protected async Task<List<ServiceArea>> GetAccessibleServiceAreasAsync()
    {
        if (CurrentUserId == null)
            return new List<ServiceArea>();

        return await AuthService.GetUserServiceAreasAsync(CurrentUserId);
    }
}