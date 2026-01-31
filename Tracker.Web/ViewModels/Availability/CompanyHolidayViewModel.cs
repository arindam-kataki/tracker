using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracker.Web.ViewModels.Availability;

/// <summary>
/// View model for creating/editing company holidays
/// </summary>
public class CompanyHolidayViewModel
{
    public string? Id { get; set; }
    
    [Required]
    [Display(Name = "Holiday Name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Display(Name = "Date")]
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [Required]
    [Display(Name = "Hours Off")]
    [Range(0.5, 24, ErrorMessage = "Hours must be between 0.5 and 24")]
    public decimal HoursOff { get; set; } = 8m;
    
    [Display(Name = "Service Area")]
    public string? ServiceAreaId { get; set; }
    
    [Display(Name = "Location")]
    [MaxLength(50)]
    public string? Location { get; set; }
    
    [Display(Name = "Repeats Annually")]
    public bool IsRecurringAnnually { get; set; } = true;
    
    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
    
    [Display(Name = "Description")]
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // === DROPDOWN OPTIONS ===
    
    public List<SelectListItem> ServiceAreas { get; set; } = new();
    public List<SelectListItem> CommonHolidays { get; set; } = new();
    
    // === COMPUTED ===
    
    public bool IsNew => string.IsNullOrEmpty(Id);
    public string PageTitle => IsNew ? "Add Holiday" : $"Edit Holiday: {Name}";
    
    public bool IsFullDay => HoursOff >= 8m;
    
    /// <summary>
    /// Initialize common holiday suggestions
    /// </summary>
    public void InitializeCommonHolidays()
    {
        CommonHolidays = new List<SelectListItem>
        {
            new("-- Select Common Holiday --", ""),
            new("New Year's Day", "New Year's Day|01-01"),
            new("Martin Luther King Jr. Day", "Martin Luther King Jr. Day|01-15"),
            new("Presidents' Day", "Presidents' Day|02-15"),
            new("Memorial Day", "Memorial Day|05-25"),
            new("Independence Day", "Independence Day|07-04"),
            new("Labor Day", "Labor Day|09-01"),
            new("Columbus Day", "Columbus Day|10-10"),
            new("Veterans Day", "Veterans Day|11-11"),
            new("Thanksgiving Day", "Thanksgiving Day|11-25"),
            new("Christmas Eve (Half Day)", "Christmas Eve|12-24|4"),
            new("Christmas Day", "Christmas Day|12-25"),
            new("New Year's Eve (Half Day)", "New Year's Eve|12-31|4"),
        };
    }
}

/// <summary>
/// View model for listing company holidays
/// </summary>
public class CompanyHolidayListViewModel
{
    public int Year { get; set; } = DateTime.Today.Year;
    public string? ServiceAreaFilter { get; set; }
    public string? LocationFilter { get; set; }
    
    public List<CompanyHolidayListItemViewModel> Holidays { get; set; } = new();
    
    // Summary
    public int TotalHolidays => Holidays.Count;
    public decimal TotalHoursOff => Holidays.Sum(h => h.HoursOff);
    public int FullDayHolidays => Holidays.Count(h => h.IsFullDay);
    public int PartialDayHolidays => Holidays.Count(h => !h.IsFullDay);
    
    // Dropdown options
    public List<SelectListItem> ServiceAreas { get; set; } = new();
    public List<SelectListItem> Locations { get; set; } = new();
    public List<SelectListItem> Years { get; set; } = new();
}

/// <summary>
/// List item for company holidays
/// </summary>
public class CompanyHolidayListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public decimal HoursOff { get; set; }
    public string? ServiceAreaCode { get; set; }
    public string? Location { get; set; }
    public bool IsRecurringAnnually { get; set; }
    public bool IsActive { get; set; }
    
    // === COMPUTED ===
    
    public bool IsFullDay => HoursOff >= 8m;
    
    public string HoursDisplay => HoursOff >= 8m 
        ? "Full Day" 
        : $"{HoursOff}h";
    
    public string ScopeDisplay
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(ServiceAreaCode)) parts.Add(ServiceAreaCode);
            if (!string.IsNullOrEmpty(Location)) parts.Add(Location);
            return parts.Any() ? string.Join(", ", parts) : "All";
        }
    }
    
    public string DateDisplay => Date.ToString("ddd, MMM d, yyyy");
    
    public string RecurrenceBadge => IsRecurringAnnually ? "Annual" : "One-time";
    public string RecurrenceBadgeClass => IsRecurringAnnually ? "bg-info" : "bg-secondary";
    
    public string StatusBadgeClass => IsActive ? "bg-success" : "bg-secondary";
    
    public bool IsPast => Date < DateOnly.FromDateTime(DateTime.Today);
    public bool IsUpcoming => Date >= DateOnly.FromDateTime(DateTime.Today) && 
                              Date <= DateOnly.FromDateTime(DateTime.Today.AddDays(30));
}
