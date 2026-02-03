namespace Tracker.Web.Entities;

public static class EnhancementColumnRules
{
    public static readonly Dictionary<string, OrganizationType[]> AllowedOrgTypes = new()
    {
        ["Sponsors"]        = new[] { OrganizationType.Client },
        ["ClientSMEs"]      = new[] { OrganizationType.Client },
        ["InvoiceApprover"] = new[] { OrganizationType.Client },
        ["SPOCs"]           = new[] { OrganizationType.Client, OrganizationType.Implementor, OrganizationType.Vendor },
        ["InternalSMEs"]    = new[] { OrganizationType.Implementor },
        ["Resources"]       = new[] { OrganizationType.Implementor, OrganizationType.Vendor }
    };
    
    public static bool IsValidForColumn(string column, OrganizationType orgType)
    {
        return AllowedOrgTypes.TryGetValue(column, out var allowed) && allowed.Contains(orgType);
    }
    
    public static OrganizationType[] GetAllowedOrgTypes(string column)
    {
        return AllowedOrgTypes.TryGetValue(column, out var allowed) ? allowed : Array.Empty<OrganizationType>();
    }
}
