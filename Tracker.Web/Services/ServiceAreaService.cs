using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class ServiceAreaService : IServiceAreaService
{
    private readonly TrackerDbContext _db;

    public ServiceAreaService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<List<ServiceArea>> GetAllAsync(bool activeOnly = true)
    {
        var query = _db.ServiceAreas.AsQueryable();

        if (activeOnly)
            query = query.Where(sa => sa.IsActive);

        return await query.OrderBy(sa => sa.DisplayOrder).ToListAsync();
    }

    public async Task<ServiceArea?> GetByIdAsync(string id)
    {
        return await _db.ServiceAreas.FindAsync(id);
    }

    public async Task<ServiceArea> CreateAsync(string name, string code)
    {
        var maxOrder = await _db.ServiceAreas.MaxAsync(sa => (int?)sa.DisplayOrder) ?? 0;

        var serviceArea = new ServiceArea
        {
            Name = name,
            Code = code,
            IsActive = true,
            DisplayOrder = maxOrder + 1
        };

        _db.ServiceAreas.Add(serviceArea);
        await _db.SaveChangesAsync();

        return serviceArea;
    }

    public async Task<ServiceArea?> UpdateAsync(string id, string name, string code, bool isActive, int displayOrder)
    {
        var serviceArea = await _db.ServiceAreas.FindAsync(id);
        if (serviceArea == null)
            return null;

        serviceArea.Name = name;
        serviceArea.Code = code;
        serviceArea.IsActive = isActive;
        serviceArea.DisplayOrder = displayOrder;

        await _db.SaveChangesAsync();
        return serviceArea;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var serviceArea = await _db.ServiceAreas.FindAsync(id);
        if (serviceArea == null)
            return false;

        // Check for related enhancements
        var hasEnhancements = await _db.Enhancements.AnyAsync(e => e.ServiceAreaId == id);
        if (hasEnhancements)
            return false; // Cannot delete if has enhancements

        _db.ServiceAreas.Remove(serviceArea);
        await _db.SaveChangesAsync();
        return true;
    }
}
