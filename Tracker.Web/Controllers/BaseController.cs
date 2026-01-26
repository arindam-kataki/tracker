using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Tracker.Web.Entities;

namespace Tracker.Web.Controllers;

/// <summary>
/// Base controller with common functionality
/// </summary>
public abstract class BaseController : Controller
{
    /// <summary>
    /// Current logged-in resource (user)
    /// </summary>
    protected Resource CurrentResource { get; private set; } = null!;
    
    /// <summary>
    /// Current resource ID from session
    /// </summary>
    protected string CurrentResourceId => HttpContext.Session.GetString("ResourceId") ?? "";
    
    /// <summary>
    /// Current resource name from session
    /// </summary>
    protected string CurrentResourceName => HttpContext.Session.GetString("ResourceName") ?? "";
    
    /// <summary>
    /// Whether current user is admin
    /// </summary>
    protected bool IsAdmin => HttpContext.Session.GetString("IsAdmin") == "true";
    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        
        // Skip auth check for Account controller
        if (context.Controller is AccountController)
            return;
        
        // Check if logged in
        var resourceId = HttpContext.Session.GetString("ResourceId");
        if (string.IsNullOrEmpty(resourceId))
        {
            context.Result = RedirectToAction("Login", "Account");
            return;
        }
        
        // Build current resource from session
        CurrentResource = new Resource
        {
            Id = resourceId,
            Name = HttpContext.Session.GetString("ResourceName") ?? "",
            Email = HttpContext.Session.GetString("ResourceEmail"),
            IsAdmin = HttpContext.Session.GetString("IsAdmin") == "true",
            OrganizationType = Enum.Parse<OrganizationType>(
                HttpContext.Session.GetString("OrganizationType") ?? "Implementor")
        };
        
        // Pass to views
        ViewBag.CurrentResource = CurrentResource;
        ViewBag.IsAdmin = CurrentResource.IsAdmin;
    }
}
