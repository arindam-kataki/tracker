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

    public async Task<List<Resource>> GetAllAsync(bool? isClientResource = null, bool? isActive = null)
    {
        var query = _db.Resources.AsQueryable();

        if (isClientResource.HasValue)
            query = query.Where(r => r.IsClientResource == isClientResource.Value);

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        return await query.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<Resource?> GetByIdAsync(string id)
    {
        return await _db.Resources.FindAsync(id);
    }

    public async Task<List<Resource>> GetClientResourcesAsync()
    {
        return await _db.Resources
            .Where(r => r.IsClientResource && r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<Resource>> GetInternalResourcesAsync()
    {
        return await _db.Resources
            .Where(r => !r.IsClientResource && r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Resource> CreateAsync(string name, string? email, bool isClientResource)
    {
        var resource = new Resource
        {
            Name = name,
            Email = email,
            IsClientResource = isClientResource,
            IsActive = true
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        return resource;
    }

    public async Task<Resource?> UpdateAsync(string id, string name, string? email, bool isClientResource, bool isActive)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null)
            return null;

        resource.Name = name;
        resource.Email = email;
        resource.IsClientResource = isClientResource;
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
