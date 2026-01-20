using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Controllers;

[Authorize]
public class ReportsController : BaseController
{
    public ReportsController(IAuthService authService) : base(authService)
    {
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "reports");
        return View("Reports");
    }
}
