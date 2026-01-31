using Tracker.Web.Entities.Billing;
using Tracker.Web.ViewModels.Billing;

namespace Tracker.Web.Services.Billing;

/// <summary>
/// Service for managing billing rates and resolving applicable rates
/// </summary>
public interface IRateService
{
    #region Rate Resolution
    
    /// <summary>
    /// Get the applicable bill rate for a resource on a specific date.
    /// Uses rate hierarchy: ClientRate > ResourceRate > ServiceAreaRate > Default
    /// </summary>
    Task<RateResolutionResult> GetBillRateAsync(
        string resourceId, 
        DateOnly date, 
        string? clientId = null,
        string? serviceAreaId = null);
    
    /// <summary>
    /// Get the cost rate for a resource on a specific date
    /// </summary>
    Task<decimal> GetCostRateAsync(string resourceId, DateOnly date);
    
    /// <summary>
    /// Calculate cost for time entries (handles overtime for non-exempt)
    /// </summary>
    Task<CostResultViewModel> CalculateCostAsync(
        string resourceId,
        DateOnly weekStartDate,
        decimal totalHours);
    
    #endregion
    
    #region Resource Rates
    
    /// <summary>
    /// Get rate history for a resource
    /// </summary>
    Task<List<ResourceRate>> GetResourceRatesAsync(string resourceId);
    
    /// <summary>
    /// Get the current/active rate for a resource
    /// </summary>
    Task<ResourceRate?> GetCurrentResourceRateAsync(string resourceId);
    
    /// <summary>
    /// Create a new resource rate
    /// </summary>
    Task<ResourceRate> CreateResourceRateAsync(ResourceRate rate);
    
    /// <summary>
    /// Update a resource rate
    /// </summary>
    Task<ResourceRate> UpdateResourceRateAsync(ResourceRate rate);
    
    /// <summary>
    /// Delete a resource rate
    /// </summary>
    Task<bool> DeleteResourceRateAsync(string id);
    
    #endregion
    
    #region Client Rates
    
    /// <summary>
    /// Get client-specific rates for a resource
    /// </summary>
    Task<List<ClientRate>> GetClientRatesAsync(string resourceId);
    
    /// <summary>
    /// Get rate for a specific resource-client combination
    /// </summary>
    Task<ClientRate?> GetClientRateAsync(string resourceId, string clientId, DateOnly date);
    
    /// <summary>
    /// Create a client-specific rate
    /// </summary>
    Task<ClientRate> CreateClientRateAsync(ClientRate rate);
    
    /// <summary>
    /// Update a client-specific rate
    /// </summary>
    Task<ClientRate> UpdateClientRateAsync(ClientRate rate);
    
    /// <summary>
    /// Delete a client-specific rate
    /// </summary>
    Task<bool> DeleteClientRateAsync(string id);
    
    #endregion
    
    #region Service Area Rate Cards
    
    /// <summary>
    /// Get rate cards for a service area
    /// </summary>
    Task<List<ServiceAreaRate>> GetServiceAreaRatesAsync(string serviceAreaId);
    
    /// <summary>
    /// Get all rate cards (with optional filters)
    /// </summary>
    Task<List<ServiceAreaRate>> GetAllRateCardsAsync(
        string? serviceAreaId = null, 
        string? resourceTypeId = null,
        bool activeOnly = true);
    
    /// <summary>
    /// Get a specific rate card
    /// </summary>
    Task<ServiceAreaRate?> GetServiceAreaRateAsync(string id);
    
    /// <summary>
    /// Create a rate card
    /// </summary>
    Task<ServiceAreaRate> CreateServiceAreaRateAsync(ServiceAreaRate rate);
    
    /// <summary>
    /// Update a rate card
    /// </summary>
    Task<ServiceAreaRate> UpdateServiceAreaRateAsync(ServiceAreaRate rate);
    
    /// <summary>
    /// Delete a rate card
    /// </summary>
    Task<bool> DeleteServiceAreaRateAsync(string id);
    
    #endregion
    
    #region ViewModels
    
    /// <summary>
    /// Get view model for rate card list
    /// </summary>
    Task<RateCardListViewModel> GetRateCardListViewModelAsync(
        string? serviceAreaId = null,
        string? resourceTypeId = null,
        bool showInactive = false);
    
    /// <summary>
    /// Get view model for editing a service area rate
    /// </summary>
    Task<ServiceAreaRateViewModel> GetServiceAreaRateViewModelAsync(string? id = null);
    
    /// <summary>
    /// Get view model for editing a resource rate
    /// </summary>
    Task<ResourceRateViewModel> GetResourceRateViewModelAsync(string resourceId, string? id = null);
    
    /// <summary>
    /// Get view model for editing a client rate
    /// </summary>
    Task<ClientRateViewModel> GetClientRateViewModelAsync(string resourceId, string? id = null);
    
    #endregion
}

/// <summary>
/// Result of rate resolution showing which rate was applied and why
/// </summary>
public class RateResolutionResult
{
    /// <summary>
    /// The resolved bill rate
    /// </summary>
    public decimal BillRate { get; set; }
    
    /// <summary>
    /// The cost rate (if available)
    /// </summary>
    public decimal? CostRate { get; set; }
    
    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "USD";
    
    /// <summary>
    /// Where this rate came from
    /// </summary>
    public RateSource Source { get; set; }
    
    /// <summary>
    /// Description of rate source (for audit/display)
    /// </summary>
    public string SourceDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the rate record used
    /// </summary>
    public string? RateId { get; set; }
    
    /// <summary>
    /// Calculated margin (if cost rate available)
    /// </summary>
    public decimal? Margin => CostRate.HasValue ? BillRate - CostRate.Value : null;
    
    /// <summary>
    /// Calculated margin percentage
    /// </summary>
    public decimal? MarginPercent => CostRate.HasValue && BillRate > 0
        ? Math.Round((BillRate - CostRate.Value) / BillRate * 100, 2)
        : null;
}

/// <summary>
/// Source of a resolved rate
/// </summary>
public enum RateSource
{
    /// <summary>
    /// Client-specific rate for this resource
    /// </summary>
    ClientRate,
    
    /// <summary>
    /// Resource's default rate
    /// </summary>
    ResourceRate,
    
    /// <summary>
    /// Service area rate card
    /// </summary>
    ServiceAreaRate,
    
    /// <summary>
    /// System default rate
    /// </summary>
    DefaultRate,
    
    /// <summary>
    /// No rate found
    /// </summary>
    None
}
