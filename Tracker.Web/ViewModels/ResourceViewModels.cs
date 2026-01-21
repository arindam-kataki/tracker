using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracker.Web.ViewModels;

public class EditResourceViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Resource Type is required")]
    public string? ResourceTypeId { get; set; }

    public List<string> SkillIds { get; set; } = new();

    public bool IsActive { get; set; } = true;

    // For dropdowns
    public List<SelectListItem> ResourceTypes { get; set; } = new();
    public List<SelectListItem> AvailableSkills { get; set; } = new();
}

public class ResourcesViewModel
{
    public List<ResourceListItem> Resources { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? TypeFilter { get; set; }
    public List<SelectListItem> ResourceTypes { get; set; } = new();
}

public class ResourceListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ResourceTypeName { get; set; }
    public string? ResourceTypeId { get; set; }
    public string SkillsDisplay { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
