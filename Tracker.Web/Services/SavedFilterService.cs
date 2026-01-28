using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

public class SavedFilterService : ISavedFilterService
{
    private readonly TrackerDbContext _db;

    public SavedFilterService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<List<SavedFilter>> GetUserFiltersAsync(string userId, string serviceAreaId)
    {
        return await _db.SavedFilters
            .Where(f => f.ResourceId == userId && f.ServiceAreaId == serviceAreaId)
            .OrderBy(f => f.Name)
            .ToListAsync();
    }

    public async Task<SavedFilter?> GetByIdAsync(string id)
    {
        return await _db.SavedFilters.FindAsync(id);
    }

    public async Task<SavedFilter?> GetDefaultFilterAsync(string userId, string serviceAreaId)
    {
        return await _db.SavedFilters
            .FirstOrDefaultAsync(f => f.ResourceId == userId && f.ServiceAreaId == serviceAreaId && f.IsDefault);
    }

    public async Task<SavedFilter> SaveFilterAsync(string userId, SaveFilterRequest request)
    {
        SavedFilter filter;
        
        if (!string.IsNullOrEmpty(request.Id))
        {
            // Update existing
            filter = await _db.SavedFilters.FindAsync(request.Id) 
                ?? throw new InvalidOperationException("Filter not found");
            
            if (filter.ResourceId != userId)
                throw new UnauthorizedAccessException("Cannot modify another user's filter");
            
            filter.Name = request.Name;
            filter.FilterJson = request.Filter.ToJson();
            filter.IsDefault = request.IsDefault;
            filter.ModifiedAt = DateTime.UtcNow;
        }
        else
        {
            // Create new
            filter = new SavedFilter
            {
                ResourceId = userId,
                ServiceAreaId = request.ServiceAreaId,
                Name = request.Name,
                FilterJson = request.Filter.ToJson(),
                IsDefault = request.IsDefault
            };
            _db.SavedFilters.Add(filter);
        }
        
        // If this is being set as default, unset other defaults
        if (request.IsDefault)
        {
            var otherDefaults = await _db.SavedFilters
                .Where(f => f.ResourceId == userId && 
                           f.ServiceAreaId == request.ServiceAreaId && 
                           f.IsDefault && 
                           f.Id != filter.Id)
                .ToListAsync();
            
            foreach (var other in otherDefaults)
            {
                other.IsDefault = false;
            }
        }
        
        await _db.SaveChangesAsync();
        return filter;
    }

    public async Task<bool> DeleteFilterAsync(string id, string userId)
    {
        var filter = await _db.SavedFilters.FindAsync(id);
        if (filter == null)
            return false;
        
        if (filter.ResourceId != userId)
            return false;
        
        _db.SavedFilters.Remove(filter);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetUserColumnsAsync(string userId, string serviceAreaId)
    {
        var pref = await _db.ResourceColumnPreferences
            .FirstOrDefaultAsync(p => p.ResourceId == userId && p.ServiceAreaId == serviceAreaId);
        
        if (pref == null)
            return ColumnDefinition.GetDefaultColumnKeys();
        
        try
        {
            var columns = JsonSerializer.Deserialize<List<string>>(pref.ColumnsJson);
            return columns ?? ColumnDefinition.GetDefaultColumnKeys();
        }
        catch
        {
            return ColumnDefinition.GetDefaultColumnKeys();
        }
    }

    public async Task SaveUserColumnsAsync(string userId, string serviceAreaId, List<string> columns)
    {
        var pref = await _db.ResourceColumnPreferences
            .FirstOrDefaultAsync(p => p.ResourceId == userId && p.ServiceAreaId == serviceAreaId);
        
        if (pref == null)
        {
            pref = new ResourceColumnPreference
            {
                ResourceId = userId,
                ServiceAreaId = serviceAreaId
            };
            _db.ResourceColumnPreferences.Add(pref);
        }
        
        pref.ColumnsJson = JsonSerializer.Serialize(columns);
        pref.ModifiedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
    }

    public async Task<List<string>> GetDistinctStatusesAsync(string serviceAreaId)
    {
        return await _db.Enhancements
            .Where(e => e.ServiceAreaId == serviceAreaId && e.Status != null)
            .Select(e => e.Status!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<List<string>> GetDistinctInfStatusesAsync(string serviceAreaId)
    {
        return await _db.Enhancements
            .Where(e => e.ServiceAreaId == serviceAreaId && e.InfStatus != null)
            .Select(e => e.InfStatus!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    public async Task<List<string>> GetDistinctServiceLinesAsync(string serviceAreaId)
    {
        return await _db.Enhancements
            .Where(e => e.ServiceAreaId == serviceAreaId && e.ServiceLine != null)
            .Select(e => e.ServiceLine!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }
}
