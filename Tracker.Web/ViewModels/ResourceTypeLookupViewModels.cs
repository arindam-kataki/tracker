using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

public class ResourceTypesViewModel
{
    public List<ResourceTypeLookupDto> ResourceTypes { get; set; } = new();
    public string? SearchTerm { get; set; }
}

public class ResourceTypeLookupDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public int ResourceCount { get; set; }
    public EnhancementColumnType EnhancementColumn { get; set; }
    public string EnhancementColumnDisplay { get; set; } = string.Empty;
    public bool AllowMultiple { get; set; }
}

public class EditResourceTypeViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
    [Required(ErrorMessage = "Enhancement Column is required")]
    public EnhancementColumnType EnhancementColumn { get; set; } = EnhancementColumnType.Resources;
    
    public bool AllowMultiple { get; set; } = true;
    
    // For dropdown
    public List<SelectListItem> EnhancementColumns { get; set; } = new()
    {
        new SelectListItem("Sponsors", "0"),
        new SelectListItem("SPOCs", "1"),
        new SelectListItem("Resources", "2")
    };
}
