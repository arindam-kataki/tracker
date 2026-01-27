using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

#region List ViewModels

/// <summary>
/// ViewModel for the Resources list page
/// </summary>
public class ResourcesViewModel
{
    public List<ResourceListItem> Resources { get; set; } = new();
    public string? SearchTerm { get; set; }
    
    // Legacy filter
    public string? TypeFilter { get; set; }
    
    // New filters
    public OrganizationType? OrgTypeFilter { get; set; }
    public string? ServiceAreaFilter { get; set; }
    
    // Dropdown options
    public List<SelectListItem> ResourceTypes { get; set; } = new();
    public List<SelectListItem> OrganizationTypes { get; set; } = new();
    public List<SelectListItem> ServiceAreas { get; set; } = new();
    
    public int TotalCount => Resources.Count;
    public int ActiveCount => Resources.Count(r => r.IsActive);
}

/// <summary>
/// DTO for displaying resource in list
/// </summary>
public class ResourceListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public OrganizationType OrganizationType { get; set; }
    public bool IsActive { get; set; }
    public bool HasLogin { get; set; }
    public string? UserId { get; set; }
    
    // Legacy fields
    public string? ResourceTypeName { get; set; }
    public string? ResourceTypeId { get; set; }
    public List<string> SkillNames { get; set; } = new();
    
    // New fields
    public List<ResourceServiceAreaSummary> ServiceAreas { get; set; } = new();
    
    public string OrganizationTypeDisplay => OrganizationType.ToDisplayString();
    public string OrganizationTypeBadgeClass => OrganizationType.ToBadgeClass();
    
    public string SkillsDisplay => SkillNames.Any() 
        ? string.Join(", ", SkillNames) 
        : "-";
    
    public string ServiceAreasDisplay
    {
        get
        {
            if (!ServiceAreas.Any()) return "-";
            
            return string.Join(", ", ServiceAreas
                .OrderByDescending(sa => sa.IsPrimary)
                .ThenBy(sa => sa.Code)
                .Select(sa => sa.IsPrimary ? $"{sa.Code}*" : sa.Code));
        }
    }
}

/// <summary>
/// Summary of a resource's service area membership for list display
/// </summary>
public class ResourceServiceAreaSummary
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public Permissions Permissions { get; set; }
}

#endregion

#region Edit ViewModels

/// <summary>
/// Main ViewModel for editing a Resource (tabbed interface)
/// </summary>
public class EditResourceViewModel
{
    public string? Id { get; set; }
    
    // === TAB 1: BASIC INFO ===
    
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;
    
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters")]
    public string? Email { get; set; }
    
    [Phone(ErrorMessage = "Invalid phone number")]
    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string? Phone { get; set; }
    
    [Required(ErrorMessage = "Organization type is required")]
    public OrganizationType OrganizationType { get; set; } = OrganizationType.Implementor;
    
    public bool IsActive { get; set; } = true;
    
    // Legacy field for backward compatibility
    public string? ResourceTypeId { get; set; }
    
    /// <summary>Service area memberships with permissions (NEW)</summary>
    public List<EditResourceServiceAreaViewModel> ServiceAreaMemberships { get; set; } = new();
    
    // === TAB 2: SKILLS ===
    
    /// <summary>Selected skill IDs</summary>
    public List<string>? SelectedSkillIds { get; set; } = new();
    
    // Legacy field
    public List<string>? SkillIds { get; set; } = new();
    
    // === DROPDOWN OPTIONS ===
    
    // Legacy
    public List<SelectListItem> ResourceTypes { get; set; } = new();
    public List<SelectListItem> AvailableSkills { get; set; } = new();
    
    // New
    public List<SelectListItem> OrganizationTypeOptions { get; set; } = new();
    public List<ServiceAreaOption> AvailableServiceAreas { get; set; } = new();
    public List<SkillGroupViewModel> AvailableSkillsGrouped { get; set; } = new();
    
    // === COMPUTED ===
    
