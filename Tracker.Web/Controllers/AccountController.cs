using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Tracker.Web.Services;

namespace Tracker.Web.Controllers;

/// <summary>
/// Controller for authentication
/// </summary>
public class AccountController : BaseController
{
    private readonly IResourceService _resourceService;
    
    public AccountController(IResourceService resourceService)
    {
        _resourceService = resourceService;
    }
    
    /// <summary>
    /// Login page
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Already logged in?
        if (!string.IsNullOrEmpty(HttpContext.Session.GetString("ResourceId")))
        {
            return RedirectToAction("Index", "Home");
        }
        
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginViewModel());
    }
    
    /// <summary>
    /// Process login
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var resource = await _resourceService.AuthenticateAsync(model.Email, model.Password);
        
        if (resource == null)
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }
        
        // Set session
        HttpContext.Session.SetString("ResourceId", resource.Id);
        HttpContext.Session.SetString("ResourceName", resource.Name);
        HttpContext.Session.SetString("ResourceEmail", resource.Email ?? "");
        HttpContext.Session.SetString("IsAdmin", resource.IsAdmin ? "true" : "false");
        HttpContext.Session.SetString("OrganizationType", resource.OrganizationType.ToString());
        
        // Redirect
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        
        return RedirectToAction("Index", "Home");
    }
    
    /// <summary>
    /// Logout
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }
    
    /// <summary>
    /// Access denied page
    /// </summary>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}

/// <summary>
/// Login view model
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
    
    public bool RememberMe { get; set; }
}
