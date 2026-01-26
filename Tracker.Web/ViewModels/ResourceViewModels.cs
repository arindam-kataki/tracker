using System.ComponentModel.DataAnnotations;
using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

/// <summary>
/// Filter options for resource list
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
/// Resource list view model
/// </summary>
public class ResourceListViewModel
{
    public List<ResourceListItemViewModel> Resources { get; set; } = new();
    public ResourceFilterViewModel Filter { get; set; } = new();
    public List<ServiceArea> ServiceAreas { get; set; } = new();
    
    // Grouped resources
    public IEnumerable<IGrouping<OrganizationType, ResourceListItemViewModel>> GroupedResources =>
        Resources.GroupBy(r => r.OrganizationType);
}

/// <summary>
/// Resource item for list display
/// </summary>
public class ResourceListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public OrganizationType OrganizationType { get; set; }
    public bool CanLogin { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
    public List<string> ServiceAreaNames { get; set; } = new();
    public string? PrimaryServiceArea { get; set; }
    
    public string OrganizationTypeDisplay => OrganizationType switch
    {
        OrganizationType.Client => "Client",
        OrganizationType.Implementor => "Implementor",
        OrganizationType.Vendor => "Vendor",
        _ => "Unknown"
    };
    
    public string ServiceAreasDisplay => ServiceAreaNames.Any() 
        ? string.Join(", ", ServiceAreaNames) 
        : "-";
    
    public static ResourceListItemViewModel FromEntity(Resource r)
    {
        return new ResourceListItemViewModel
        {
            Id = r.Id,
            Name = r.Name,
            Email = r.Email,
            Phone = r.Phone,
            OrganizationType = r.OrganizationType,
            CanLogin = r.CanLogin,
            IsAdmin = r.IsAdmin,
            IsActive = r.IsActive,
            ServiceAreaNames = r.ServiceAreas?.Select(sa => sa.ServiceArea?.Name ?? "").Where(n => !string.IsNullOrEmpty(n)).ToList() ?? new(),
            PrimaryServiceArea = r.ServiceAreas?.FirstOrDefault(sa => sa.IsPrimary)?.ServiceArea?.Name
        };
    }
}

/// <summary>
/// Resource edit/create view model
/// </summary>
public class ResourceEditViewModel
{
    public string? Id { get; set; }
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? Email { get; set; }
    
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string? Phone { get; set; }
    
    public OrganizationType OrganizationType { get; set; } = OrganizationType.Implementor;
    
    public bool CanLogin { get; set; } = false;
    
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string? Password { get; set; }
    
    public bool IsAdmin { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    public List<ResourceServiceAreaEditViewModel> ServiceAreas { get; set; } = new();
    
    // For dropdown population
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    
    public bool IsNew => string.IsNullOrEmpty(Id);
    
    public static ResourceEditViewModel FromEntity(Resource r)
    {
        return new ResourceEditViewModel
        {
            Id = r.Id,
            Name = r.Name,
            Email = r.Email,
            Phone = r.Phone,
            OrganizationType = r.OrganizationType,
            CanLogin = r.CanLogin,
            IsAdmin = r.IsAdmin,
            IsActive = r.IsActive,
            ServiceAreas = r.ServiceAreas?.Select(ResourceServiceAreaEditViewModel.FromEntity).ToList() ?? new()
        };
    }
}

/// <summary>
/// Service area membership edit model
/// </summary>
public class ResourceServiceAreaEditViewModel
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public string? ServiceAreaName { get; set; }
    public bool IsPrimary { get; set; }
    public Permissions Permissions { get; set; } = Permissions.None;
    
    // Individual permission checkboxes for UI binding
    public bool CanViewEnhancements
    {
        get => Permissions.HasFlag(Entities.Permissions.ViewEnhancements);
        set => SetPermission(Entities.Permissions.ViewEnhancements, value);
    }
    
    public bool CanEditEnhancements
    {
        get => Permissions.HasFlag(Entities.Permissions.EditEnhancements);
        set => SetPermission(Entities.Permissions.EditEnhancements, value);
    }
    
    public bool CanUploadEnhancements
    {
        get => Permissions.HasFlag(Entities.Permissions.UploadEnhancements);
        set => SetPermission(Entities.Permissions.UploadEnhancements, value);
    }
    
    public bool CanLogTimesheet
    {
        get => Permissions.HasFlag(Entities.Permissions.LogTimesheet);
        set => SetPermission(Entities.Permissions.LogTimesheet, value);
    }
    
    public bool CanViewAllTimesheets
    {
        get => Permissions.HasFlag(Entities.Permissions.ViewAllTimesheets);
        set => SetPermission(Entities.Permissions.ViewAllTimesheets, value);
    }
    
    public bool CanApproveTimesheets
    {
        get => Permissions.HasFlag(Entities.Permissions.ApproveTimesheets);
        set => SetPermission(Entities.Permissions.ApproveTimesheets, value);
    }
    
    public bool CanViewConsolidation
    {
        get => Permissions.HasFlag(Entities.Permissions.ViewConsolidation);
        set => SetPermission(Entities.Permissions.ViewConsolidation, value);
    }
    
    public bool CanCreateConsolidation
    {
        get => Permissions.HasFlag(Entities.Permissions.CreateConsolidation);
        set => SetPermission(Entities.Permissions.CreateConsolidation, value);
    }
    
    public bool CanFinalizeConsolidation
    {
        get => Permissions.HasFlag(Entities.Permissions.FinalizeConsolidation);
        set => SetPermission(Entities.Permissions.FinalizeConsolidation, value);
    }
    
