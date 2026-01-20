using System.Text.Json;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.ViewModels;

/// <summary>
/// View model for the Reports index page showing list of named reports.
/// </summary>
public class ReportsIndexViewModel
{
    public List<NamedReportListItem> Reports { get; set; } = new();
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
    public bool IsSuperAdmin { get; set; }
}

/// <summary>
/// Summary of a named report for list display.
/// </summary>
public class NamedReportListItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> ServiceAreaNames { get; set; } = new();
    public int ColumnCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
}

/// <summary>
/// View model for creating/editing a named report.
/// </summary>
public class NamedReportEditViewModel
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Service Areas
    public List<string> SelectedServiceAreaIds { get; set; } = new();
    public List<ServiceAreaOption> AvailableServiceAreas { get; set; } = new();
    
    // Filter criteria
    public EnhancementFilterViewModel Filter { get; set; } = new();
    
    // Columns to export
    public List<string> SelectedColumns { get; set; } = new();
    public List<ReportColumnOption> AvailableColumns { get; set; } = new();
    
    // Filter dropdown options
    public List<string> AvailableStatuses { get; set; } = new();
    public List<string> AvailableInfStatuses { get; set; } = new();
    public List<string> AvailableServiceLines { get; set; } = new();
    
    // For populating from existing report
    public static NamedReportEditViewModel FromReport(NamedReport report, List<ServiceArea> serviceAreas)
    {
        var vm = new NamedReportEditViewModel
        {
            Id = report.Id,
            Name = report.Name,
            Description = report.Description,
            Filter = EnhancementFilterViewModel.FromJson(report.FilterJson)
        };
        
        // Parse service area IDs
        try
        {
            vm.SelectedServiceAreaIds = JsonSerializer.Deserialize<List<string>>(report.ServiceAreaIdsJson) ?? new();
        }
        catch
        {
            vm.SelectedServiceAreaIds = new();
        }
        
        // Parse columns
        try
        {
            vm.SelectedColumns = JsonSerializer.Deserialize<List<string>>(report.ColumnsJson) ?? new();
        }
        catch
        {
            vm.SelectedColumns = new();
        }
        
        // Populate available service areas
        vm.AvailableServiceAreas = serviceAreas.Select(sa => new ServiceAreaOption
        {
            Id = sa.Id,
            Name = sa.Name,
            IsSelected = vm.SelectedServiceAreaIds.Contains(sa.Id)
        }).ToList();
        
        // Populate available columns
        vm.AvailableColumns = GetAllReportColumns();
        foreach (var col in vm.AvailableColumns)
        {
            col.IsSelected = vm.SelectedColumns.Contains(col.Key);
        }
        
        return vm;
    }
    
    public static List<ReportColumnOption> GetAllReportColumns()
    {
        return new List<ReportColumnOption>
        {
            new() { Key = "workIdDescription", Label = "Work ID / Description", IsDefault = true },
            new() { Key = "status", Label = "Status", IsDefault = true },
            new() { Key = "estimatedHours", Label = "Est. Hours", IsDefault = true },
            new() { Key = "estimatedStartDate", Label = "Est. Start", IsDefault = true },
            new() { Key = "estimatedEndDate", Label = "Est. End", IsDefault = true },
            new() { Key = "sponsors", Label = "Sponsors", IsDefault = false },
            new() { Key = "spocs", Label = "Infy SPOC", IsDefault = false },
            new() { Key = "resources", Label = "Resources", IsDefault = false },
            new() { Key = "returnedHours", Label = "Ret. Hours", IsDefault = false },
            new() { Key = "startDate", Label = "Start Date", IsDefault = false },
            new() { Key = "endDate", Label = "End Date", IsDefault = false },
            new() { Key = "infStatus", Label = "INF Status", IsDefault = false },
            new() { Key = "serviceLine", Label = "Service Line", IsDefault = false },
            new() { Key = "infServiceLine", Label = "INF Service Line", IsDefault = false },
            new() { Key = "notes", Label = "Notes", IsDefault = false },
            new() { Key = "estimationNotes", Label = "Est. Notes", IsDefault = false },
            new() { Key = "timeW1", Label = "Time W1", IsDefault = false },
            new() { Key = "timeW2", Label = "Time W2", IsDefault = false },
            new() { Key = "timeW3", Label = "Time W3", IsDefault = false },
            new() { Key = "timeW4", Label = "Time W4", IsDefault = false },
            new() { Key = "timeW5", Label = "Time W5", IsDefault = false },
            new() { Key = "timeW6", Label = "Time W6", IsDefault = false },
            new() { Key = "timeW7", Label = "Time W7", IsDefault = false },
            new() { Key = "timeW8", Label = "Time W8", IsDefault = false },
            new() { Key = "timeW9", Label = "Time W9", IsDefault = false },
            new() { Key = "createdAt", Label = "Created", IsDefault = false },
            new() { Key = "createdBy", Label = "Created By", IsDefault = false },
            new() { Key = "modifiedAt", Label = "Modified", IsDefault = false },
            new() { Key = "modifiedBy", Label = "Modified By", IsDefault = false }
        };
    }
    
    public static List<string> GetDefaultColumnKeys()
    {
        return GetAllReportColumns().Where(c => c.IsDefault).Select(c => c.Key).ToList();
    }
}

public class ServiceAreaOption
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSelected { get; set; }
}

public class ReportColumnOption
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public bool IsSelected { get; set; }
}

/// <summary>
/// View model for the Run Report page (results view).
/// </summary>
public class ReportResultViewModel
{
    public string ReportId { get; set; } = string.Empty;
    public string ReportName { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Column configuration
    public List<string> Columns { get; set; } = new();
    public bool IncludeServiceAreaColumn { get; set; }
    public Dictionary<string, string> ServiceAreaNames { get; set; } = new();
    
    // Results
    public List<EnhancementExportRow> Items { get; set; } = new();
    
    // Paging
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    
    // Sorting
    public string SortColumn { get; set; } = "workIdDescription";
    public string SortOrder { get; set; } = "asc";
    
    // Helper for getting column labels
    public string GetColumnLabel(string key)
    {
        return key switch
        {
            "serviceArea" => "Service Area",
            "workIdDescription" => "Work ID / Description",
            "status" => "Status",
            "estimatedHours" => "Est. Hours",
            "estimatedStartDate" => "Est. Start",
            "estimatedEndDate" => "Est. End",
            "sponsors" => "Sponsors",
            "spocs" => "Infy SPOC",
            "resources" => "Resources",
            "returnedHours" => "Ret. Hours",
            "startDate" => "Start Date",
            "endDate" => "End Date",
            "infStatus" => "INF Status",
            "serviceLine" => "Service Line",
            "infServiceLine" => "INF Service Line",
            "notes" => "Notes",
            "estimationNotes" => "Est. Notes",
            "createdAt" => "Created",
            "createdBy" => "Created By",
            "modifiedAt" => "Modified",
            "modifiedBy" => "Modified By",
            _ when key.StartsWith("timeW") => key.Replace("timeW", "Time W"),
            _ => key
        };
    }
}

/// <summary>
/// Request model for saving a report.
/// </summary>
public class SaveReportRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> ServiceAreaIds { get; set; } = new();
    public EnhancementFilterViewModel Filter { get; set; } = new();
    public List<string> Columns { get; set; } = new();
}
