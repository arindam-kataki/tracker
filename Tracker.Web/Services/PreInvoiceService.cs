using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

/// <summary>
/// Service for generating Pre Invoice reports.
/// Aggregates timesheet data grouped by SignIT, Service Area, Charge Code, and WorkId.
/// </summary>
public class PreInvoiceService : IPreInvoiceService
{
    private readonly TrackerDbContext _db;
    private readonly ILogger<PreInvoiceService> _logger;

    public PreInvoiceService(TrackerDbContext db, ILogger<PreInvoiceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DateOnly?> GetEarliestTimesheetMonthAsync(List<string>? serviceAreaIds = null)
    {
        var query = _db.TimeEntries
            .Include(te => te.Enhancement)
            .AsQueryable();

        if (serviceAreaIds != null && serviceAreaIds.Any())
        {
            query = query.Where(te => serviceAreaIds.Contains(te.Enhancement.ServiceAreaId));
        }

        var earliest = await query
            .OrderBy(te => te.StartDate)
            .Select(te => te.StartDate)
            .FirstOrDefaultAsync();

        return earliest == default ? null : earliest;
    }

    public async Task<List<MonthOption>> GetAvailableMonthsAsync(List<string>? serviceAreaIds = null)
    {
        var earliest = await GetEarliestTimesheetMonthAsync(serviceAreaIds);
        if (earliest == null)
            return new List<MonthOption>();

        var months = new List<MonthOption>();
        var current = new DateOnly(earliest.Value.Year, earliest.Value.Month, 1);
        var now = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);

        while (current <= now)
        {
            months.Add(new MonthOption
            {
                Value = current.ToString("yyyy-MM"),
                Display = current.ToString("MMM yyyy")
            });
            current = current.AddMonths(1);
        }

        // Reverse so most recent is first
        months.Reverse();
        return months;
    }

