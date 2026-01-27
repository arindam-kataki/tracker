using System.ComponentModel.DataAnnotations;
using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

// ===== Authentication =====
public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
    public bool RememberMe { get; set; }
}

// ===== Users =====
public class UsersViewModel
{
    public List<User> Users { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
}

public class EditUserViewModel
{
    public string? Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string DisplayName { get; set; } = string.Empty;

    public string? Password { get; set; }

    public UserRole Role { get; set; } = UserRole.User;

    public bool IsActive { get; set; } = true;

    public List<string> SelectedServiceAreaIds { get; set; } = new();
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
}


// ===== Enhancements =====
public class EnhancementsViewModel
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public List<Enhancement> Enhancements { get; set; } = new();
    
    // Paging
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
    public int StartItem => TotalItems == 0 ? 0 : (PageNumber - 1) * PageSize + 1;
    public int EndItem => Math.Min(PageNumber * PageSize, TotalItems);
    
    // Sorting
    public string SortColumn { get; set; } = "workId";
    public string SortOrder { get; set; } = "asc";
    
    // Filter
    public EnhancementFilterViewModel Filter { get; set; } = new();
    public List<string> AvailableStatuses { get; set; } = new();
    public List<string> AvailableInfStatuses { get; set; } = new();
    public List<string> AvailableServiceLines { get; set; } = new();
    
    // Saved Filters
    public List<SavedFilterViewModel> SavedFilters { get; set; } = new();
    public string? CurrentFilterId { get; set; }
    public string? CurrentFilterName { get; set; }
    
    // Column preferences
    public List<ColumnDefinition> AllColumns { get; set; } = new();
    public List<string> VisibleColumns { get; set; } = new();
    
    // Resources for bulk edit
    public List<Resource> AvailableSponsors { get; set; } = new();  // Client resources
    public List<Resource> AvailableSpocs { get; set; } = new();     // SPOC resources
    public List<Resource> AvailableResources { get; set; } = new(); // Internal resources
    
    // Legacy
    public List<Resource> AvailableContacts { get; set; } = new();
    
    // Legacy compatibility
    public string? SearchTerm => Filter?.Search;
    public List<string> SelectedStatuses => Filter?.Statuses ?? new List<string>();
}

public class EditEnhancementViewModel
{
    public string? Id { get; set; }

    [Required]
    public string WorkId { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;
    public decimal? EstimatedHours { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public string? EstimationNotes { get; set; }
    public string? Status { get; set; }
    public string? ServiceLine { get; set; }
    public decimal? ReturnedHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? InfStatus { get; set; }
    public string? InfServiceLine { get; set; }
    public decimal? TimeW1 { get; set; }
    public decimal? TimeW2 { get; set; }
    public decimal? TimeW3 { get; set; }
    public decimal? TimeW4 { get; set; }
    public decimal? TimeW5 { get; set; }
    public decimal? TimeW6 { get; set; }
    public decimal? TimeW7 { get; set; }
    public decimal? TimeW8 { get; set; }
    public decimal? TimeW9 { get; set; }

    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    public List<Resource> AvailableContacts { get; set; } = new();
    public List<Resource> AvailableResources { get; set; } = new();
    public List<string> SelectedContactIds { get; set; } = new();
    public List<string> SelectedResourceIds { get; set; } = new();
}

public class EstimationBreakdownViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;

    public decimal RequirementsAndEstimation { get; set; }
    public string? RequirementsAndEstimationNotes { get; set; }
    public decimal VendorCoordination { get; set; }
    public string? VendorCoordinationNotes { get; set; }
    public decimal DesignFunctionalTechnical { get; set; }
    public string? DesignFunctionalTechnicalNotes { get; set; }
    public decimal TestingSTI { get; set; }
    public string? TestingSTINotes { get; set; }
    public decimal TestingUAT { get; set; }
    public string? TestingUATNotes { get; set; }
    public decimal GoLiveDeploymentValidation { get; set; }
    public string? GoLiveDeploymentValidationNotes { get; set; }
    public decimal Hypercare { get; set; }
    public string? HypercareNotes { get; set; }
    public decimal Documentation { get; set; }
    public string? DocumentationNotes { get; set; }
    public decimal PMLead { get; set; }
    public string? PMLeadNotes { get; set; }
    public decimal Contingency { get; set; }
    public string? ContingencyNotes { get; set; }

    public decimal TotalHours =>
        RequirementsAndEstimation + VendorCoordination + DesignFunctionalTechnical +
        TestingSTI + TestingUAT + GoLiveDeploymentValidation + Hypercare +
        Documentation + PMLead + Contingency;
}

// ===== Upload =====
public class UploadViewModel
{
    public string? ServiceAreaId { get; set; }
    public string ServiceAreaName { get; set; } = string.Empty;
    public List<UploadRowViewModel> Rows { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public class UploadRowViewModel
{
    public int RowNumber { get; set; }
    public string WorkId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool HasMatch { get; set; }
    public string? MatchedEnhancementId { get; set; }
    public string? MatchInfo { get; set; }
    public bool ShouldImport { get; set; } = true;
}

// ===== Service Areas =====
public class ServiceAreasViewModel
{
    public List<ServiceArea> ServiceAreas { get; set; } = new();
}

public class EditServiceAreaViewModel
{
    public string? Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    public int DisplayOrder { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}

// ===== Bulk Actions =====
public class BulkActionViewModel
{
    public List<string> SelectedIds { get; set; } = new();
    public string Action { get; set; } = string.Empty;
    public string? Value { get; set; }
}

// ===== Sidebar =====
public class SidebarViewModel
{
    public List<ServiceArea> ServiceAreas { get; set; } = new();
    public string? CurrentServiceAreaId { get; set; }
    public bool IsSuperAdmin { get; set; }
    public string? CurrentPage { get; set; }
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public bool CanConsolidate { get; set; }
	public string? UserName { get; set; }
}
