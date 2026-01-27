using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IAuthService _authService;

    public HomeController(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return RedirectToAction("Login", "Account");

        // Get user's first accessible service area and redirect
        var serviceAreas = await _authService.GetUserServiceAreasAsync(userId);
        
        if (serviceAreas.Any())
        {
            return RedirectToAction("Index", "Enhancements", new { serviceAreaId = serviceAreas.First().Id });
        }

        // No service areas assigned - show message
        return View();
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }
}
