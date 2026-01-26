using System.ComponentModel.DataAnnotations;
using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

/// <summary>
/// Filter options for resource list (legacy - used by old controllers)
/// </summary>
public class ResourceFilterViewModel
{
    public string? Search { get; set; }
    public OrganizationType? OrganizationType { get; set; }
    public bool? CanLogin { get; set; }
    public bool? IsActive { get; set; }
    public string? ServiceAreaId { get; set; }
}

/// <summary>
/// Change password view model
/// </summary>
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Current password is required")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Please confirm your new password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}