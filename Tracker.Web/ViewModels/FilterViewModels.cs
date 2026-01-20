using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tracker.Web.ViewModels;

public class EnhancementFilterViewModel
{
    // Text search
    public string? Search { get; set; }
    
    // Multi-select
    public List<string> Statuses { get; set; } = new();
    public List<string> InfStatuses { get; set; } = new();
    public List<string> ServiceLines { get; set; } = new();
    
    // Numeric ranges
    public decimal? EstimatedHoursMin { get; set; }
    public decimal? EstimatedHoursMax { get; set; }
    public decimal? ReturnedHoursMin { get; set; }
    public decimal? ReturnedHoursMax { get; set; }
    
    // Date ranges
    public DateTime? EstimatedStartDateFrom { get; set; }
    public DateTime? EstimatedStartDateTo { get; set; }
    public DateTime? EstimatedEndDateFrom { get; set; }
    public DateTime? EstimatedEndDateTo { get; set; }
    public DateTime? StartDateFrom { get; set; }
    public DateTime? StartDateTo { get; set; }
    public DateTime? EndDateFrom { get; set; }
    public DateTime? EndDateTo { get; set; }
    public DateTime? CreatedAtFrom { get; set; }
    public DateTime? CreatedAtTo { get; set; }
    public DateTime? ModifiedAtFrom { get; set; }
    public DateTime? ModifiedAtTo { get; set; }
    
    // Sorting
    public string? SortColumn { get; set; }
    public string SortOrder { get; set; } = "asc";
    
    // Paging
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    
    // Serialization helpers
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions 
        { 
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    
    public static EnhancementFilterViewModel FromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return new EnhancementFilterViewModel();
            
        try
        {
            return JsonSerializer.Deserialize<EnhancementFilterViewModel>(json, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new EnhancementFilterViewModel();
        }
        catch
        {
            return new EnhancementFilterViewModel();
        }
    }
    
    public bool HasAnyFilter()
    {
        return !string.IsNullOrEmpty(Search) ||
               Statuses.Any() ||
               InfStatuses.Any() ||
               ServiceLines.Any() ||
               EstimatedHoursMin.HasValue ||
               EstimatedHoursMax.HasValue ||
               ReturnedHoursMin.HasValue ||
               ReturnedHoursMax.HasValue ||
               EstimatedStartDateFrom.HasValue ||
               EstimatedStartDateTo.HasValue ||
               EstimatedEndDateFrom.HasValue ||
               EstimatedEndDateTo.HasValue ||
               StartDateFrom.HasValue ||
               StartDateTo.HasValue ||
               EndDateFrom.HasValue ||
               EndDateTo.HasValue ||
               CreatedAtFrom.HasValue ||
               CreatedAtTo.HasValue ||
               ModifiedAtFrom.HasValue ||
               ModifiedAtTo.HasValue;
    }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

public class ColumnDefinition
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsDefault { get; set; } = true;
    public bool IsFrozen { get; set; } = false;
    public string CssClass { get; set; } = string.Empty;
    public bool IsSortable { get; set; } = true;
    
    public static List<ColumnDefinition> GetAllColumns()
    {
        return new List<ColumnDefinition>
        {
            new() { Key = "select", Label = "", IsDefault = true, IsFrozen = true, CssClass = "col-select", IsSortable = false },
            new() { Key = "workIdDescription", Label = "Work ID / Description", IsDefault = true, IsFrozen = true, CssClass = "col-workid-desc" },
            new() { Key = "status", Label = "Status", IsDefault = true, IsFrozen = false },
            new() { Key = "estimatedHours", Label = "Est. Hours", IsDefault = true, IsFrozen = false, CssClass = "text-end" },
            new() { Key = "estimatedStartDate", Label = "Est. Start", IsDefault = true, IsFrozen = false },
            new() { Key = "estimatedEndDate", Label = "Est. End", IsDefault = true, IsFrozen = false },
            new() { Key = "returnedHours", Label = "Ret. Hours", IsDefault = false, IsFrozen = false, CssClass = "text-end" },
            new() { Key = "startDate", Label = "Start Date", IsDefault = false, IsFrozen = false },
            new() { Key = "endDate", Label = "End Date", IsDefault = false, IsFrozen = false },
            new() { Key = "infStatus", Label = "INF Status", IsDefault = false, IsFrozen = false },
            new() { Key = "serviceLine", Label = "Service Line", IsDefault = false, IsFrozen = false },
            new() { Key = "infServiceLine", Label = "INF Service Line", IsDefault = false, IsFrozen = false },
            new() { Key = "notes", Label = "Notes", IsDefault = false, IsFrozen = false },
            new() { Key = "createdAt", Label = "Created", IsDefault = false, IsFrozen = false },
            new() { Key = "createdBy", Label = "Created By", IsDefault = false, IsFrozen = false },
            new() { Key = "modifiedAt", Label = "Modified", IsDefault = false, IsFrozen = false },
            new() { Key = "modifiedBy", Label = "Modified By", IsDefault = false, IsFrozen = false },
            new() { Key = "actions", Label = "", IsDefault = true, IsFrozen = false, CssClass = "col-actions", IsSortable = false }
        };
    }
    
    public static List<ColumnDefinition> GetSortableColumns()
    {
        return GetAllColumns().Where(c => c.IsSortable).ToList();
    }
    
    public static List<string> GetDefaultColumnKeys()
    {
        return GetAllColumns().Where(c => c.IsDefault).Select(c => c.Key).ToList();
    }
}

public class SavedFilterViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

public class SaveFilterRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public EnhancementFilterViewModel Filter { get; set; } = new();
}

public class SaveColumnsRequest
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
}

public class BulkUpdateRequest
{
    public List<string> SelectedIds { get; set; } = new();
    public string? Status { get; set; }
    public string? InfStatus { get; set; }
    public string? ServiceLine { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public List<string>? ResourceIds { get; set; }
    public List<string>? ContactIds { get; set; }
    public bool ClearStartDate { get; set; }
    public bool ClearEndDate { get; set; }
    public bool ClearEstimatedStartDate { get; set; }
    public bool ClearEstimatedEndDate { get; set; }
    public bool ClearResources { get; set; }
    public bool ClearContacts { get; set; }
}
