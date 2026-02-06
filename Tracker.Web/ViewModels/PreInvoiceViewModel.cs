using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;

/// <summary>
/// ViewModel for the Pre Invoice page.
/// </summary>
public class PreInvoiceViewModel
{
    /// <summary>
    /// Selected month in "yyyy-MM" format
    /// </summary>
    public string? SelectedMonth { get; set; }

    /// <summary>
    /// Available months for dropdown (from earliest timesheet entry to current month)
    /// </summary>
    public List<MonthOption> AvailableMonths { get; set; } = new();

    /// <summary>
    /// Grouped pre-invoice data rows
    /// </summary>
    public List<PreInvoiceGroup> Groups { get; set; } = new();

    /// <summary>
    /// Summary totals
    /// </summary>
    public decimal TotalApprovedEffort => Groups.Sum(g => g.Items.Sum(i => i.ApprovedEffort));
    public decimal TotalHoursToLastMonth => Groups.Sum(g => g.Items.Sum(i => i.HoursConsumedToLastMonth));
    public decimal TotalHoursThisMonth => Groups.Sum(g => g.Items.Sum(i => i.HoursConsumedThisMonth));
    public decimal TotalHoursToDate => Groups.Sum(g => g.Items.Sum(i => i.HoursConsumedToDate));
    public decimal TotalEffortRemaining => Groups.Sum(g => g.Items.Sum(i => i.EffortRemaining));
}

/// <summary>
/// A month option for the dropdown
/// </summary>
public class MonthOption
{
    public string Value { get; set; } = string.Empty; // "yyyy-MM"
    public string Display { get; set; } = string.Empty; // "Jan 2025"
}

/// <summary>
/// A group in the pre-invoice report (grouped by SignIT)
/// </summary>
public class PreInvoiceGroup
{
    public string SignITReference { get; set; } = string.Empty;
    public List<PreInvoiceItem> Items { get; set; } = new();

    public decimal GroupApprovedEffort => Items.Sum(i => i.ApprovedEffort);
    public decimal GroupHoursToLastMonth => Items.Sum(i => i.HoursConsumedToLastMonth);
    public decimal GroupHoursThisMonth => Items.Sum(i => i.HoursConsumedThisMonth);
    public decimal GroupHoursToDate => Items.Sum(i => i.HoursConsumedToDate);
    public decimal GroupEffortRemaining => Items.Sum(i => i.EffortRemaining);
}

/// <summary>
/// A single row in the pre-invoice report
/// </summary>
public class PreInvoiceItem
{
    public string EnhancementId { get; set; } = string.Empty;
    public string SignITReference { get; set; } = string.Empty;
    public string ProjectTitle { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public decimal ApprovedEffort { get; set; }
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ServiceAreaName { get; set; } = string.Empty;
    public string ChargeCode { get; set; } = string.Empty;
    public decimal HoursConsumedToLastMonth { get; set; }
    public decimal HoursConsumedThisMonth { get; set; }
    public decimal HoursConsumedToDate { get; set; }
    public decimal EffortRemaining { get; set; }
    public string Status { get; set; } = string.Empty;
}
