namespace Tracker.Web.ViewModels;

/// <summary>
/// ViewModel for the Effort Matrix page.
/// Shows resource-level monthly contributed hours across 12 months.
/// </summary>
public class EffortMatrixViewModel
{
    /// <summary>
    /// Selected year
    /// </summary>
    public int SelectedYear { get; set; }

    /// <summary>
    /// Available years for dropdown
    /// </summary>
    public List<int> AvailableYears { get; set; } = new();

    /// <summary>
    /// Flat list of rows — one per (SignIT, Resource, ServiceArea, ChargeCode)
    /// </summary>
    public List<EffortMatrixRow> Rows { get; set; } = new();

    /// <summary>
    /// Monthly totals (index 0 = Jan, 11 = Dec)
    /// </summary>
    public decimal[] MonthlyTotals { get; set; } = new decimal[12];

    /// <summary>
    /// Grand total across all months
    /// </summary>
    public decimal GrandTotal { get; set; }
}

/// <summary>
/// A single row in the effort matrix
/// </summary>
public class EffortMatrixRow
{
    public string EnhancementId { get; set; } = string.Empty;
    public string SignITReference { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ChargeCode { get; set; } = string.Empty;

    /// <summary>
    /// Monthly contributed hours (index 0 = Jan, 11 = Dec)
    /// </summary>
    public decimal[] MonthlyHours { get; set; } = new decimal[12];

    /// <summary>
    /// Row total across all months
    /// </summary>
    public decimal RowTotal => MonthlyHours.Sum();
}
