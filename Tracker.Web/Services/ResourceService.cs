using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class ResourceService : IResourceService
{
    private readonly TrackerDbContext _db;

    public ResourceService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<List<Resource>> GetAllAsync(ResourceType? type = null, bool? isActive = null)
    {
        var query = _db.Resources.AsQueryable();

        if (type.HasValue)
            query = query.Where(r => r.Type == type.Value);

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        return await query.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<Resource?> GetByIdAsync(string id)
    {
        return await _db.Resources.FindAsync(id);
    }

    public async Task<List<Resource>> GetByTypeAsync(ResourceType type)
    {
        return await _db.Resources
            .Where(r => r.Type == type && r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<Resource>> GetClientResourcesAsync()
    {
        return await GetByTypeAsync(ResourceType.Client);
    }

    public async Task<List<Resource>> GetSpocResourcesAsync()
    {
        return await GetByTypeAsync(ResourceType.SPOC);
    }

    public async Task<List<Resource>> GetInternalResourcesAsync()
    {
        return await GetByTypeAsync(ResourceType.Internal);
    }

    public async Task<Resource> CreateAsync(string name, string? email, ResourceType type)
    {
        var resource = new Resource
        {
            Name = name,
            Email = email,
            Type = type,
            IsActive = true
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        return resource;
    }

    public async Task<Resource?> UpdateAsync(string id, string name, string? email, ResourceType type, bool isActive)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null)
            return null;

        resource.Name = name;
        resource.Email = email;
        resource.Type = type;
        resource.IsActive = isActive;

        await _db.SaveChangesAsync();
        return resource;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null)
            return false;

        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();
        return true;
    }
}
