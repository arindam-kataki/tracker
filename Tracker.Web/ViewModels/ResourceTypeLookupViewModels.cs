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
    
    // For dropdown - all Enhancement columns
    public List<SelectListItem> EnhancementColumns { get; set; } = new()
    {
        // Resource columns (grouped)
        new SelectListItem { Text = "── Resource Columns ──", Value = "", Disabled = true },
        new SelectListItem("Sponsors", ((int)EnhancementColumnType.Sponsors).ToString()),
        new SelectListItem("SPOCs", ((int)EnhancementColumnType.SPOCs).ToString()),
        new SelectListItem("Resources", ((int)EnhancementColumnType.Resources).ToString()),
        
        // Text columns
        new SelectListItem { Text = "── Text Columns ──", Value = "", Disabled = true },
        new SelectListItem("Status", ((int)EnhancementColumnType.Status).ToString()),
        new SelectListItem("Service Line", ((int)EnhancementColumnType.ServiceLine).ToString()),
        new SelectListItem("INF Status", ((int)EnhancementColumnType.InfStatus).ToString()),
        new SelectListItem("INF Service Line", ((int)EnhancementColumnType.InfServiceLine).ToString()),
        new SelectListItem("Notes", ((int)EnhancementColumnType.Notes).ToString()),
        new SelectListItem("Estimation Notes", ((int)EnhancementColumnType.EstimationNotes).ToString()),
        
        // Numeric columns
        new SelectListItem { Text = "── Numeric Columns ──", Value = "", Disabled = true },
        new SelectListItem("Estimated Hours", ((int)EnhancementColumnType.EstimatedHours).ToString()),
        new SelectListItem("Returned Hours", ((int)EnhancementColumnType.ReturnedHours).ToString()),
        new SelectListItem("Time W1", ((int)EnhancementColumnType.TimeW1).ToString()),
        new SelectListItem("Time W2", ((int)EnhancementColumnType.TimeW2).ToString()),
        new SelectListItem("Time W3", ((int)EnhancementColumnType.TimeW3).ToString()),
        new SelectListItem("Time W4", ((int)EnhancementColumnType.TimeW4).ToString()),
        new SelectListItem("Time W5", ((int)EnhancementColumnType.TimeW5).ToString()),
        new SelectListItem("Time W6", ((int)EnhancementColumnType.TimeW6).ToString()),
        new SelectListItem("Time W7", ((int)EnhancementColumnType.TimeW7).ToString()),
        new SelectListItem("Time W8", ((int)EnhancementColumnType.TimeW8).ToString()),
        new SelectListItem("Time W9", ((int)EnhancementColumnType.TimeW9).ToString()),
        
        // Date columns
        new SelectListItem { Text = "── Date Columns ──", Value = "", Disabled = true },
        new SelectListItem("Est. Start Date", ((int)EnhancementColumnType.EstimatedStartDate).ToString()),
        new SelectListItem("Est. End Date", ((int)EnhancementColumnType.EstimatedEndDate).ToString()),
        new SelectListItem("Start Date", ((int)EnhancementColumnType.StartDate).ToString()),
        new SelectListItem("End Date", ((int)EnhancementColumnType.EndDate).ToString()),
    };
}
