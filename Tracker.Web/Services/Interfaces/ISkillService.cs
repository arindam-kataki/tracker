using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Entities;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services.Interfaces;

public interface ISkillService
{
    Task<List<SkillDto>> GetAllAsync(string? search = null, string? serviceAreaId = null);
    Task<Skill?> GetByIdAsync(string id);
    Task<Skill> CreateAsync(string name, string? description, string serviceAreaId);
    Task UpdateAsync(string id, string name, string? description, string serviceAreaId, bool isActive);
    Task<bool> DeleteAsync(string id);
    Task<List<Skill>> GetActiveByServiceAreaAsync(string serviceAreaId);
    Task<List<SelectListItem>> GetSkillsSelectListAsync(string serviceAreaId, List<string>? selectedIds = null);
    Task UpdateEnhancementSkillsAsync(string enhancementId, List<string>? skillIds);
}
