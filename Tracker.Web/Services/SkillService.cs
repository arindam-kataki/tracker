using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.ViewModels;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Services;

public class SkillService : ISkillService
{
    private readonly TrackerDbContext _context;

    public SkillService(TrackerDbContext context)
    {
        _context = context;
    }

    public async Task<List<SkillDto>> GetAllAsync(string? search = null, string? serviceAreaId = null)
    {
        var query = _context.Skills
            .Include(s => s.ServiceArea)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(search) || 
                                     (s.Description != null && s.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(serviceAreaId))
        {
            query = query.Where(s => s.ServiceAreaId == serviceAreaId);
        }

        return await query
            .OrderBy(s => s.ServiceArea.Name)
            .ThenBy(s => s.Name)
            .Select(s => new SkillDto
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                ServiceAreaId = s.ServiceAreaId,
                ServiceAreaName = s.ServiceArea.Name,
                IsActive = s.IsActive,
                ResourceCount = s.ResourceSkills.Count,
                EnhancementCount = s.EnhancementSkills.Count
            })
            .ToListAsync();
    }

    public async Task<Skill?> GetByIdAsync(string id)
    {
        return await _context.Skills
            .Include(s => s.ServiceArea)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Skill> CreateAsync(string name, string? description, string serviceAreaId)
    {
        var skill = new Skill
        {
            Name = name,
            Description = description,
            ServiceAreaId = serviceAreaId
        };

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
        return skill;
    }

    public async Task UpdateAsync(string id, string name, string? description, string serviceAreaId, bool isActive)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill == null) return;

        skill.Name = name;
        skill.Description = description;
        skill.ServiceAreaId = serviceAreaId;
        skill.IsActive = isActive;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var skill = await _context.Skills
            .Include(s => s.ResourceSkills)
            .Include(s => s.EnhancementSkills)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (skill == null) return false;

        // Don't delete if resources or enhancements are using this skill
        if (skill.ResourceSkills.Any() || skill.EnhancementSkills.Any())
            return false;

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Skill>> GetActiveByServiceAreaAsync(string serviceAreaId)
    {
        return await _context.Skills
            .Where(s => s.IsActive && s.ServiceAreaId == serviceAreaId)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetSkillsSelectListAsync(string serviceAreaId, List<string>? selectedIds = null)
    {
        var skills = await GetActiveByServiceAreaAsync(serviceAreaId);
        return skills.Select(s => new SelectListItem
        {
            Value = s.Id,
            Text = s.Name,
            Selected = selectedIds?.Contains(s.Id) ?? false
        }).ToList();
    }
}
