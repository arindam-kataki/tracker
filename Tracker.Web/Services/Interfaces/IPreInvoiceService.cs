using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

/// <summary>
/// Service for generating Pre Invoice reports from timesheet and enhancement data.
/// </summary>
public interface IPreInvoiceService
{
    /// <summary>
    /// Get the earliest month that has timesheet entries, across all accessible service areas.
    /// Returns null if no entries exist.
    /// </summary>
    Task<DateOnly?> GetEarliestTimesheetMonthAsync(List<string>? serviceAreaIds = null);

    /// <summary>
    /// Generate the list of available months from the earliest timesheet month to current month.
    /// </summary>
    Task<List<MonthOption>> GetAvailableMonthsAsync(List<string>? serviceAreaIds = null);

    /// <summary>
    /// Generate the pre-invoice data for a given month.
    /// Groups by SignIT → Service Area → Charge Code → WorkId.
    /// </summary>
    /// <param name="year">Year of the selected month</param>
    /// <param name="month">Month (1-12)</param>
    /// <param name="serviceAreaIds">Optional filter by service area IDs (for permission scoping)</param>
    Task<List<PreInvoiceGroup>> GeneratePreInvoiceAsync(int year, int month, List<string>? serviceAreaIds = null);
}
