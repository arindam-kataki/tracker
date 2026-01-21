using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracker.Web.ViewModels;

public class SkillsViewModel
{
    public List<SkillDto> Skills { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? ServiceAreaFilter { get; set; }
    public List<SelectListItem> ServiceAreas { get; set; } = new();
}

public class SkillDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int ResourceCount { get; set; }
    public int EnhancementCount { get; set; }
}

public class EditSkillViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Service Area is required")]
    public string ServiceAreaId { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public List<SelectListItem> ServiceAreas { get; set; } = new();
}
