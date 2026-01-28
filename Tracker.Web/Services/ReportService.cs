using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

public class ReportService : IReportService
{
    private readonly TrackerDbContext _context;

    public ReportService(TrackerDbContext context)
    {
        _context = context;
    }

    public async Task<List<NamedReport>> GetUserReportsAsync(string userId)
    {
        return await _context.Set<NamedReport>()
            .Where(r => r.ResourceId == userId)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<NamedReport>> GetAccessibleReportsAsync(string userId, bool isSuperAdmin, List<string> accessibleServiceAreaIds)
    {
        var reports = await _context.Set<NamedReport>()
            .Where(r => r.ResourceId == userId)
            .OrderBy(r => r.Name)
            .ToListAsync();

        if (isSuperAdmin)
        {
            return reports;
        }

        // Filter reports to only those where user has access to ALL service areas in the report
        return reports.Where(r =>
        {
            var reportServiceAreaIds = DeserializeServiceAreaIds(r.ServiceAreaIdsJson);
            return reportServiceAreaIds.All(saId => accessibleServiceAreaIds.Contains(saId));
        }).ToList();
    }

    public async Task<NamedReport?> GetByIdAsync(string id)
    {
        return await _context.Set<NamedReport>()
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<NamedReport> CreateReportAsync(string userId, NamedReportEditViewModel model)
    {
        var report = new NamedReport
        {
            ResourceId = userId,
            Name = model.Name,
            Description = model.Description,
            ServiceAreaIdsJson = JsonSerializer.Serialize(model.SelectedServiceAreaIds ?? new List<string>()),
            FilterJson = model.Filter?.ToJson() ?? "{}",
            ColumnsJson = JsonSerializer.Serialize(model.SelectedColumns ?? new List<string>()),
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<NamedReport>().Add(report);
        await _context.SaveChangesAsync();

        return report;
    }

    public async Task<NamedReport> UpdateReportAsync(string id, NamedReportEditViewModel model)
    {
        var report = await GetByIdAsync(id);
        if (report == null)
            throw new InvalidOperationException("Report not found");

        report.Name = model.Name;
        report.Description = model.Description;
        report.ServiceAreaIdsJson = JsonSerializer.Serialize(model.SelectedServiceAreaIds ?? new List<string>());
        report.FilterJson = model.Filter?.ToJson() ?? "{}";
        report.ColumnsJson = JsonSerializer.Serialize(model.SelectedColumns ?? new List<string>());
        report.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return report;
    }

    public async Task<bool> DeleteReportAsync(string id, string userId)
    {
        var report = await GetByIdAsync(id);
        if (report == null || report.ResourceId != userId)
            return false;

        _context.Set<NamedReport>().Remove(report);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ReportResultViewModel> RunReportAsync(
        string reportId, 
        string userId, 
        bool isSuperAdmin, 
        List<string> accessibleServiceAreaIds,
        ReportRunOptions? options = null)
    {
        var report = await GetByIdAsync(reportId);
        if (report == null)
            throw new InvalidOperationException("Report not found");

        options ??= new ReportRunOptions();

        // Parse report configuration
        var reportServiceAreaIds = DeserializeServiceAreaIds(report.ServiceAreaIdsJson);
        var filter = EnhancementFilterViewModel.FromJson(report.FilterJson);
        var columns = DeserializeColumns(report.ColumnsJson);

        // Filter service areas to only those user can access
        var effectiveServiceAreaIds = isSuperAdmin 
            ? reportServiceAreaIds 
            : reportServiceAreaIds.Where(saId => accessibleServiceAreaIds.Contains(saId)).ToList();

        if (!effectiveServiceAreaIds.Any())
        {
            return new ReportResultViewModel
            {
                ReportId = reportId,
                ReportName = report.Name,
                Columns = columns,
                Items = new List<EnhancementExportRow>(),
                TotalCount = 0,
                Page = options.Page,
                PageSize = options.PageSize
            };
        }

        // Build query
        var query = _context.Enhancements
            .Include(e => e.ServiceArea)
            .Include(e => e.Sponsors).ThenInclude(s => s.Resource)
            .Include(e => e.Spocs).ThenInclude(s => s.Resource)
            .Include(e => e.Resources).ThenInclude(r => r.Resource)
            .Where(e => effectiveServiceAreaIds.Contains(e.ServiceAreaId))
            .AsQueryable();

        // Apply filters
        query = ApplyFilters(query, filter);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = ApplySorting(query, options.SortColumn ?? "workIdDescription", options.SortOrder ?? "asc");

        // Apply paging
        var items = await query
            .Skip((options.Page - 1) * options.PageSize)
            .Take(options.PageSize)
            .ToListAsync();

        // Map to export rows
        var exportRows = items.Select(e => new EnhancementExportRow
        {
            Id = e.Id,
            WorkId = e.WorkId,
            Description = e.Description,
            Notes = e.Notes,
            ServiceAreaName = e.ServiceArea?.Name ?? "",
            EstimatedHours = e.EstimatedHours,
            EstimatedStartDate = e.EstimatedStartDate,
            EstimatedEndDate = e.EstimatedEndDate,
            EstimationNotes = e.EstimationNotes,
            Status = e.Status,
            ServiceLine = e.ServiceLine,
            ReturnedHours = e.ReturnedHours,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            InfStatus = e.InfStatus,
            InfServiceLine = e.InfServiceLine,
            TimeW1 = e.TimeW1,
            TimeW2 = e.TimeW2,
            TimeW3 = e.TimeW3,
            TimeW4 = e.TimeW4,
            TimeW5 = e.TimeW5,
            TimeW6 = e.TimeW6,
            TimeW7 = e.TimeW7,
            TimeW8 = e.TimeW8,
            TimeW9 = e.TimeW9,
            Sponsors = string.Join(", ", e.Sponsors.Select(s => s.Resource?.Name).Where(n => n != null)),
            Spocs = string.Join(", ", e.Spocs.Select(s => s.Resource?.Name).Where(n => n != null)),
            Resources = string.Join(", ", e.Resources.Select(r => r.Resource?.Name).Where(n => n != null)),
            CreatedBy = e.CreatedBy,
            CreatedAt = e.CreatedAt,
            ModifiedBy = e.ModifiedBy,
            ModifiedAt = e.ModifiedAt
        }).ToList();

        return new ReportResultViewModel
        {
            ReportId = reportId,
            ReportName = report.Name,
            Description = report.Description,
            Columns = columns,
            Items = exportRows,
            TotalCount = totalCount,
            Page = options.Page,
            PageSize = options.PageSize,
            SortColumn = options.SortColumn ?? "workIdDescription",
            SortOrder = options.SortOrder ?? "asc",
            IncludeServiceAreaColumn = effectiveServiceAreaIds.Count > 1,
            ServiceAreaNames = await GetServiceAreaNamesAsync(effectiveServiceAreaIds)
        };
    }

    public async Task<bool> UserCanAccessReportAsync(string reportId, string userId, bool isSuperAdmin, List<string> accessibleServiceAreaIds)
    {
        var report = await GetByIdAsync(reportId);
        if (report == null)
            return false;

        // User must own the report
        if (report.ResourceId != userId)
            return false;

        if (isSuperAdmin)
            return true;

        // User must have access to all service areas in the report
        var reportServiceAreaIds = DeserializeServiceAreaIds(report.ServiceAreaIdsJson);
        return reportServiceAreaIds.All(saId => accessibleServiceAreaIds.Contains(saId));
    }

    // Helper methods
    private List<string> DeserializeServiceAreaIds(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private List<string> DeserializeColumns(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    private async Task<Dictionary<string, string>> GetServiceAreaNamesAsync(List<string> serviceAreaIds)
    {
        return await _context.ServiceAreas
            .Where(sa => serviceAreaIds.Contains(sa.Id))
            .ToDictionaryAsync(sa => sa.Id, sa => sa.Name);
    }

    private IQueryable<Enhancement> ApplyFilters(IQueryable<Enhancement> query, EnhancementFilterViewModel filter)
    {
        // Search
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var searchLower = filter.Search.ToLower();
            query = query.Where(e =>
                e.WorkId.ToLower().Contains(searchLower) ||
                e.Description.ToLower().Contains(searchLower) ||
                (e.Notes != null && e.Notes.ToLower().Contains(searchLower)));
        }

        // Status filter
        if (filter.Statuses != null && filter.Statuses.Any())
        {
            query = query.Where(e => e.Status != null && filter.Statuses.Contains(e.Status));
        }

        // InfStatus filter
        if (filter.InfStatuses != null && filter.InfStatuses.Any())
        {
            query = query.Where(e => e.InfStatus != null && filter.InfStatuses.Contains(e.InfStatus));
        }

        // ServiceLine filter
        if (filter.ServiceLines != null && filter.ServiceLines.Any())
        {
            query = query.Where(e => e.ServiceLine != null && filter.ServiceLines.Contains(e.ServiceLine));
        }

        // Estimated Hours range
        if (filter.EstimatedHoursMin.HasValue)
        {
            query = query.Where(e => e.EstimatedHours >= filter.EstimatedHoursMin.Value);
        }
        if (filter.EstimatedHoursMax.HasValue)
        {
            query = query.Where(e => e.EstimatedHours <= filter.EstimatedHoursMax.Value);
        }

        // Returned Hours range
        if (filter.ReturnedHoursMin.HasValue)
        {
            query = query.Where(e => e.ReturnedHours >= filter.ReturnedHoursMin.Value);
        }
        if (filter.ReturnedHoursMax.HasValue)
        {
            query = query.Where(e => e.ReturnedHours <= filter.ReturnedHoursMax.Value);
        }

        // Date ranges
        if (filter.EstimatedStartDateFrom.HasValue)
        {
            query = query.Where(e => e.EstimatedStartDate >= filter.EstimatedStartDateFrom.Value);
        }
        if (filter.EstimatedStartDateTo.HasValue)
        {
            query = query.Where(e => e.EstimatedStartDate <= filter.EstimatedStartDateTo.Value);
        }
        if (filter.EstimatedEndDateFrom.HasValue)
        {
            query = query.Where(e => e.EstimatedEndDate >= filter.EstimatedEndDateFrom.Value);
        }
        if (filter.EstimatedEndDateTo.HasValue)
        {
            query = query.Where(e => e.EstimatedEndDate <= filter.EstimatedEndDateTo.Value);
        }
        if (filter.StartDateFrom.HasValue)
        {
            query = query.Where(e => e.StartDate >= filter.StartDateFrom.Value);
        }
        if (filter.StartDateTo.HasValue)
        {
            query = query.Where(e => e.StartDate <= filter.StartDateTo.Value);
        }
        if (filter.EndDateFrom.HasValue)
        {
            query = query.Where(e => e.EndDate >= filter.EndDateFrom.Value);
        }
        if (filter.EndDateTo.HasValue)
        {
            query = query.Where(e => e.EndDate <= filter.EndDateTo.Value);
        }
        if (filter.CreatedAtFrom.HasValue)
        {
            query = query.Where(e => e.CreatedAt >= filter.CreatedAtFrom.Value);
        }
        if (filter.CreatedAtTo.HasValue)
        {
            query = query.Where(e => e.CreatedAt <= filter.CreatedAtTo.Value);
        }
        if (filter.ModifiedAtFrom.HasValue)
        {
            query = query.Where(e => e.ModifiedAt >= filter.ModifiedAtFrom.Value);
        }
        if (filter.ModifiedAtTo.HasValue)
        {
            query = query.Where(e => e.ModifiedAt <= filter.ModifiedAtTo.Value);
        }

        return query;
    }

    private IQueryable<Enhancement> ApplySorting(IQueryable<Enhancement> query, string sortColumn, string sortOrder)
    {
        var ascending = sortOrder.ToLower() != "desc";

        return sortColumn.ToLower() switch
        {
            "workiddescription" => ascending 
                ? query.OrderBy(e => e.WorkId).ThenBy(e => e.Description)
                : query.OrderByDescending(e => e.WorkId).ThenByDescending(e => e.Description),
            "status" => ascending ? query.OrderBy(e => e.Status) : query.OrderByDescending(e => e.Status),
            "estimatedhours" => ascending ? query.OrderBy(e => e.EstimatedHours) : query.OrderByDescending(e => e.EstimatedHours),
            "estimatedstartdate" => ascending ? query.OrderBy(e => e.EstimatedStartDate) : query.OrderByDescending(e => e.EstimatedStartDate),
            "estimatedenddate" => ascending ? query.OrderBy(e => e.EstimatedEndDate) : query.OrderByDescending(e => e.EstimatedEndDate),
            "returnedhours" => ascending ? query.OrderBy(e => e.ReturnedHours) : query.OrderByDescending(e => e.ReturnedHours),
            "startdate" => ascending ? query.OrderBy(e => e.StartDate) : query.OrderByDescending(e => e.StartDate),
            "enddate" => ascending ? query.OrderBy(e => e.EndDate) : query.OrderByDescending(e => e.EndDate),
            "infstatus" => ascending ? query.OrderBy(e => e.InfStatus) : query.OrderByDescending(e => e.InfStatus),
            "serviceline" => ascending ? query.OrderBy(e => e.ServiceLine) : query.OrderByDescending(e => e.ServiceLine),
            "createdat" => ascending ? query.OrderBy(e => e.CreatedAt) : query.OrderByDescending(e => e.CreatedAt),
            "modifiedat" => ascending ? query.OrderBy(e => e.ModifiedAt) : query.OrderByDescending(e => e.ModifiedAt),
            _ => ascending ? query.OrderBy(e => e.WorkId) : query.OrderByDescending(e => e.WorkId)
        };
    }
}
