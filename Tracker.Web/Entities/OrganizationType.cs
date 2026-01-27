namespace Tracker.Web.Entities;

public enum OrganizationType
{
    Client = 0,
    Implementor = 1,
    Vendor = 2
}

public static class OrganizationTypeHelper
{
    public static string ToDisplayString(this OrganizationType orgType) => orgType switch
    {
        OrganizationType.Client => "Client",
        OrganizationType.Implementor => "Implementor",
        OrganizationType.Vendor => "Vendor",
        _ => "Unknown"
    };
    
    public static string ToBadgeClass(this OrganizationType orgType) => orgType switch
    {
        OrganizationType.Client => "bg-info",
        OrganizationType.Implementor => "bg-primary",
        OrganizationType.Vendor => "bg-warning text-dark",
        _ => "bg-secondary"
    };
}
