using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _authService.ValidateCredentialsAsync(model.Email, model.Password);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        _logger.LogInformation("User {Email} logged in at {Time}", user.Email, DateTime.UtcNow);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User {Email} logged out at {Time}", email, DateTime.UtcNow);
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