    public bool CanViewResources
    {
        get => Permissions.HasFlag(Entities.Permissions.ViewResources);
        set => SetPermission(Entities.Permissions.ViewResources, value);
    }
    
    public bool CanManageResources
    {
        get => Permissions.HasFlag(Entities.Permissions.ManageResources);
        set => SetPermission(Entities.Permissions.ManageResources, value);
    }
    
    public bool CanViewReports
    {
        get => Permissions.HasFlag(Entities.Permissions.ViewReports);
        set => SetPermission(Entities.Permissions.ViewReports, value);
    }
    
    private void SetPermission(Permissions permission, bool value)
    {
        if (value)
            Permissions |= permission;
        else
            Permissions &= ~permission;
    }
    
    public void ApplyTemplate(string template)
    {
        Permissions = template.ToLower() switch
        {
            "spoc" or "lead" => Entities.Permissions.SPOC,
            "developer" => Entities.Permissions.Developer,
            "tester" => Entities.Permissions.Tester,
            "analyst" => Entities.Permissions.Analyst,
            "finance-view" => Entities.Permissions.FinanceView,
            "finance-approve" => Entities.Permissions.FinanceApprove,
            "hr" => Entities.Permissions.HR,
            "reporting" => Entities.Permissions.Reporting,
            "view-only" => Entities.Permissions.BasicView,
            _ => Permissions.None
        };
    }
    
    public static ResourceServiceAreaEditViewModel FromEntity(ResourceServiceArea rsa)
    {
        return new ResourceServiceAreaEditViewModel
        {
            ServiceAreaId = rsa.ServiceAreaId,
            ServiceAreaName = rsa.ServiceArea?.Name,
            IsPrimary = rsa.IsPrimary,
            Permissions = rsa.Permissions
        };
    }
}

/// <summary>
/// Profile view model for the current user
/// </summary>
public class ProfileViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public OrganizationType OrganizationType { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    public List<ProfileServiceAreaViewModel> ServiceAreas { get; set; } = new();
    
    public string OrganizationTypeDisplay => OrganizationType switch
    {
        OrganizationType.Client => "Client",
        OrganizationType.Implementor => "Implementor",
        OrganizationType.Vendor => "Vendor",
        _ => "Unknown"
    };
    
    public static ProfileViewModel FromEntity(Resource r)
    {
        return new ProfileViewModel
        {
            Id = r.Id,
            Name = r.Name,
            Email = r.Email,
            Phone = r.Phone,
            OrganizationType = r.OrganizationType,
            IsAdmin = r.IsAdmin,
            CreatedAt = r.CreatedAt,
            LastLoginAt = r.LastLoginAt,
            ServiceAreas = r.ServiceAreas?
                .OrderByDescending(sa => sa.IsPrimary)
                .ThenBy(sa => sa.ServiceArea?.Name)
                .Select(ProfileServiceAreaViewModel.FromEntity)
                .ToList() ?? new()
        };
    }
}

/// <summary>
/// Service area info for profile display
/// </summary>
public class ProfileServiceAreaViewModel
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public Permissions Permissions { get; set; }
    public DateTime JoinedAt { get; set; }
    
    public string PermissionsDisplay
    {
        get
        {
            var perms = new List<string>();
            
            if (Permissions.HasFlag(Entities.Permissions.ViewEnhancements)) perms.Add("View Enhancements");
            if (Permissions.HasFlag(Entities.Permissions.EditEnhancements)) perms.Add("Edit Enhancements");
            if (Permissions.HasFlag(Entities.Permissions.UploadEnhancements)) perms.Add("Upload Enhancements");
            if (Permissions.HasFlag(Entities.Permissions.LogTimesheet)) perms.Add("Log Timesheet");
            if (Permissions.HasFlag(Entities.Permissions.ViewAllTimesheets)) perms.Add("View All Timesheets");
            if (Permissions.HasFlag(Entities.Permissions.ApproveTimesheets)) perms.Add("Approve Timesheets");
            if (Permissions.HasFlag(Entities.Permissions.ViewConsolidation)) perms.Add("View Consolidation");
            if (Permissions.HasFlag(Entities.Permissions.CreateConsolidation)) perms.Add("Create Consolidation");
            if (Permissions.HasFlag(Entities.Permissions.FinalizeConsolidation)) perms.Add("Finalize Consolidation");
            if (Permissions.HasFlag(Entities.Permissions.ViewResources)) perms.Add("View Resources");
            if (Permissions.HasFlag(Entities.Permissions.ManageResources)) perms.Add("Manage Resources");
            if (Permissions.HasFlag(Entities.Permissions.ViewReports)) perms.Add("View Reports");
            
            return perms.Any() ? string.Join(", ", perms) : "None";
        }
    }
    
    public static ProfileServiceAreaViewModel FromEntity(ResourceServiceArea rsa)
    {
        return new ProfileServiceAreaViewModel
        {
            ServiceAreaId = rsa.ServiceAreaId,
            ServiceAreaName = rsa.ServiceArea?.Name ?? "",
            ServiceAreaCode = rsa.ServiceArea?.Code ?? "",
            IsPrimary = rsa.IsPrimary,
            Permissions = rsa.Permissions,
            JoinedAt = rsa.JoinedAt
        };
    }
}

/// <summary>
/// Change password view model
/// </summary>
public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "New password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
    public string NewPassword { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Confirm password is required")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
