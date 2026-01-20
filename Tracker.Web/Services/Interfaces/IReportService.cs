using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

public interface IReportService
{
    // Named Report CRUD
    Task<List<NamedReport>> GetUserReportsAsync(string userId);
    Task<List<NamedReport>> GetAccessibleReportsAsync(string userId, bool isSuperAdmin, List<string> accessibleServiceAreaIds);
    Task<NamedReport?> GetByIdAsync(string id);
    Task<NamedReport> CreateReportAsync(string userId, NamedReportEditViewModel model);
    Task<NamedReport> UpdateReportAsync(string id, NamedReportEditViewModel model);
    Task<bool> DeleteReportAsync(string id, string userId);
    
    // Report Execution
    Task<ReportResultViewModel> RunReportAsync(string reportId, string userId, bool isSuperAdmin, List<string> accessibleServiceAreaIds, ReportRunOptions? options = null);
    
    // Helpers
    Task<bool> UserCanAccessReportAsync(string reportId, string userId, bool isSuperAdmin, List<string> accessibleServiceAreaIds);
}

public class ReportRunOptions
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SortColumn { get; set; }
    public string? SortOrder { get; set; }
}
