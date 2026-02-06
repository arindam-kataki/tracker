using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

/// <summary>
/// Service for generating the Effort Matrix — resource-level monthly contribution breakdown.
/// </summary>
public interface IEffortMatrixService
{
    /// <summary>
    /// Get available years that have timesheet entries.
    /// </summary>
    Task<List<int>> GetAvailableYearsAsync(List<string>? serviceAreaIds = null);

    /// <summary>
    /// Generate effort matrix rows for a given year.
    /// Each row is a unique (SignIT, Resource, ServiceArea, ChargeCode) combination
    /// with 12 monthly contributed hours columns.
    /// </summary>
    Task<List<EffortMatrixRow>> GenerateMatrixAsync(int year, List<string>? serviceAreaIds = null);
}