    public bool IsNew => string.IsNullOrEmpty(Id);
    public string PageTitle => IsNew ? "Add Resource" : $"Edit Resource: {Name}";
}

/// <summary>
/// ViewModel for a single service area membership with permissions
/// </summary>
public class EditResourceServiceAreaViewModel
{
    public string? Id { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    
    // === PERMISSIONS (individual checkboxes) ===
    
    // Enhancements
    public bool ViewEnhancements { get; set; }
    public bool EditEnhancements { get; set; }
    public bool UploadEnhancements { get; set; }
    
    // Timesheets
    public bool LogTimesheet { get; set; }
    public bool ViewAllTimesheets { get; set; }
    public bool ApproveTimesheets { get; set; }
    
    // Invoicing
    public bool ViewInvoices { get; set; }
    public bool CreateInvoices { get; set; }
    public bool UpdateInvoices { get; set; }
    
    // Resources
    public bool ViewResources { get; set; }
    public bool ManageResources { get; set; }
    
    // Reports
    public bool ViewReports { get; set; }
    
    /// <summary>
    /// Convert individual booleans to Permissions flags
    /// </summary>
    public Permissions ToPermissions()
    {
        var perms = Permissions.None;
        
        if (ViewEnhancements) perms |= Permissions.ViewEnhancements;
        if (EditEnhancements) perms |= Permissions.EditEnhancements;
        if (UploadEnhancements) perms |= Permissions.UploadEnhancements;
        
        if (LogTimesheet) perms |= Permissions.LogTimesheet;
        if (ViewAllTimesheets) perms |= Permissions.ViewAllTimesheets;
        if (ApproveTimesheets) perms |= Permissions.ApproveTimesheets;
        
        if (ViewInvoices) perms |= Permissions.ViewInvoices;
        if (CreateInvoices) perms |= Permissions.CreateInvoices;
        if (UpdateInvoices) perms |= Permissions.UpdateInvoices;
        
        if (ViewResources) perms |= Permissions.ViewResources;
        if (ManageResources) perms |= Permissions.ManageResources;
        
        if (ViewReports) perms |= Permissions.ViewReports;
        
        return perms;
    }
    
    /// <summary>
    /// Populate individual booleans from Permissions flags
    /// </summary>
    public void FromPermissions(Permissions perms)
    {
        ViewEnhancements = perms.HasFlag(Permissions.ViewEnhancements);
        EditEnhancements = perms.HasFlag(Permissions.EditEnhancements);
        UploadEnhancements = perms.HasFlag(Permissions.UploadEnhancements);
        
        LogTimesheet = perms.HasFlag(Permissions.LogTimesheet);
        ViewAllTimesheets = perms.HasFlag(Permissions.ViewAllTimesheets);
        ApproveTimesheets = perms.HasFlag(Permissions.ApproveTimesheets);
        
        ViewInvoices = perms.HasFlag(Permissions.ViewInvoices);
        CreateInvoices = perms.HasFlag(Permissions.CreateInvoices);
        UpdateInvoices = perms.HasFlag(Permissions.UpdateInvoices);
        
        ViewResources = perms.HasFlag(Permissions.ViewResources);
        ManageResources = perms.HasFlag(Permissions.ManageResources);
        
        ViewReports = perms.HasFlag(Permissions.ViewReports);
    }
}

/// <summary>
/// Skills grouped by service area for the Skills tab
/// </summary>
public class SkillGroupViewModel
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public bool IsMember { get; set; }
    public List<SkillOptionViewModel> Skills { get; set; } = new();
}

/// <summary>
/// Individual skill option
/// </summary>
public class SkillOptionViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSelected { get; set; }
}

#endregion

#region API/AJAX ViewModels

/// <summary>
/// Response for resource operations
/// </summary>
public class ResourceOperationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ResourceId { get; set; }
    public List<string> Errors { get; set; } = new();
}

#endregion

// NOTE: ServiceAreaOption is defined in EnhancementDetailsViewModel.cs
// We extend it here with Code property if needed via partial class or just use it as-is
