namespace Tracker.Web.Entities.Enums;

/// <summary>
/// Type of employment relationship
/// </summary>
public enum EmploymentType
{
    /// <summary>
    /// Full-time employee (typically 40 hours/week)
    /// </summary>
    FullTime = 0,

    /// <summary>
    /// Part-time employee (reduced hours)
    /// </summary>
    PartTime = 1,

    /// <summary>
    /// Independent contractor (1099)
    /// </summary>
    Contractor = 2,

    /// <summary>
    /// Employee of a vendor/staffing company
    /// </summary>
    Vendor = 3,

    /// <summary>
    /// Subcontractor (contracted through another company)
    /// </summary>
    Subcontractor = 4,

    /// <summary>
    /// Intern (paid or unpaid)
    /// </summary>
    Intern = 5,

    /// <summary>
    /// Temporary employee
    /// </summary>
    Temporary = 6
}

/// <summary>
/// Extension methods for EmploymentType
/// </summary>
public static class EmploymentTypeExtensions
{
    public static string ToDisplayString(this EmploymentType type) => type switch
    {
        EmploymentType.FullTime => "Full-Time",
        EmploymentType.PartTime => "Part-Time",
        EmploymentType.Contractor => "Contractor",
        EmploymentType.Vendor => "Vendor",
        EmploymentType.Subcontractor => "Subcontractor",
        EmploymentType.Intern => "Intern",
        EmploymentType.Temporary => "Temporary",
        _ => type.ToString()
    };

    public static string ToBadgeClass(this EmploymentType type) => type switch
    {
        EmploymentType.FullTime => "bg-success",
        EmploymentType.PartTime => "bg-info",
        EmploymentType.Contractor => "bg-warning text-dark",
        EmploymentType.Vendor => "bg-secondary",
        EmploymentType.Subcontractor => "bg-secondary",
        EmploymentType.Intern => "bg-light text-dark",
        EmploymentType.Temporary => "bg-warning text-dark",
        _ => "bg-secondary"
    };

    /// <summary>
    /// Whether this type is an internal employee (vs external)
    /// </summary>
    public static bool IsEmployee(this EmploymentType type) => type switch
    {
        EmploymentType.FullTime => true,
        EmploymentType.PartTime => true,
        EmploymentType.Intern => true,
        EmploymentType.Temporary => true,
        _ => false
    };

    /// <summary>
    /// Whether this type is external (contractor, vendor, etc.)
    /// </summary>
    public static bool IsExternal(this EmploymentType type) => !type.IsEmployee();

    /// <summary>
    /// Default standard hours per week for this employment type
    /// </summary>
    public static decimal DefaultWeeklyHours(this EmploymentType type) => type switch
    {
        EmploymentType.FullTime => 40m,
        EmploymentType.PartTime => 20m,
        EmploymentType.Contractor => 40m,
        EmploymentType.Vendor => 40m,
        EmploymentType.Subcontractor => 40m,
        EmploymentType.Intern => 20m,
        EmploymentType.Temporary => 40m,
        _ => 40m
    };
}