    public async Task<List<PreInvoiceGroup>> GeneratePreInvoiceAsync(
        int year, int month, List<string>? serviceAreaIds = null)
    {
        var monthStart = new DateOnly(year, month, 1);
        var monthEnd = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        _logger.LogInformation(
            "Generating pre-invoice for {Month}/{Year}, service areas: {ServiceAreas}",
            month, year, serviceAreaIds != null ? string.Join(",", serviceAreaIds) : "all");

        // ---------------------------------------------------------------
        // Step 1: Get all enhancements that have ANY time entries up to the end of the selected month
        // ---------------------------------------------------------------
        var enhancementsWithTimeQuery = _db.TimeEntries
            .Include(te => te.Enhancement)
                .ThenInclude(e => e.ServiceArea)
            .Where(te => te.StartDate <= monthEnd);

        if (serviceAreaIds != null && serviceAreaIds.Any())
        {
            enhancementsWithTimeQuery = enhancementsWithTimeQuery
                .Where(te => serviceAreaIds.Contains(te.Enhancement.ServiceAreaId));
        }

        // Get distinct enhancement IDs that have time entries
        var enhancementIds = await enhancementsWithTimeQuery
            .Select(te => te.EnhancementId)
            .Distinct()
            .ToListAsync();

        if (!enhancementIds.Any())
            return new List<PreInvoiceGroup>();

        // ---------------------------------------------------------------
        // Step 2: Load enhancements with their service areas
        // ---------------------------------------------------------------
        var enhancements = await _db.Enhancements
            .Include(e => e.ServiceArea)
            .Where(e => enhancementIds.Contains(e.Id))
            .ToListAsync();

        // ---------------------------------------------------------------
        // Step 3: Load all time entries up to end of selected month for these enhancements
        // ---------------------------------------------------------------
        var allEntries = await _db.TimeEntries
            .Where(te => enhancementIds.Contains(te.EnhancementId))
            .Where(te => te.StartDate <= monthEnd)
            .ToListAsync();

        // ---------------------------------------------------------------
        // Step 4: Load charge codes from EnhancementResources
        // (join: Enhancement + ServiceArea → ChargeCode)
        // Also get charge codes from TimeEntry.ChargeCode if populated
        // ---------------------------------------------------------------
        var enhancementResources = await _db.Set<Entities.EnhancementResource>()
            .Where(er => enhancementIds.Contains(er.EnhancementId))
            .ToListAsync();

        // ---------------------------------------------------------------
        // Step 5: Build the pre-invoice items
        // Group by (SignIT, ServiceArea, ChargeCode, WorkId)
        // ---------------------------------------------------------------
        var items = new List<PreInvoiceItem>();

        foreach (var enhancement in enhancements)
        {
            var enhEntries = allEntries.Where(te => te.EnhancementId == enhancement.Id).ToList();

            // Determine charge codes for this enhancement
            // Priority: TimeEntry.ChargeCode > EnhancementResource.ChargeCode
            // Group entries by their charge code
            var entriesByChargeCode = new Dictionary<string, List<Entities.TimeEntry>>();

            foreach (var entry in enhEntries)
            {
                // Use the charge code from the time entry if available
                var chargeCode = entry.ChargeCode;

                // If not on the time entry, look it up from EnhancementResource
                if (string.IsNullOrEmpty(chargeCode))
                {
                    // Find the best matching EnhancementResource charge code
                    // First try: match by enhancement + service area
                    var er = enhancementResources
                        .Where(r => r.EnhancementId == enhancement.Id
                                    && r.ServiceAreaId == enhancement.ServiceAreaId
                                    && !string.IsNullOrEmpty(r.ChargeCode))
                        .FirstOrDefault();

                    // Fallback: any charge code for this enhancement
                    er ??= enhancementResources
                        .Where(r => r.EnhancementId == enhancement.Id
                                    && !string.IsNullOrEmpty(r.ChargeCode))
                        .FirstOrDefault();

                    chargeCode = er?.ChargeCode ?? string.Empty;
                }

                var key = chargeCode ?? string.Empty;
                if (!entriesByChargeCode.ContainsKey(key))
                    entriesByChargeCode[key] = new List<Entities.TimeEntry>();

                entriesByChargeCode[key].Add(entry);
            }

            // If no entries had charge codes, still create one group with empty charge code
            if (!entriesByChargeCode.Any())
            {
                // Find charge code from EnhancementResource
                var er = enhancementResources
                    .Where(r => r.EnhancementId == enhancement.Id && !string.IsNullOrEmpty(r.ChargeCode))
                    .FirstOrDefault();

                entriesByChargeCode[er?.ChargeCode ?? string.Empty] = enhEntries;
            }

            foreach (var (chargeCode, entries) in entriesByChargeCode)
            {
                // Hours consumed to last month (all entries with StartDate before the selected month)
                var hoursToLastMonth = entries
                    .Where(te => te.StartDate < monthStart)
                    .Sum(te => te.ContributedHours);

                // Hours consumed this month
                var hoursThisMonth = entries
                    .Where(te => te.StartDate >= monthStart && te.StartDate <= monthEnd)
                    .Sum(te => te.ContributedHours);

                // Hours consumed to date (up to end of selected month)
                var hoursToDate = hoursToLastMonth + hoursThisMonth;

                // Approved effort (ReturnedHours = signed/approved hours)
                var approvedEffort = enhancement.ReturnedHours ?? 0;

                // Effort remaining
                var effortRemaining = approvedEffort - hoursToDate;

                items.Add(new PreInvoiceItem
                {
                    EnhancementId = enhancement.Id,
                    SignITReference = enhancement.SignITReference ?? "(No SignIT)",
                    ProjectTitle = enhancement.Description,
                    WorkId = enhancement.WorkId,
                    ApprovedEffort = approvedEffort,
                    ServiceAreaCode = enhancement.ServiceArea?.Code ?? "N/A",
                    ServiceAreaName = enhancement.ServiceArea?.Name ?? "N/A",
                    ChargeCode = chargeCode,
                    HoursConsumedToLastMonth = hoursToLastMonth,
                    HoursConsumedThisMonth = hoursThisMonth,
                    HoursConsumedToDate = hoursToDate,
                    EffortRemaining = effortRemaining,
                    Status = enhancement.InfStatus ?? enhancement.Status ?? "N/A"
                });
            }
        }

        // ---------------------------------------------------------------
        // Step 6: Group by SignIT reference
        // ---------------------------------------------------------------
        var groups = items
            .OrderBy(i => i.SignITReference)
            .ThenBy(i => i.ServiceAreaCode)
            .ThenBy(i => i.ChargeCode)
            .ThenBy(i => i.WorkId)
            .GroupBy(i => i.SignITReference)
            .Select(g => new PreInvoiceGroup
            {
                SignITReference = g.Key,
                Items = g.ToList()
            })
            .ToList();

        _logger.LogInformation(
            "Pre-invoice generated: {GroupCount} SignIT groups, {ItemCount} line items",
            groups.Count, items.Count);

        return groups;
    }
}
