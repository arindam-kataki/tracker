namespace Tracker.Web.ViewModels;

public class DashboardViewModel
{
    // Summary Cards
    public int TotalEnhancements { get; set; }
    public int NewCount { get; set; }
    public int InProgressCount { get; set; }
    public int OnHoldCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    
    // Hours Summary
    public decimal TotalEstimatedHours { get; set; }
    public decimal TotalReturnedHours { get; set; }
    public decimal TotalRecordedHours { get; set; }
    
    // By Service Area (for charts)
    public List<ServiceAreaSummary> ByServiceArea { get; set; } = new();
    
    // Recent Activity
    public List<RecentEnhancement> RecentEnhancements { get; set; } = new();
}

public class ServiceAreaSummary
{
    public string ServiceAreaId { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public int TotalCount { get; set; }
    public int NewCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal ReturnedHours { get; set; }
}

public class RecentEnhancement
{
    public string Id { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string ServiceAreaName { get; set; } = string.Empty;
    public string ServiceAreaId { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
