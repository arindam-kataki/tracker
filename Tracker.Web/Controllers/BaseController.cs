using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

public abstract class BaseController : Controller
{
    protected readonly IAuthService AuthService;

    protected BaseController(IAuthService authService)
    {
        AuthService = authService;
    }

    protected string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);
    protected string? CurrentUserEmail => User.FindFirstValue(ClaimTypes.Email);
    protected string? CurrentUserName => User.FindFirstValue(ClaimTypes.Name);
    protected bool IsSuperAdmin => User.IsInRole(UserRole.SuperAdmin.ToString());
    protected bool IsReporting => User.IsInRole(UserRole.Reporting.ToString()) || IsSuperAdmin;

    protected async Task<SidebarViewModel> GetSidebarViewModelAsync(string? currentServiceAreaId = null, string currentPage = "")
    {
        var userId = CurrentUserId;
        var serviceAreas = userId != null
            ? await AuthService.GetUserServiceAreasAsync(userId)
            : new List<ServiceArea>();

        return new SidebarViewModel
        {
            ServiceAreas = serviceAreas,
            CurrentServiceAreaId = currentServiceAreaId,
            IsSuperAdmin = IsSuperAdmin,
            CurrentPage = currentPage,
            UserEmail = CurrentUserEmail,      // ADD THIS
            UserRole = IsSuperAdmin ? "SuperAdmin" : (IsReporting ? "Reporting" : "User")  // ADD THIS
        };
    }
}
