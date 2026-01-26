namespace Tracker.Web.Entities;

/// <summary>
/// Junction table linking Resources to Service Areas with permissions.
/// Defines what a resource can do within a specific service area.
/// </summary>
public class ResourceServiceArea
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // === FOREIGN KEYS ===
    public string ResourceId { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    
    // === MEMBERSHIP INFO ===
    
    /// <summary>
    /// Is this the primary service area for this resource?
    /// </summary>
    public bool IsPrimary { get; set; } = false;
    
    /// <summary>
    /// When the resource joined this service area
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    
    // === PERMISSIONS ===
    
    /// <summary>
    /// Permissions flags for this resource in this service area
    /// </summary>
    public Permissions Permissions { get; set; } = Permissions.None;
    
    // === NAVIGATION ===
    public virtual Resource Resource { get; set; } = null!;
    public virtual ServiceArea ServiceArea { get; set; } = null!;
    
    // === HELPER METHODS ===
    
    /// <summary>
    /// Check if this membership has a specific permission
    /// </summary>
    public bool HasPermission(Permissions permission) => Permissions.HasFlag(permission);
    
    /// <summary>
    /// Check if this membership has any of the specified permissions
    /// </summary>
    public bool HasAnyPermission(Permissions permissions) => (Permissions & permissions) != 0;
    
    /// <summary>
    /// Check if this membership has all of the specified permissions
    /// </summary>
    public bool HasAllPermissions(Permissions permissions) => (Permissions & permissions) == permissions;
    
    /// <summary>
    /// Get display string for permissions
    /// </summary>
    public string PermissionsDisplay
    {
        get
        {
            var perms = new List<string>();
            
            if (Permissions.HasFlag(Permissions.ViewEnhancements)) perms.Add("View Enh");
            if (Permissions.HasFlag(Permissions.EditEnhancements)) perms.Add("Edit Enh");
            if (Permissions.HasFlag(Permissions.UploadEnhancements)) perms.Add("Upload Enh");
            
            if (Permissions.HasFlag(Permissions.LogTimesheet)) perms.Add("Log Time");
            if (Permissions.HasFlag(Permissions.ViewAllTimesheets)) perms.Add("View Time");
            if (Permissions.HasFlag(Permissions.ApproveTimesheets)) perms.Add("Approve Time");
            
            if (Permissions.HasFlag(Permissions.ViewConsolidation)) perms.Add("View Consol");
            if (Permissions.HasFlag(Permissions.CreateConsolidation)) perms.Add("Create Consol");
            if (Permissions.HasFlag(Permissions.FinalizeConsolidation)) perms.Add("Finalize Consol");
            
            if (Permissions.HasFlag(Permissions.ViewResources)) perms.Add("View Res");
            if (Permissions.HasFlag(Permissions.ManageResources)) perms.Add("Manage Res");
            
            if (Permissions.HasFlag(Permissions.ViewReports)) perms.Add("View Reports");
            
            return perms.Any() ? string.Join(", ", perms) : "None";
        }
    }
}

/// <summary>
/// Permission flags for service area access.
/// Uses flags enum for flexible combination of permissions.
/// </summary>
[Flags]
public enum Permissions
{
    None = 0,
    
    // === ENHANCEMENT PERMISSIONS ===
    
    /// <summary>
    /// Can view enhancements in this service area
    /// </summary>
    ViewEnhancements = 1 << 0,      // 1
    
    /// <summary>
    /// Can edit enhancements in this service area
    /// </summary>
    EditEnhancements = 1 << 1,      // 2
    
    /// <summary>
    /// Can upload/import enhancements in this service area
    /// </summary>
    UploadEnhancements = 1 << 2,    // 4
    
    // === TIMESHEET PERMISSIONS ===
    
    /// <summary>
    /// Can log own timesheet entries
    /// </summary>
    LogTimesheet = 1 << 3,          // 8
    
    /// <summary>
    /// Can view all timesheets in this service area
    /// </summary>
    ViewAllTimesheets = 1 << 4,     // 16
    
    /// <summary>
    /// Can approve timesheets in this service area
    /// </summary>
    ApproveTimesheets = 1 << 5,     // 32
    
    // === CONSOLIDATION PERMISSIONS ===
    
    /// <summary>
    /// Can view consolidation/billing data
    /// </summary>
    ViewConsolidation = 1 << 6,     // 64
    
    /// <summary>
    /// Can create consolidation records
    /// </summary>
    CreateConsolidation = 1 << 7,   // 128
    
    /// <summary>
    /// Can finalize consolidation (mark as invoiced)
    /// </summary>
    FinalizeConsolidation = 1 << 8, // 256
    
    // === RESOURCE PERMISSIONS ===
    
    /// <summary>
    /// Can view resources in this service area
    /// </summary>
    ViewResources = 1 << 9,         // 512
    
    /// <summary>
    /// Can manage (add/edit/delete) resources
    /// </summary>
    ManageResources = 1 << 10,      // 1024
    
    // === REPORT PERMISSIONS ===
    
    /// <summary>
    /// Can view reports for this service area
    /// </summary>
    ViewReports = 1 << 11,          // 2048
    
    // === COMMON COMBINATIONS (Templates) ===
    
    /// <summary>
    /// Basic view-only access
    /// </summary>
    BasicView = ViewEnhancements | ViewResources,
    
    /// <summary>
    /// Developer permissions - view and log time
    /// </summary>
    Developer = ViewEnhancements | LogTimesheet | ViewResources,
    
    /// <summary>
    /// Tester permissions - same as developer
    /// </summary>
    Tester = Developer,
    
    /// <summary>
    /// Analyst permissions - same as developer
    /// </summary>
    Analyst = Developer,
    
    /// <summary>
    /// SPOC/Lead permissions - full project access
    /// </summary>
    SPOC = ViewEnhancements | EditEnhancements | UploadEnhancements |
           LogTimesheet | ViewAllTimesheets | ApproveTimesheets |
           ViewConsolidation | CreateConsolidation |
           ViewResources | ViewReports,
    
    /// <summary>
    /// Finance view-only permissions
    /// </summary>
    FinanceView = ViewEnhancements | ViewConsolidation | ViewReports,
    
    /// <summary>
    /// Finance approve permissions
    /// </summary>
    FinanceApprove = ViewEnhancements | ViewConsolidation | 
                     CreateConsolidation | FinalizeConsolidation | ViewReports,
    
    /// <summary>
    /// HR permissions - resource management
    /// </summary>
    HR = ViewResources | ManageResources,
    
    /// <summary>
    /// Reporting permissions - read-only access to everything
    /// </summary>
    Reporting = ViewEnhancements | ViewConsolidation | ViewReports
}
