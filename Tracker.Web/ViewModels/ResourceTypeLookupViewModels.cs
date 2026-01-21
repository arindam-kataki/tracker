using System.ComponentModel.DataAnnotations;

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
}
