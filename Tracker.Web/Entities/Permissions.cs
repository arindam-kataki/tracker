namespace Tracker.Web.Entities;

/// <summary>
/// Permission flags for service area access.
/// </summary>
[Flags]
public enum Permissions
{
    None = 0,
    
    // Enhancements
    ViewEnhancements = 1 << 0,
    EditEnhancements = 1 << 1,
    UploadEnhancements = 1 << 2,
    
    // Timesheets
    LogTimesheet = 1 << 3,
    ViewAllTimesheets = 1 << 4,
    ApproveTimesheets = 1 << 5,
    
    // Invoicing
    ViewInvoices = 1 << 6,
    CreateInvoices = 1 << 7,
    UpdateInvoices = 1 << 8,
    
    // Resources
    ViewResources = 1 << 9,
    ManageResources = 1 << 10,
    
    // Reports
    ViewReports = 1 << 11,
    
    // Combinations
    FullAccess = ViewEnhancements | EditEnhancements | UploadEnhancements |
                 LogTimesheet | ViewAllTimesheets | ApproveTimesheets |
                 ViewInvoices | CreateInvoices | UpdateInvoices |
                 ViewResources | ManageResources | ViewReports
}

public static class PermissionsHelper
{
    public static string ToDisplayString(this Permissions permissions)
    {
        if (permissions == Permissions.None) return "None";
        if (permissions == Permissions.FullAccess) return "Full Access";
        
        var perms = new List<string>();
        if (permissions.HasFlag(Permissions.ViewEnhancements)) perms.Add("View Enh");
        if (permissions.HasFlag(Permissions.EditEnhancements)) perms.Add("Edit Enh");
        if (permissions.HasFlag(Permissions.UploadEnhancements)) perms.Add("Upload Enh");
        if (permissions.HasFlag(Permissions.LogTimesheet)) perms.Add("Log Time");
        if (permissions.HasFlag(Permissions.ViewAllTimesheets)) perms.Add("View Time");
        if (permissions.HasFlag(Permissions.ApproveTimesheets)) perms.Add("Approve Time");
        if (permissions.HasFlag(Permissions.ViewInvoices)) perms.Add("View Inv");
        if (permissions.HasFlag(Permissions.CreateInvoices)) perms.Add("Create Inv");
        if (permissions.HasFlag(Permissions.UpdateInvoices)) perms.Add("Update Inv");
        if (permissions.HasFlag(Permissions.ViewResources)) perms.Add("View Res");
        if (permissions.HasFlag(Permissions.ManageResources)) perms.Add("Manage Res");
        if (permissions.HasFlag(Permissions.ViewReports)) perms.Add("Reports");
        
        return string.Join(", ", perms);
    }
}
