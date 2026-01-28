using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

#region List ViewModels

public class ResourcesViewModel
{
    public List<ResourceListItem> Resources { get; set; } = new();
    public string? SearchTerm { get; set; }
    public string? TypeFilter { get; set; }
    public OrganizationType? OrgTypeFilter { get; set; }
    public string? ServiceAreaFilter { get; set; }
    public List<SelectListItem> ResourceTypes { get; set; } = new();
    public List<SelectListItem> OrganizationTypes { get; set; } = new();
    public List<SelectListItem> ServiceAreas { get; set; } = new();
    
    public int TotalCount => Resources.Count;
    public int ActiveCount => Resources.Count(r => r.IsActive);
    public int AdminCount => Resources.Count(r => r.IsAdmin);
}

public class ResourceListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public OrganizationType OrganizationType { get; set; }
    public bool IsActive { get; set; }
    
    // Authentication
    public bool HasLoginAccess { get; set; }
    public bool IsAdmin { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Legacy
    public string? ResourceTypeName { get; set; }
    public string? ResourceTypeId { get; set; }
    public List<string> SkillNames { get; set; } = new();
    
    // New
    public List<ResourceServiceAreaSummary> ServiceAreas { get; set; } = new();
    
    public string OrganizationTypeDisplay => OrganizationType.ToDisplayString();
    public string OrganizationTypeBadgeClass => OrganizationType.ToBadgeClass();
    public string SkillsDisplay => SkillNames.Any() ? string.Join(", ", SkillNames) : "-";
    
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
    
    public string LastLoginDisplay => LastLoginAt?.ToString("MMM dd, yyyy HH:mm") ?? "Never";
}

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

public class EditResourceViewModel
{
    public string? Id { get; set; }
    
    // === BASIC INFO ===
    
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
    
    // === AUTHENTICATION ===
    
    /// <summary>Can this person login?</summary>
    public bool HasLoginAccess { get; set; } = false;
    
    /// <summary>Is this person a SuperAdmin?</summary>
    public bool IsAdmin { get; set; } = false;
    
    /// <summary>Can this person access Consolidation?</summary>
    public bool CanConsolidate { get; set; } = false;
    
    /// <summary>New password (only set when creating or changing)</summary>
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
    public string? NewPassword { get; set; }
    
    /// <summary>Last login timestamp (read-only)</summary>
    public DateTime? LastLoginAt { get; set; }
    
    /// <summary>Is this the last active admin? (prevents deletion/deactivation)</summary>
    public bool IsLastAdmin { get; set; }
    
    // === SERVICE AREA MEMBERSHIPS ===
    
    public List<EditResourceServiceAreaViewModel> ServiceAreaMemberships { get; set; } = new();
    
    // === SKILLS ===
    
    public List<string>? SelectedSkillIds { get; set; } = new();
    public List<string>? SkillIds { get; set; } = new();
    
    // === LEGACY ===
    
    public string? ResourceTypeId { get; set; }
    
    // === DROPDOWN OPTIONS ===
    
    public List<SelectListItem> ResourceTypes { get; set; } = new();
    public List<SelectListItem> AvailableSkills { get; set; } = new();
    public List<SelectListItem> OrganizationTypeOptions { get; set; } = new();
    public List<ServiceAreaOption> AvailableServiceAreas { get; set; } = new();
    public List<SkillGroupViewModel> AvailableSkillsGrouped { get; set; } = new();
    
    // === COMPUTED ===
    
    public bool IsNew => string.IsNullOrEmpty(Id);
    public string PageTitle => IsNew ? "Add Resource" : $"Edit Resource: {Name}";
}

public class EditResourceServiceAreaViewModel
{
    public string? Id { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
        
    /// <summary>
    /// The resource ID this person reports to within this service area
    /// </summary>
    public string? ReportsToResourceId { get; set; }
    
    /// <summary>
    /// Display name for the reporting manager (for UI display)
    /// </summary>
    public string? ReportsToName { get; set; }
    
    // Permissions
    public bool ViewEnhancements { get; set; }
    public bool EditEnhancements { get; set; }
    public bool UploadEnhancements { get; set; }
    public bool LogTimesheet { get; set; }
    public bool ViewAllTimesheets { get; set; }
    public bool ApproveTimesheets { get; set; }
    public bool ViewInvoices { get; set; }
    public bool CreateInvoices { get; set; }
    public bool UpdateInvoices { get; set; }
    public bool ViewResources { get; set; }
    public bool ManageResources { get; set; }
    public bool ViewReports { get; set; }
    
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

public class SkillGroupViewModel
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public bool IsMember { get; set; }
    public List<SkillOptionViewModel> Skills { get; set; } = new();
}

public class SkillOptionViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSelected { get; set; }
}

#endregion

#region API ViewModels

public class ResourceOperationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ResourceId { get; set; }
    public List<string> Errors { get; set; } = new();
}

#endregion
