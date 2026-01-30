namespace Tracker.Web.ViewModels;

/// <summary>
/// Request model for sharing an enhancement to another service area
/// </summary>
public class ShareEnhancementRequest
{
    public string EnhancementId { get; set; } = string.Empty;
    public string TargetServiceAreaId { get; set; } = string.Empty;
    
    /// <summary>
    /// Required note explaining why this work item is being shared.
    /// This will be added as the latest note in the shared copy.
    /// </summary>
    public string SharingNote { get; set; } = string.Empty;
}
