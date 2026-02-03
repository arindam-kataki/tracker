namespace Tracker.Web.Entities;

public class EnhancementResource
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EnhancementId { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    
    /// <summary>
    /// The service area this resource is allocated against (SAP, ADM, MSWT, etc.)
    /// </summary>
    public string? ServiceAreaId { get; set; }
    
    /// <summary>
    /// Charge code for billing/tracking purposes
    /// </summary>
    public string? ChargeCode { get; set; }
    
    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
    public virtual Resource Resource { get; set; } = null!;
    public virtual ServiceArea? ServiceArea { get; set; }
}