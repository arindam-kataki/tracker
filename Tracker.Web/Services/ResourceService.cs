using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

/// <summary>
/// Service for managing resources (people assigned to enhancements)
/// </summary>
public class ResourceService : IResourceService
{
    private readonly TrackerDbContext _db;

    public ResourceService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<List<ResourceListItem>> GetAllAsync(string? search = null, string? typeFilter = null)
    {
        var query = _db.Resources
            .Include(r => r.ResourceType)
            .Include(r => r.Skills)
                .ThenInclude(rs => rs.Skill)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(s) ||
                                     (r.Email != null && r.Email.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(typeFilter))
        {
            query = query.Where(r => r.ResourceTypeId == typeFilter);
        }

        return await query
            .OrderBy(r => r.OrganizationType)
            .ThenBy(r => r.Name)
            .Select(r => new ResourceListItem
            {
                Id = r.Id,
                Name = r.Name,
                Email = r.Email,
                ResourceTypeId = r.ResourceTypeId,
                ResourceTypeName = r.ResourceType != null ? r.ResourceType.Name : null,
                OrganizationType = r.OrganizationType,
                IsActive = r.IsActive,
                SkillNames = r.Skills.Select(s => s.Skill.Name).ToList()
            })
            .ToListAsync();
    }

    public async Task<Resource?> GetByIdAsync(string id)
    {
        return await _db.Resources
            .Include(r => r.ResourceType)
            .Include(r => r.Skills)
                .ThenInclude(rs => rs.Skill)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Resource> CreateAsync(string name, string? email, string? resourceTypeId, List<string>? skillIds = null)
    {
        var resource = new Resource
        {
            Name = name,
            Email = email,
            ResourceTypeId = resourceTypeId,
            IsActive = true
        };

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        // Add skills
        if (skillIds != null && skillIds.Any())
        {
            foreach (var skillId in skillIds)
            {
                _db.ResourceSkills.Add(new ResourceSkill
                {
                    ResourceId = resource.Id,
                    SkillId = skillId
                });
            }
            await _db.SaveChangesAsync();
        }

        return resource;
    }

    public async Task UpdateAsync(string id, string name, string? email, string? resourceTypeId, bool isActive, List<string>? skillIds = null)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource == null) return;

        resource.Name = name;
        resource.Email = email;
        resource.ResourceTypeId = resourceTypeId;
        resource.IsActive = isActive;
        resource.UpdatedAt = DateTime.UtcNow;

        // Update skills - remove existing and add new
        var existingSkills = await _db.ResourceSkills.Where(rs => rs.ResourceId == id).ToListAsync();
        _db.ResourceSkills.RemoveRange(existingSkills);

        if (skillIds != null && skillIds.Any())
        {
            foreach (var skillId in skillIds)
            {
                _db.ResourceSkills.Add(new ResourceSkill
                {
                    ResourceId = id,
                    SkillId = skillId
                });
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var resource = await _db.Resources.FindAsync(id);
        if (resource != null)
        {
            // Remove associated skills first
            var skills = await _db.ResourceSkills.Where(rs => rs.ResourceId == id).ToListAsync();
            _db.ResourceSkills.RemoveRange(skills);
            
            _db.Resources.Remove(resource);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<Resource>> GetActiveAsync()
    {
        return await _db.Resources
            .Include(r => r.ResourceType)
            .Where(r => r.IsActive)
            .OrderBy(r => r.OrganizationType)
            .ThenBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetResourceTypesSelectListAsync()
    {
        return await _db.ResourceTypeLookups
            .Where(rt => rt.IsActive)
            .OrderBy(rt => rt.DisplayOrder)
            .ThenBy(rt => rt.Name)
            .Select(rt => new SelectListItem
            {
                Value = rt.Id,
                Text = rt.Name
            })
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetSkillsSelectListAsync(List<string>? selectedIds = null)
    {
        var skills = await _db.Skills
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem
            {
                Value = s.Id,
                Text = s.Name,
                Selected = selectedIds != null && selectedIds.Contains(s.Id)
            })
            .ToListAsync();

        return skills;
    }

    public async Task<List<string>> GetResourceSkillIdsAsync(string resourceId)
    {
        return await _db.ResourceSkills
            .Where(rs => rs.ResourceId == resourceId)
            .Select(rs => rs.SkillId)
            .ToListAsync();
    }

    public async Task<List<Resource>> GetByResourceTypeNameAsync(string typeName)
    {
        return await _db.Resources
            .Include(r => r.ResourceType)
            .Where(r => r.IsActive && r.ResourceType != null && r.ResourceType.Name == typeName)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }
}