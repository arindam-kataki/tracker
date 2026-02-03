using Tracker.Web.Entities;

namespace Tracker.Web.ViewModels;
/// <summary>
/// View model for a single resource allocation row on an enhancement
/// </summary>
public class ResourceAllocationViewModel
{
    public string? Id { get; set; }
    public string EnhancementId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public string? ServiceAreaId { get; set; }
    public string? ServiceAreaCode { get; set; }
    public string? ChargeCode { get; set; }
}

/// <summary>
/// Container for the resource allocations partial
/// </summary>
public class ResourceAllocationsPartialViewModel
{
    public string EnhancementId { get; set; } = string.Empty;
    public bool IsEditMode { get; set; }
    public List<ResourceAllocationViewModel> Allocations { get; set; } = new();
    public List<Resource> AvailableResources { get; set; } = new();
    public List<ServiceArea> AvailableServiceAreas { get; set; } = new();
}