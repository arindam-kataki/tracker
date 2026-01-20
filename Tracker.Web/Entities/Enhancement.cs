using System.ComponentModel.DataAnnotations;

namespace Tracker.Web.Entities;

public class Enhancement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // Core fields from upload
    [Required]
    public string WorkId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    
    // Service Area
    public string ServiceAreaId { get; set; } = string.Empty;
    
    // Estimation fields
    public decimal? EstimatedHours { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public string? EstimationNotes { get; set; }
    public string? Status { get; set; }
    public string? ServiceLine { get; set; }
    
    // Returned/Actual fields
    public decimal? ReturnedHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? InfStatus { get; set; }
    public string? InfServiceLine { get; set; }
    
    // Weekly time allocations
    public decimal? TimeW1 { get; set; }
    public decimal? TimeW2 { get; set; }
    public decimal? TimeW3 { get; set; }
    public decimal? TimeW4 { get; set; }
    public decimal? TimeW5 { get; set; }
    public decimal? TimeW6 { get; set; }
    public decimal? TimeW7 { get; set; }
    public decimal? TimeW8 { get; set; }
    public decimal? TimeW9 { get; set; }
    
    // Audit fields
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
    
    // Concurrency
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // Navigation
    public virtual ServiceArea ServiceArea { get; set; } = null!;
    public virtual EstimationBreakdown? EstimationBreakdown { get; set; }
    public virtual ICollection<EnhancementContact> Contacts { get; set; } = new List<EnhancementContact>();
    public virtual ICollection<EnhancementResource> Resources { get; set; } = new List<EnhancementResource>();
}
