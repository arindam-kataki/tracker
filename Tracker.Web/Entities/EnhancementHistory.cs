namespace Tracker.Web.Entities;

public class EnhancementHistory
{
    public string AuditId { get; set; } = Guid.NewGuid().ToString();
    public string AuditAction { get; set; } = string.Empty; // Insert, Update, Delete
    public DateTime AuditAt { get; set; } = DateTime.UtcNow;
    public string? AuditBy { get; set; }
    
    // Snapshot of Enhancement fields
    public string EnhancementId { get; set; } = string.Empty;
    public string WorkId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string ServiceAreaId { get; set; } = string.Empty;
    
    // Sizing - L3H fields
    public decimal? EstimatedHours { get; set; }
    public DateTime? EstimatedStartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public string? EstimationNotes { get; set; }
    public string? Status { get; set; }
    public string? ServiceLine { get; set; }
    public string? EstimatedLaborType { get; set; }      // ADD THIS
    public string? EstimatedPriority { get; set; }       // ADD THIS
    public string? EstimatedStatus { get; set; }        // ADD THIS
    
    // Actual/Core Info - Inf* fields
    public decimal? ReturnedHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? InfStatus { get; set; }
    public string? InfServiceLine { get; set; }
    public string? InfLaborType { get; set; }   // ADD THIS
    public string? InfPriority { get; set; }    // ADD THIS
    
    public string? ApprovalNotes { get; set; }
    
    // Legacy time allocations
    public decimal? TimeW1 { get; set; }
    public decimal? TimeW2 { get; set; }
    public decimal? TimeW3 { get; set; }
    public decimal? TimeW4 { get; set; }
    public decimal? TimeW5 { get; set; }
    public decimal? TimeW6 { get; set; }
    public decimal? TimeW7 { get; set; }
    public decimal? TimeW8 { get; set; }
    public decimal? TimeW9 { get; set; }
    
    // Original audit info
    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }
}