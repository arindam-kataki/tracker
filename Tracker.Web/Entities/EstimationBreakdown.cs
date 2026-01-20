namespace Tracker.Web.Entities;

public class EstimationBreakdown
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EnhancementId { get; set; } = string.Empty;
    
    // Phase hours and notes
    public decimal? RequirementsAndEstimation { get; set; }
    public string? RequirementsAndEstimationNotes { get; set; }
    
    public decimal? VendorCoordination { get; set; }
    public string? VendorCoordinationNotes { get; set; }
    
    public decimal? DesignFunctionalTechnical { get; set; }
    public string? DesignFunctionalTechnicalNotes { get; set; }
    
    public decimal? TestingSTI { get; set; }
    public string? TestingSTINotes { get; set; }
    
    public decimal? TestingUAT { get; set; }
    public string? TestingUATNotes { get; set; }
    
    public decimal? GoLiveDeploymentValidation { get; set; }
    public string? GoLiveDeploymentValidationNotes { get; set; }
    
    public decimal? Hypercare { get; set; }
    public string? HypercareNotes { get; set; }
    
    public decimal? Documentation { get; set; }
    public string? DocumentationNotes { get; set; }
    
    public decimal? PMLead { get; set; }
    public string? PMLeadNotes { get; set; }
    
    public decimal? Contingency { get; set; }
    public string? ContingencyNotes { get; set; }
    
    // Computed total
    public decimal TotalHours => 
        (RequirementsAndEstimation ?? 0) +
        (VendorCoordination ?? 0) +
        (DesignFunctionalTechnical ?? 0) +
        (TestingSTI ?? 0) +
        (TestingUAT ?? 0) +
        (GoLiveDeploymentValidation ?? 0) +
        (Hypercare ?? 0) +
        (Documentation ?? 0) +
        (PMLead ?? 0) +
        (Contingency ?? 0);
    
    // Navigation
    public virtual Enhancement Enhancement { get; set; } = null!;
}
