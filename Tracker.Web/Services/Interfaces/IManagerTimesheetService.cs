using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

/// <summary>
/// Service interface for Manager Timesheet functionality.
/// Provides hierarchy building and timesheet data aggregation for managers.
/// </summary>
public interface IManagerTimesheetService
{
    /// <summary>
    /// Builds the hierarchy tree for a manager, showing all resources that report to them
    /// (directly or indirectly) with rollup nodes for those who have their own reports.
    /// </summary>
    /// <param name="managerId">The manager's resource ID</param>
    /// <param name="serviceAreaIds">Service areas to include</param>
    /// <param name="year">Year for calculating hours</param>
    /// <param name="month">Month for calculating hours</param>
    /// <returns>List of hierarchy nodes representing the tree structure</returns>
    Task<List<HierarchyNodeViewModel>> BuildHierarchyTreeAsync(
        string managerId,
        List<string> serviceAreaIds,
        int year,
        int month);

    /// <summary>
    /// Gets all resource IDs that should be included for a given node selection.
    /// For individual nodes, returns just that resource.
    /// For rollup nodes, returns the resource and all their subordinates recursively.
    /// </summary>
    /// <param name="nodeId">The selected node ID</param>
    /// <param name="managerId">The manager's resource ID (for context)</param>
    /// <param name="serviceAreaIds">Service areas to consider</param>
    /// <returns>List of resource IDs included in the selection</returns>
    Task<List<string>> GetResourceIdsForNodeAsync(
        string nodeId,
        string managerId,
        List<string> serviceAreaIds);

    /// <summary>
    /// Gets calendar data (hours per day) for a set of resources in a given month.
    /// </summary>
    /// <param name="resourceIds">Resource IDs to include</param>
    /// <param name="serviceAreaIds">Service areas to include</param>
    /// <param name="year">Year</param>
    /// <param name="month">Month</param>
    /// <returns>List of calendar day view models</returns>
    Task<List<CalendarDayViewModel>> GetCalendarDataAsync(
        List<string> resourceIds,
        List<string> serviceAreaIds,
        int year,
        int month);

    /// <summary>
    /// Gets work items grouped by service area with resource breakdown.
    /// </summary>
    /// <param name="resourceIds">Resource IDs to include</param>
    /// <param name="serviceAreaIds">Service areas to include</param>
    /// <param name="year">Year</param>
    /// <param name="month">Month</param>
    /// <returns>List of service area groups with work items</returns>
    Task<List<ManagerServiceAreaGroupViewModel>> GetWorkItemsByServiceAreaAsync(
        List<string> resourceIds,
        List<string> serviceAreaIds,
        int year,
        int month);

    /// <summary>
    /// Gets the complete view model for the manager timesheet page.
    /// </summary>
    /// <param name="managerId">The manager's resource ID</param>
    /// <param name="managerName">The manager's display name</param>
    /// <param name="serviceAreaIds">Service areas the manager can view</param>
    /// <param name="viewableServiceAreas">Full service area entities for display</param>
    /// <param name="year">Year to display</param>
    /// <param name="month">Month to display</param>
    /// <param name="selectedNodeId">Currently selected node (null for default/rollup all)</param>
    /// <returns>Complete view model for the page</returns>
    Task<ManagerTimesheetViewModel> GetViewModelAsync(
        string managerId,
        string managerName,
        List<string> serviceAreaIds,
        List<ServiceArea> viewableServiceAreas,
        int year,
        int month,
        string? selectedNodeId = null);

    /// <summary>
    /// Checks if the manager has any reportees in the given service areas.
    /// </summary>
    /// <param name="managerId">The manager's resource ID</param>
    /// <param name="serviceAreaIds">Service areas to check</param>
    /// <returns>True if the manager has at least one reportee</returns>
    Task<bool> HasReporteesAsync(string managerId, List<string> serviceAreaIds);
}
