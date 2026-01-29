using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

/// <summary>
/// Service implementation for Manager Timesheet functionality.
/// </summary>
public class ManagerTimesheetService : IManagerTimesheetService
{
    private readonly TrackerDbContext _db;
    private readonly IResourceService _resourceService;
    private readonly ILogger<ManagerTimesheetService> _logger;

    public ManagerTimesheetService(
        TrackerDbContext db,
        IResourceService resourceService,
        ILogger<ManagerTimesheetService> logger)
    {
        _db = db;
        _resourceService = resourceService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<HierarchyNodeViewModel>> BuildHierarchyTreeAsync(
        string managerId,
        List<string> serviceAreaIds,
        int year,
        int month)
    {
        var result = new List<HierarchyNodeViewModel>();
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Get all resources in the manager's chain
        var allResources = await _resourceService.GetReportingChainDownwardsAsync(managerId, serviceAreaIds);
        var resourceDict = allResources.ToDictionary(r => r.Id);

        // Get hours for all resources in the month
        var hoursDict = await GetHoursByResourceAsync(
            allResources.Select(r => r.Id).ToList(),
            serviceAreaIds,
            startDate,
            endDate);

        // Build a map of who reports to whom within these service areas
        var reportsToMap = await BuildReportsToMapAsync(serviceAreaIds);

        // Find direct reports to the manager
        var directReportIds = reportsToMap
            .Where(kvp => kvp.Value == managerId)
            .Select(kvp => kvp.Key)
            .Where(id => resourceDict.ContainsKey(id))
            .ToList();

        // Create the root rollup node (all team)
        var allResourceIds = allResources.Select(r => r.Id).ToList();
        var rootRollup = new HierarchyNodeViewModel
        {
            NodeId = $"rollup_{managerId}",
            ResourceId = managerId,
            Name = "Your Team (All)",
            IsRollup = true,
            IsSelf = false,
            TotalHours = allResourceIds.Sum(id => hoursDict.GetValueOrDefault(id, 0)),
            ResourceCount = allResourceIds.Count,
            Level = 0
        };

        // Build children for root
        rootRollup.Children = BuildChildNodes(
            managerId,
            directReportIds,
            resourceDict,
            hoursDict,
            reportsToMap,
            serviceAreaIds,
            level: 1);

        result.Add(rootRollup);
        return result;
    }

    private List<HierarchyNodeViewModel> BuildChildNodes(
        string parentId,
        List<string> directReportIds,
        Dictionary<string, Resource> resourceDict,
        Dictionary<string, decimal> hoursDict,
        Dictionary<string, string> reportsToMap,
        List<string> serviceAreaIds,
        int level)
    {
        var nodes = new List<HierarchyNodeViewModel>();

        // Add the parent as an individual node first (if not root level)
        if (resourceDict.TryGetValue(parentId, out var parentResource))
        {
            nodes.Add(new HierarchyNodeViewModel
            {
                NodeId = parentId,
                ResourceId = parentId,
                Name = parentResource.Name,
                IsRollup = false,
                IsSelf = false, // Will be set by caller if needed
                TotalHours = hoursDict.GetValueOrDefault(parentId, 0),
                ResourceCount = 1,
                Level = level
            });
        }

        // Add each direct report
        foreach (var reportId in directReportIds.OrderBy(id => resourceDict.GetValueOrDefault(id)?.Name ?? ""))
        {
            if (!resourceDict.TryGetValue(reportId, out var resource))
                continue;

            // Check if this person has their own reports
            var theirReports = reportsToMap
                .Where(kvp => kvp.Value == reportId)
                .Select(kvp => kvp.Key)
                .Where(id => resourceDict.ContainsKey(id))
                .ToList();

            if (theirReports.Any())
            {
                // Create a rollup node for this person
                var subordinateIds = GetAllSubordinateIds(reportId, reportsToMap, resourceDict);
                subordinateIds.Add(reportId); // Include self

                var rollupNode = new HierarchyNodeViewModel
                {
                    NodeId = $"rollup_{reportId}",
                    ResourceId = reportId,
                    Name = $"{resource.Name} (Team)",
                    IsRollup = true,
                    IsSelf = false,
                    TotalHours = subordinateIds.Sum(id => hoursDict.GetValueOrDefault(id, 0)),
                    ResourceCount = subordinateIds.Count,
                    Level = level
                };

                // Recursively build children
                rollupNode.Children = BuildChildNodes(
                    reportId,
                    theirReports,
                    resourceDict,
                    hoursDict,
                    reportsToMap,
                    serviceAreaIds,
                    level + 1);

                nodes.Add(rollupNode);
            }
            else
            {
                // Just an individual node
                nodes.Add(new HierarchyNodeViewModel
                {
                    NodeId = reportId,
                    ResourceId = reportId,
                    Name = resource.Name,
                    IsRollup = false,
                    IsSelf = false,
                    TotalHours = hoursDict.GetValueOrDefault(reportId, 0),
                    ResourceCount = 1,
                    Level = level
                });
            }
        }

        return nodes;
    }

    private List<string> GetAllSubordinateIds(
        string resourceId,
        Dictionary<string, string> reportsToMap,
        Dictionary<string, Resource> resourceDict)
    {
        var result = new List<string>();
        var toProcess = new Queue<string>();
        
        // Find direct reports
        var directReports = reportsToMap
            .Where(kvp => kvp.Value == resourceId)
            .Select(kvp => kvp.Key)
            .Where(id => resourceDict.ContainsKey(id));

        foreach (var id in directReports)
        {
            toProcess.Enqueue(id);
        }

        while (toProcess.Count > 0)
        {
            var current = toProcess.Dequeue();
            if (!result.Contains(current))
            {
                result.Add(current);
                
                // Find this person's reports
                var theirReports = reportsToMap
                    .Where(kvp => kvp.Value == current)
                    .Select(kvp => kvp.Key)
                    .Where(id => resourceDict.ContainsKey(id));

                foreach (var id in theirReports)
                {
                    toProcess.Enqueue(id);
                }
            }
        }

        return result;
    }

    private async Task<Dictionary<string, string>> BuildReportsToMapAsync(List<string> serviceAreaIds)
    {
        // Map: ResourceId -> ReportsToResourceId
        var relationships = await _db.ResourceServiceAreas
            .Where(rsa => serviceAreaIds.Contains(rsa.ServiceAreaId))
            .Where(rsa => rsa.ReportsToResourceId != null)
            .Where(rsa => rsa.Resource.IsActive)
            .Select(rsa => new { rsa.ResourceId, rsa.ReportsToResourceId })
            .ToListAsync();

        // If a person reports to multiple people in different SAs, we take any one
        return relationships
            .GroupBy(r => r.ResourceId)
            .ToDictionary(g => g.Key, g => g.First().ReportsToResourceId!);
    }

    private async Task<Dictionary<string, decimal>> GetHoursByResourceAsync(
        List<string> resourceIds,
        List<string> serviceAreaIds,
        DateOnly startDate,
        DateOnly endDate)
    {
        // SQLite doesn't support Sum on decimal, so we fetch and aggregate in memory
        var entries = await _db.TimeEntries
            .Where(te => resourceIds.Contains(te.ResourceId))
            .Where(te => serviceAreaIds.Contains(te.Enhancement.ServiceAreaId))
            .Where(te => te.StartDate >= startDate && te.StartDate <= endDate)
            .Select(te => new { te.ResourceId, te.Hours })
            .ToListAsync();

        return entries
            .GroupBy(e => e.ResourceId)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Hours));
    }

    /// <inheritdoc />
    public async Task<List<string>> GetResourceIdsForNodeAsync(
        string nodeId,
        string managerId,
        List<string> serviceAreaIds)
    {
        if (nodeId.StartsWith("rollup_"))
        {
            // It's a rollup node - get the resource ID and all subordinates
            var resourceId = nodeId.Replace("rollup_", "");
            var resources = await _resourceService.GetReportingChainDownwardsAsync(resourceId, serviceAreaIds);
            return resources.Select(r => r.Id).ToList();
        }
        else
        {
            // Individual node - just return the single resource
            return new List<string> { nodeId };
        }
    }

    /// <inheritdoc />
    public async Task<List<CalendarDayViewModel>> GetCalendarDataAsync(
        List<string> resourceIds,
        List<string> serviceAreaIds,
        int year,
        int month)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Get all entries for the month with work phase info
        var entries = await _db.TimeEntries
            .Include(te => te.WorkPhase)
            .Where(te => resourceIds.Contains(te.ResourceId))
            .Where(te => serviceAreaIds.Contains(te.Enhancement.ServiceAreaId))
            .Where(te => te.StartDate >= startDate && te.StartDate <= endDate)
            .ToListAsync();

        // Group by date, then by work phase
        var dataByDate = entries
            .GroupBy(e => e.StartDate)
            .ToDictionary(
                g => g.Key,
                g => new
                {
                    TotalHours = g.Sum(e => e.Hours),
                    TotalContributed = g.Sum(e => e.ContributedHours),
                    ByPhase = g
                        .GroupBy(e => new
                        {
                            e.WorkPhaseId,
                            Code = e.WorkPhase?.Code ?? "UNK",
                            Name = e.WorkPhase?.Name ?? "Unknown"
                        })
                        .Select(pg => new WorkPhaseHoursViewModel
                        {
                            WorkPhaseId = pg.Key.WorkPhaseId,
                            WorkPhaseCode = pg.Key.Code,
                            WorkPhaseName = pg.Key.Name,
                            Hours = pg.Sum(e => e.Hours),
                            ContributedHours = pg.Sum(e => e.ContributedHours)
                        })
                        .OrderBy(p => p.WorkPhaseCode)
                        .ToList()
                });

        // Build calendar days for the entire month
        var result = new List<CalendarDayViewModel>();
        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var dayData = dataByDate.GetValueOrDefault(date);
            result.Add(new CalendarDayViewModel
            {
                Date = date,
                Hours = dayData?.TotalHours ?? 0,
                ContributedHours = dayData?.TotalContributed ?? 0,
                IsCurrentMonth = true,
                WorkPhaseBreakdown = dayData?.ByPhase ?? new List<WorkPhaseHoursViewModel>()
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<List<ManagerServiceAreaGroupViewModel>> GetWorkItemsByServiceAreaAsync(
        List<string> resourceIds,
        List<string> serviceAreaIds,
        int year,
        int month)
    {
        var startDate = new DateOnly(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Get all entries with related data
        var entries = await _db.TimeEntries
            .Include(te => te.Resource)
            .Include(te => te.Enhancement)
                .ThenInclude(e => e.ServiceArea)
            .Include(te => te.WorkPhase)
            .Where(te => resourceIds.Contains(te.ResourceId))
            .Where(te => serviceAreaIds.Contains(te.Enhancement.ServiceAreaId))
            .Where(te => te.StartDate >= startDate && te.StartDate <= endDate)
            .ToListAsync();

        // Group by Service Area
        var result = entries
            .GroupBy(e => new
            {
                ServiceAreaId = e.Enhancement.ServiceAreaId,
                ServiceAreaCode = e.Enhancement.ServiceArea?.Code ?? "Unknown",
                ServiceAreaName = e.Enhancement.ServiceArea?.Name ?? "Unknown"
            })
            .OrderBy(g => g.Key.ServiceAreaCode)
            .Select(saGroup => new ManagerServiceAreaGroupViewModel
            {
                ServiceAreaId = saGroup.Key.ServiceAreaId,
                ServiceAreaCode = saGroup.Key.ServiceAreaCode,
                ServiceAreaName = saGroup.Key.ServiceAreaName,
                TotalHours = saGroup.Sum(e => e.Hours),
                TotalContributedHours = saGroup.Sum(e => e.ContributedHours),
                WorkItems = saGroup
                    .GroupBy(e => new
                    {
                        e.EnhancementId,
                        WorkId = e.Enhancement?.WorkId ?? "Unknown",
                        Description = e.Enhancement?.Description
                    })
                    .OrderBy(g => g.Key.WorkId)
                    .Select(wiGroup => new ManagerWorkItemViewModel
                    {
                        EnhancementId = wiGroup.Key.EnhancementId,
                        WorkId = wiGroup.Key.WorkId,
                        Description = wiGroup.Key.Description,
                        TotalHours = wiGroup.Sum(e => e.Hours),
                        TotalContributedHours = wiGroup.Sum(e => e.ContributedHours),
                        ResourceBreakdown = wiGroup
                            .GroupBy(e => new { e.ResourceId, ResourceName = e.Resource?.Name ?? "Unknown" })
                            .OrderBy(g => g.Key.ResourceName)
                            .Select(rGroup => new ResourceHoursViewModel
                            {
                                ResourceId = rGroup.Key.ResourceId,
                                ResourceName = rGroup.Key.ResourceName,
                                Hours = rGroup.Sum(e => e.Hours),
                                ContributedHours = rGroup.Sum(e => e.ContributedHours),
                                WorkPhaseBreakdown = rGroup
                                    .GroupBy(e => new
                                    {
                                        e.WorkPhaseId,
                                        Code = e.WorkPhase?.Code ?? "UNK",
                                        Name = e.WorkPhase?.Name ?? "Unknown"
                                    })
                                    .OrderBy(pg => pg.Key.Code)
                                    .Select(pg => new WorkPhaseHoursViewModel
                                    {
                                        WorkPhaseId = pg.Key.WorkPhaseId,
                                        WorkPhaseCode = pg.Key.Code,
                                        WorkPhaseName = pg.Key.Name,
                                        Hours = pg.Sum(e => e.Hours),
                                        ContributedHours = pg.Sum(e => e.ContributedHours)
                                    })
                                    .ToList()
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

        return result;
    }

    /// <inheritdoc />
    public async Task<ManagerTimesheetViewModel> GetViewModelAsync(
        string managerId,
        string managerName,
        List<string> serviceAreaIds,
        List<ServiceArea> viewableServiceAreas,
        int year,
        int month,
        string? selectedNodeId = null)
    {
        // Build hierarchy tree
        var hierarchyTree = await BuildHierarchyTreeAsync(managerId, serviceAreaIds, year, month);

        // Determine selected node (default to root rollup)
        var effectiveNodeId = selectedNodeId ?? $"rollup_{managerId}";
        var isRollup = effectiveNodeId.StartsWith("rollup_");

        // Get resource IDs for the selected node
        var resourceIds = await GetResourceIdsForNodeAsync(effectiveNodeId, managerId, serviceAreaIds);

        // Find the selected node in the tree for display info
        var selectedNodeName = "Your Team (All)";
        var selectedNode = FindNodeInTree(hierarchyTree, effectiveNodeId);
        if (selectedNode != null)
        {
            selectedNodeName = selectedNode.Name;
        }

        // Get calendar data
        var calendarDays = await GetCalendarDataAsync(resourceIds, serviceAreaIds, year, month);

        // Get work items by service area
        var serviceAreaGroups = await GetWorkItemsByServiceAreaAsync(resourceIds, serviceAreaIds, year, month);

        return new ManagerTimesheetViewModel
        {
            ManagerId = managerId,
            ManagerName = managerName,
            Year = year,
            Month = month,
            HierarchyTree = hierarchyTree,
            SelectedNodeId = effectiveNodeId,
            IsRollupSelected = isRollup,
            SelectedNodeName = selectedNodeName,
            SelectedResourceCount = resourceIds.Count,
            CalendarDays = calendarDays,
            ServiceAreaGroups = serviceAreaGroups,
            TotalHours = calendarDays.Sum(d => d.Hours),
            TotalContributedHours = calendarDays.Sum(d => d.ContributedHours),
            ViewableServiceAreas = viewableServiceAreas
        };
    }

    private HierarchyNodeViewModel? FindNodeInTree(List<HierarchyNodeViewModel> nodes, string nodeId)
    {
        foreach (var node in nodes)
        {
            if (node.NodeId == nodeId)
                return node;

            var found = FindNodeInTree(node.Children, nodeId);
            if (found != null)
                return found;
        }
        return null;
    }

    /// <inheritdoc />
    public async Task<bool> HasReporteesAsync(string managerId, List<string> serviceAreaIds)
    {
        return await _db.ResourceServiceAreas
            .AnyAsync(rsa => 
                rsa.ReportsToResourceId == managerId && 
                serviceAreaIds.Contains(rsa.ServiceAreaId) &&
                rsa.Resource.IsActive);
    }
}
