using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

public class ResourceTypeLookupService : IResourceTypeLookupService
{
    private readonly TrackerDbContext _context;

    public ResourceTypeLookupService(TrackerDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResourceTypeLookupDto>> GetAllAsync(string? search = null)
    {
        var query = _context.ResourceTypeLookups.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(search) || 
                                     (r.Description != null && r.Description.ToLower().Contains(search)));
        }

        var items = await query
            .OrderBy(r => r.DisplayOrder)
            .ThenBy(r => r.Name)
            .Select(r => new ResourceTypeLookupDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                DisplayOrder = r.DisplayOrder,
                IsActive = r.IsActive,
                ResourceCount = r.Resources.Count,
                EnhancementColumn = r.EnhancementColumn,
                AllowMultiple = r.AllowMultiple
            })
            .ToListAsync();
        
        // Set display text for each item
        foreach (var item in items)
        {
            item.EnhancementColumnDisplay = GetColumnDisplayName(item.EnhancementColumn);
        }
        
        return items;
    }

    public async Task<ResourceTypeLookup?> GetByIdAsync(string id)
    {
        return await _context.ResourceTypeLookups.FindAsync(id);
    }

    public async Task<ResourceTypeLookup> CreateAsync(string name, string? description, int displayOrder,
        EnhancementColumnType enhancementColumn, bool allowMultiple)
    {
        var resourceType = new ResourceTypeLookup
        {
            Name = name,
            Description = description,
            DisplayOrder = displayOrder,
            EnhancementColumn = enhancementColumn,
            AllowMultiple = allowMultiple
        };

        _context.ResourceTypeLookups.Add(resourceType);
        await _context.SaveChangesAsync();
        return resourceType;
    }

    public async Task UpdateAsync(string id, string name, string? description, int displayOrder, bool isActive,
        EnhancementColumnType enhancementColumn, bool allowMultiple)
    {
        var resourceType = await _context.ResourceTypeLookups.FindAsync(id);
        if (resourceType == null) return;

        resourceType.Name = name;
        resourceType.Description = description;
        resourceType.DisplayOrder = displayOrder;
        resourceType.IsActive = isActive;
        resourceType.EnhancementColumn = enhancementColumn;
        resourceType.AllowMultiple = allowMultiple;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var resourceType = await _context.ResourceTypeLookups
            .Include(r => r.Resources)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resourceType == null) return false;

        // Don't delete if resources are using this type
        if (resourceType.Resources.Any())
            return false;

        _context.ResourceTypeLookups.Remove(resourceType);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ResourceTypeLookup>> GetActiveAsync()
    {
        return await _context.ResourceTypeLookups
            .Where(r => r.IsActive)
            .OrderBy(r => r.DisplayOrder)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }
    
    private static string GetColumnDisplayName(EnhancementColumnType column)
    {
        return column switch
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
}
