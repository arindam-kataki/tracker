using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Tracker.Web.ViewModels.Billing;

/// <summary>
/// View model for resource rate management
/// </summary>
public class ResourceRateViewModel
{
    public string? Id { get; set; }
    
    [Required]
    public string ResourceId { get; set; } = string.Empty;
    public string? ResourceName { get; set; }
    
    [Required]
    [Display(Name = "Effective From")]
    public DateOnly EffectiveFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [Display(Name = "Effective To")]
    public DateOnly? EffectiveTo { get; set; }
    
    [Required]
    [Display(Name = "Bill Rate ($/hr)")]
    [Range(0, 10000, ErrorMessage = "Bill rate must be between 0 and 10,000")]
    public decimal BillRate { get; set; }
    
    [Required]
    [Display(Name = "Cost Rate ($/hr)")]
    [Range(0, 10000, ErrorMessage = "Cost rate must be between 0 and 10,000")]
    public decimal CostRate { get; set; }
    
    [Display(Name = "Currency")]
    public string Currency { get; set; } = "USD";
    
    [Display(Name = "Notes")]
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // === DROPDOWN OPTIONS ===
    public List<SelectListItem> Currencies { get; set; } = new();
    
    // === COMPUTED ===
    public bool IsNew => string.IsNullOrEmpty(Id);
    public string PageTitle => IsNew ? "Add Rate" : "Edit Rate";
    
    public decimal HourlyMargin => BillRate - CostRate;
    public decimal MarginPercent => BillRate > 0 
        ? Math.Round((BillRate - CostRate) / BillRate * 100, 2) 
        : 0;
    
    public void InitializeCurrencies()
    {
        Currencies = new List<SelectListItem>
        {
            new("USD - US Dollar", "USD"),
            new("EUR - Euro", "EUR"),
            new("GBP - British Pound", "GBP"),
            new("INR - Indian Rupee", "INR"),
            new("CAD - Canadian Dollar", "CAD"),
            new("AUD - Australian Dollar", "AUD"),
            new("JPY - Japanese Yen", "JPY"),
        };
    }
}

/// <summary>
/// View model for client-specific rate
/// </summary>
public class ClientRateViewModel
{
    public string? Id { get; set; }
    
    [Required]
    public string ResourceId { get; set; } = string.Empty;
    public string? ResourceName { get; set; }
    
    [Required]
    [Display(Name = "Client")]
    public string ClientId { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    
    [Required]
    [Display(Name = "Effective From")]
    public DateOnly EffectiveFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [Display(Name = "Effective To")]
    public DateOnly? EffectiveTo { get; set; }
    
    [Required]
    [Display(Name = "Bill Rate ($/hr)")]
    [Range(0, 10000)]
    public decimal BillRate { get; set; }
    
    [Display(Name = "Currency")]
    public string Currency { get; set; } = "USD";
    
    [Display(Name = "Notes")]
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // === DROPDOWN OPTIONS ===
    public List<SelectListItem> Clients { get; set; } = new();
    public List<SelectListItem> Currencies { get; set; } = new();
    
    // === COMPUTED ===
    public bool IsNew => string.IsNullOrEmpty(Id);
    public string PageTitle => IsNew ? "Add Client Rate" : "Edit Client Rate";
}

/// <summary>
/// View model for service area rate card
/// </summary>
public class ServiceAreaRateViewModel
{
    public string? Id { get; set; }
    
    [Required]
    [Display(Name = "Service Area")]
    public string ServiceAreaId { get; set; } = string.Empty;
    public string? ServiceAreaName { get; set; }
    
    [Required]
    [Display(Name = "Resource Type")]
    public string ResourceTypeId { get; set; } = string.Empty;
    public string? ResourceTypeName { get; set; }
    
    [Display(Name = "Client (Optional)")]
    public string? ClientId { get; set; }
    public string? ClientName { get; set; }
    
    [Required]
    [Display(Name = "Effective From")]
    public DateOnly EffectiveFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    
    [Display(Name = "Effective To")]
    public DateOnly? EffectiveTo { get; set; }
    
    [Required]
    [Display(Name = "Bill Rate ($/hr)")]
    [Range(0, 10000)]
    public decimal BillRate { get; set; }
    
    [Display(Name = "Cost Rate ($/hr)")]
    [Range(0, 10000)]
    public decimal? CostRate { get; set; }
    
    [Display(Name = "Currency")]
    public string Currency { get; set; } = "USD";
    
    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
    
    [Display(Name = "Notes")]
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // === DROPDOWN OPTIONS ===
    public List<SelectListItem> ServiceAreas { get; set; } = new();
    public List<SelectListItem> ResourceTypes { get; set; } = new();
    public List<SelectListItem> Clients { get; set; } = new();
    public List<SelectListItem> Currencies { get; set; } = new();
    
    // === COMPUTED ===
    public bool IsNew => string.IsNullOrEmpty(Id);
    public string PageTitle => IsNew ? "Add Rate Card" : "Edit Rate Card";
    
    public decimal? HourlyMargin => CostRate.HasValue ? BillRate - CostRate.Value : null;
    public decimal? MarginPercent => CostRate.HasValue && BillRate > 0
        ? Math.Round((BillRate - CostRate.Value) / BillRate * 100, 2)
        : null;
}

/// <summary>
/// View model for rate card listing
/// </summary>
public class RateCardListViewModel
{
    public string? ServiceAreaId { get; set; }
    public string? ResourceTypeId { get; set; }
    public bool ShowInactive { get; set; }
    
    public List<RateCardListItemViewModel> RateCards { get; set; } = new();
    
    // Dropdown options
    public List<SelectListItem> ServiceAreas { get; set; } = new();
    public List<SelectListItem> ResourceTypes { get; set; } = new();
}

/// <summary>
/// List item for rate cards
/// </summary>
public class RateCardListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string ServiceAreaCode { get; set; } = string.Empty;
    public string ResourceTypeName { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    
    public decimal BillRate { get; set; }
    public decimal? CostRate { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; }
    
    // === COMPUTED ===
    
    public bool IsCurrentlyEffective
    {
        get
        {
            if (!IsActive) return false;
            var today = DateOnly.FromDateTime(DateTime.Today);
            return EffectiveFrom <= today && (!EffectiveTo.HasValue || EffectiveTo >= today);
        }
    }
    
    public string DisplayName
    {
        get
        {
            var name = $"{ServiceAreaCode} - {ResourceTypeName}";
            if (!string.IsNullOrEmpty(ClientName))
                name += $" ({ClientName})";
            return name;
        }
    }
    
    public string EffectivePeriodDisplay => EffectiveTo.HasValue
        ? $"{EffectiveFrom:MMM d, yyyy} - {EffectiveTo:MMM d, yyyy}"
        : $"{EffectiveFrom:MMM d, yyyy} - Present";
    
    public string RateDisplay => $"{Currency} {BillRate:N2}/hr";
    
    public decimal? MarginPercent => CostRate.HasValue && BillRate > 0
        ? Math.Round((BillRate - CostRate.Value) / BillRate * 100, 1)
        : null;
}
