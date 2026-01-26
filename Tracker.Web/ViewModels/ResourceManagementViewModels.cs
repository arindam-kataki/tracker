using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

/// <summary>
/// ViewModel for the Resources list page
/// </summary>
public class ResourcesViewModel
{
    public List<ResourceListItem> Resources { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? TypeFilter { get; set; }
    public string? OrgTypeFilter { get; set; }
    public List<SelectListItem> ResourceTypes { get; set; } = new();
}

/// <summary>
/// DTO for displaying resource in list
/// </summary>
public class ResourceListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ResourceTypeName { get; set; }
    public string? ResourceTypeId { get; set; }
    public OrganizationType OrganizationType { get; set; }
    public bool IsActive { get; set; }
    public List<string> SkillNames { get; set; } = new();
    
    public string OrganizationTypeDisplay => OrganizationType switch
    {
        OrganizationType.Client => "Client",
        OrganizationType.Implementor => "Implementor",
        OrganizationType.Vendor => "Vendor",
        _ => "Unknown"
    };
    
    public string SkillsDisplay => SkillNames.Any() 
        ? string.Join(", ", SkillNames) 
        : "-";
}

/// <summary>
/// ViewModel for editing/creating a Resource
/// </summary>
public class EditResourceViewModel
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? Email { get; set; }
    
    public string? ResourceTypeId { get; set; }
    
    public OrganizationType OrganizationType { get; set; } = OrganizationType.Implementor;
    
    public bool IsActive { get; set; } = true;
    
    public List<string>? SkillIds { get; set; } = new();
    
    // For dropdowns
    public List<SelectListItem> ResourceTypes { get; set; } = new();
    public List<SelectListItem> AvailableSkills { get; set; } = new();
    
    public bool IsNew => string.IsNullOrEmpty(Id);
}