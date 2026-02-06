using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

/// <summary>
/// Service for generating the Effort Matrix report.
/// Aggregates timesheet ContributedHours by (SignIT, Resource, ServiceArea, ChargeCode) × 12 months.
/// </summary>
public class EffortMatrixService : IEffortMatrixService
{
    private readonly TrackerDbContext _db;
    private readonly ILogger<EffortMatrixService> _logger;

    public EffortMatrixService(TrackerDbContext db, ILogger<EffortMatrixService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<int>> GetAvailableYearsAsync(List<string>? serviceAreaIds = null)
    {
        var query = _db.TimeEntries
            .Include(te => te.Enhancement)
            .AsQueryable();

        if (serviceAreaIds != null && serviceAreaIds.Any())
            query = query.Where(te => serviceAreaIds.Contains(te.Enhancement.ServiceAreaId));

        // Get distinct years from StartDate
        var years = await query
            .Select(te => te.StartDate.Year)
            .Distinct()
            .OrderByDescending(y => y)
            .ToListAsync();

        return years;
    }

    public async Task<List<EffortMatrixRow>> GenerateMatrixAsync(int year, List<string>? serviceAreaIds = null)
    {
        var yearStart = new DateOnly(year, 1, 1);
        var yearEnd = new DateOnly(year, 12, 31);

        _logger.LogInformation("Generating effort matrix for {Year}, service areas: {ServiceAreas}",
            year, serviceAreaIds != null ? string.Join(",", serviceAreaIds) : "all");

        // ---------------------------------------------------------------
        // Step 1: Load all time entries for the year
        // ---------------------------------------------------------------
        var entriesQuery = _db.TimeEntries
            .Include(te => te.Enhancement)
                .ThenInclude(e => e.ServiceArea)
            .Include(te => te.Resource)
            .Where(te => te.StartDate >= yearStart && te.StartDate <= yearEnd);

        if (serviceAreaIds != null && serviceAreaIds.Any())
            entriesQuery = entriesQuery.Where(te => serviceAreaIds.Contains(te.Enhancement.ServiceAreaId));

        var entries = await entriesQuery.ToListAsync();

        if (!entries.Any())
            return new List<EffortMatrixRow>();

        // ---------------------------------------------------------------
        // Step 2: Load charge codes from EnhancementResources for fallback
        // ---------------------------------------------------------------
        var enhancementIds = entries.Select(te => te.EnhancementId).Distinct().ToList();

        var enhancementResources = await _db.Set<EnhancementResource>()
            .Where(er => enhancementIds.Contains(er.EnhancementId))
            .ToListAsync();

        // ---------------------------------------------------------------
        // Step 3: Build rows grouped by (SignIT, Resource, ServiceArea, ChargeCode)
        // ---------------------------------------------------------------
        var rowDict = new Dictionary<string, EffortMatrixRow>();

        foreach (var entry in entries)
        {
            var enhancement = entry.Enhancement;
            var signIt = enhancement.SignITReference ?? "(No SignIT)";
            var resourceName = entry.Resource?.Name ?? "Unknown";
            var saCode = enhancement.ServiceArea?.Code ?? "N/A";

            // Resolve charge code: TimeEntry.ChargeCode > EnhancementResource fallback
            var chargeCode = entry.ChargeCode;
            if (string.IsNullOrEmpty(chargeCode))
            {
                var er = enhancementResources
                    .Where(r => r.EnhancementId == enhancement.Id
                                && r.ResourceId == entry.ResourceId
                                && !string.IsNullOrEmpty(r.ChargeCode))
                    .FirstOrDefault();

                er ??= enhancementResources
                    .Where(r => r.EnhancementId == enhancement.Id
                                && r.ServiceAreaId == enhancement.ServiceAreaId
                                && !string.IsNullOrEmpty(r.ChargeCode))
                    .FirstOrDefault();

                chargeCode = er?.ChargeCode ?? string.Empty;
            }

            // Build composite key
            var key = $"{signIt}|{entry.ResourceId}|{saCode}|{chargeCode}";

            if (!rowDict.TryGetValue(key, out var row))
            {
                row = new EffortMatrixRow
                {
                    EnhancementId = enhancement.Id,
                    SignITReference = signIt,
                    ProjectTitle = enhancement.Description,
                    ResourceId = entry.ResourceId,
                    ResourceName = resourceName,
                    ServiceAreaCode = saCode,
                    ChargeCode = chargeCode
                };
                rowDict[key] = row;
            }

            // Add contributed hours to the correct month bucket (0-based: Jan=0)
            var monthIndex = entry.StartDate.Month - 1;
            row.MonthlyHours[monthIndex] += entry.ContributedHours;
        }

        // ---------------------------------------------------------------
        // Step 4: Sort and return
        // ---------------------------------------------------------------
        var result = rowDict.Values
            .OrderBy(r => r.SignITReference)
            .ThenBy(r => r.ResourceName)
            .ThenBy(r => r.ServiceAreaCode)
            .ThenBy(r => r.ChargeCode)
            .ToList();

        _logger.LogInformation("Effort matrix generated: {RowCount} rows for {Year}", result.Count, year);

        return result;
    }
}
