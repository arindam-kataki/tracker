namespace Tracker.Web.Entities;

/// <summary>
/// Defines which Enhancement column a resource type maps to
/// </summary>
public enum EnhancementColumnType
{
    // Resource columns (multi-select)
    Sponsors = 0,
    SPOCs = 1,
    Resources = 2,
    
    // Text/String columns
    Status = 10,
    ServiceLine = 11,
    InfStatus = 12,
    InfServiceLine = 13,
    Notes = 14,
    EstimationNotes = 15,
    
    // Numeric columns
    EstimatedHours = 20,
    ReturnedHours = 21,
    TimeW1 = 22,
    TimeW2 = 23,
    TimeW3 = 24,
    TimeW4 = 25,
    TimeW5 = 26,
    TimeW6 = 27,
    TimeW7 = 28,
    TimeW8 = 29,
    TimeW9 = 30,
    
    // Date columns
    EstimatedStartDate = 40,
    EstimatedEndDate = 41,
    StartDate = 42,
    EndDate = 43
}

public class ResourceTypeLookup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Which Enhancement column resources of this type appear in
    /// </summary>
    public EnhancementColumnType EnhancementColumn { get; set; } = EnhancementColumnType.Resources;
    
    /// <summary>
    /// Whether multiple resources of this type are allowed on an enhancement
    /// </summary>
    public bool AllowMultiple { get; set; } = true;

    // Navigation
    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
    
    // Helper
    public string EnhancementColumnDisplay => EnhancementColumn switch
    {
        EnhancementColumnType.Sponsors => "Sponsors",
        EnhancementColumnType.SPOCs => "SPOCs",
        EnhancementColumnType.Resources => "Resources",
        EnhancementColumnType.Status => "Status",
        EnhancementColumnType.ServiceLine => "Service Line",
        EnhancementColumnType.InfStatus => "INF Status",
        EnhancementColumnType.InfServiceLine => "INF Service Line",
        EnhancementColumnType.Notes => "Notes",
        EnhancementColumnType.EstimationNotes => "Estimation Notes",
        EnhancementColumnType.EstimatedHours => "Estimated Hours",
        EnhancementColumnType.ReturnedHours => "Returned Hours",
        EnhancementColumnType.TimeW1 => "Time W1",
        EnhancementColumnType.TimeW2 => "Time W2",
        EnhancementColumnType.TimeW3 => "Time W3",
        EnhancementColumnType.TimeW4 => "Time W4",
        EnhancementColumnType.TimeW5 => "Time W5",
        EnhancementColumnType.TimeW6 => "Time W6",
        EnhancementColumnType.TimeW7 => "Time W7",
        EnhancementColumnType.TimeW8 => "Time W8",
        EnhancementColumnType.TimeW9 => "Time W9",
        EnhancementColumnType.EstimatedStartDate => "Est. Start Date",
        EnhancementColumnType.EstimatedEndDate => "Est. End Date",
        EnhancementColumnType.StartDate => "Start Date",
        EnhancementColumnType.EndDate => "End Date",
        _ => "Unknown"
    };
}
