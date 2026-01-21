using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.ViewModels;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class ResourceService : IResourceService
{
    private readonly TrackerDbContext _context;

    public ResourceService(TrackerDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResourceListItem>> GetAllAsync(string? search = null, string? typeFilter = null)
    {
        var query = _context.Resources
            .Include(r => r.ResourceType)
            .Include(r => r.Skills)
                .ThenInclude(rs => rs.Skill)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(search) || 
                                     (r.Email != null && r.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(typeFilter))
        {
            query = query.Where(r => r.ResourceTypeId == typeFilter);
        }

        return await query
            .OrderBy(r => r.Name)
            .Select(r => new ResourceListItem
            {
                Id = r.Id,
                Name = r.Name,
                Email = r.Email,
                ResourceTypeName = r.ResourceType != null ? r.ResourceType.Name : "Unknown",
                ResourceTypeId = r.ResourceTypeId,
                SkillsDisplay = string.Join(", ", r.Skills.Select(s => s.Skill.Name)),
                IsActive = r.IsActive
            })
            .ToListAsync();
    }

    public async Task<Resource?> GetByIdAsync(string id)
    {
        return await _context.Resources
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
            ResourceTypeId = resourceTypeId
        };

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        // Add skills
        if (skillIds?.Any() == true)
        {
            foreach (var skillId in skillIds)
            {
                _context.ResourceSkills.Add(new ResourceSkill
                {
                    ResourceId = resource.Id,
                    SkillId = skillId
                });
            }
            await _context.SaveChangesAsync();
        }

        return resource;
    }

    public async Task UpdateAsync(string id, string name, string? email, string? resourceTypeId, bool isActive, List<string>? skillIds = null)
    {
        var resource = await _context.Resources
            .Include(r => r.Skills)
            .FirstOrDefaultAsync(r => r.Id == id);
            
        if (resource == null) return;

        resource.Name = name;
        resource.Email = email;
        resource.ResourceTypeId = resourceTypeId;
        resource.IsActive = isActive;

        // Update skills - remove all and re-add
        _context.ResourceSkills.RemoveRange(resource.Skills);
        
        if (skillIds?.Any() == true)
        {
            foreach (var skillId in skillIds)
            {
                _context.ResourceSkills.Add(new ResourceSkill
                {
                    ResourceId = resource.Id,
                    SkillId = skillId
                });
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var resource = await _context.Resources.FindAsync(id);
        if (resource != null)
        {
            _context.Resources.Remove(resource);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Resource>> GetActiveAsync()
    {
        return await _context.Resources
            .Include(r => r.ResourceType)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetResourceTypesSelectListAsync()
    {
        return await _context.ResourceTypeLookups
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
        var skills = await _context.Skills
            .Include(s => s.ServiceArea)
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceArea.Name)
            .ThenBy(s => s.Name)
            .ToListAsync();

        return skills.Select(s => new SelectListItem
        {
            Value = s.Id,
            Text = s.Name,
            Group = new SelectListGroup { Name = s.ServiceArea.Name },
            Selected = selectedIds?.Contains(s.Id) ?? false
        }).ToList();
    }

    public async Task<List<string>> GetResourceSkillIdsAsync(string resourceId)
    {
        return await _context.ResourceSkills
            .Where(rs => rs.ResourceId == resourceId)
            .Select(rs => rs.SkillId)
            .ToListAsync();
    }
}
