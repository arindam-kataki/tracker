using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

public interface IExcelExportService
{
    /// <summary>
    /// Exports enhancements to an Excel file (.xlsx) using OpenXML.
    /// </summary>
    /// <param name="enhancements">The enhancement data to export.</param>
    /// <param name="columns">The column keys to include in the export, in order.</param>
    /// <param name="includeServiceAreaColumn">Whether to add a Service Area column (for cross-area reports).</param>
    /// <returns>Byte array of the Excel file.</returns>
    byte[] ExportToExcel(List<EnhancementExportRow> enhancements, List<string> columns, bool includeServiceAreaColumn = false);
    
    /// <summary>
    /// Gets suggested filename for the export.
    /// </summary>
    string GetExportFilename(string reportName);
}

/// <summary>
/// Flattened enhancement data for export.
/// </summary>
public class EnhancementExportRow
{
    public string Id { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string ServiceAreaName { get; set; } = string.Empty;
    public decimal? EstimatedHours { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public string? EstimationNotes { get; set; }
    public string? Status { get; set; }
    public string? ServiceLine { get; set; }
    public decimal? ReturnedHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? InfStatus { get; set; }
    public string? InfServiceLine { get; set; }
    public decimal? TimeW1 { get; set; }
    public decimal? TimeW2 { get; set; }
    public decimal? TimeW3 { get; set; }
    public decimal? TimeW4 { get; set; }
    public decimal? TimeW5 { get; set; }
    public decimal? TimeW6 { get; set; }
    public decimal? TimeW7 { get; set; }
    public decimal? TimeW8 { get; set; }
    public decimal? TimeW9 { get; set; }
    
    // CSV strings for resources
    public string Sponsors { get; set; } = string.Empty;
    public string Spocs { get; set; } = string.Empty;
    public string Resources { get; set; } = string.Empty;
    
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}
