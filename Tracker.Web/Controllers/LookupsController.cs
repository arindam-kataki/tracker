using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tracker.Web.Services;
using Tracker.Web.ViewModels;
using Tracker.Web.Services.Interfaces;

namespace Tracker.Web.Controllers;

[Authorize(Policy = "SuperAdmin")]
[Route("lookups")]
public class LookupsController : BaseController
{
    private readonly IResourceTypeLookupService _resourceTypeService;
    private readonly ISkillService _skillService;
    private readonly IServiceAreaService _serviceAreaService;
    private readonly ILogger<LookupsController> _logger;
    private readonly IWorkPhaseService _workPhaseService;

    public LookupsController(
        IAuthService authService,
        IResourceTypeLookupService resourceTypeService,
        ISkillService skillService,
        IServiceAreaService serviceAreaService,
        IWorkPhaseService workPhaseService,
        ILogger<LookupsController> logger) : base(authService)
    {
        _resourceTypeService = resourceTypeService;
        _skillService = skillService;
        _serviceAreaService = serviceAreaService;
        _workPhaseService = workPhaseService;
        _logger = logger;
    }

    #region Resource Types

    [HttpGet("resource-types")]
    public async Task<IActionResult> ResourceTypes(string? search)
    {
        var resourceTypes = await _resourceTypeService.GetAllAsync(search);

        var model = new ResourceTypesViewModel
        {
            ResourceTypes = resourceTypes,
            SearchTerm = search
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "resourcetypes");
        return View(model);
    }

    [HttpGet("resource-types/edit")]
    public async Task<IActionResult> EditResourceType(string? id)
    {
        var model = new EditResourceTypeViewModel();

        if (!string.IsNullOrEmpty(id))
        {
            var resourceType = await _resourceTypeService.GetByIdAsync(id);
            if (resourceType == null)
                return NotFound();

            model.Id = resourceType.Id;
            model.Name = resourceType.Name;
            model.Description = resourceType.Description;
            model.DisplayOrder = resourceType.DisplayOrder;
            model.IsActive = resourceType.IsActive;
            model.EnhancementColumn = resourceType.EnhancementColumn;
            model.AllowMultiple = resourceType.AllowMultiple;
        }

        return PartialView("_EditResourceType", model);
    }

    [HttpPost("resource-types/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveResourceType(EditResourceTypeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_EditResourceType", model);
        }

        if (string.IsNullOrEmpty(model.Id))
        {
            await _resourceTypeService.CreateAsync(
                model.Name,
                model.Description,
                model.DisplayOrder,
                model.EnhancementColumn,
                model.AllowMultiple);
            _logger.LogInformation("Resource type {Name} created by {Admin}", model.Name, CurrentUserEmail);
        }
        else
        {
            await _resourceTypeService.UpdateAsync(
                model.Id,
                model.Name,
                model.Description,
                model.DisplayOrder,
                model.IsActive,
                model.EnhancementColumn,
                model.AllowMultiple);
            _logger.LogInformation("Resource type {Name} updated by {Admin}", model.Name, CurrentUserEmail);
        }

        return Json(new { success = true });
    }

    [HttpPost("resource-types/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResourceType(string id)
    {
        var resourceType = await _resourceTypeService.GetByIdAsync(id);
        if (resourceType == null)
            return NotFound();

        var deleted = await _resourceTypeService.DeleteAsync(id);
        if (!deleted)
        {
            return BadRequest("Cannot delete resource type that is in use by resources.");
        }

        _logger.LogInformation("Resource type {Name} deleted by {Admin}", resourceType.Name, CurrentUserEmail);

        return Json(new { success = true });
    }

    #endregion

    #region Skills

    [HttpGet("skills")]
    public async Task<IActionResult> Skills(string? search, string? serviceAreaId)
    {
        var skills = await _skillService.GetAllAsync(search, serviceAreaId);
        var serviceAreas = await _serviceAreaService.GetAllAsync();

        var model = new SkillsViewModel
        {
            Skills = skills,
            SearchTerm = search,
            ServiceAreaFilter = serviceAreaId,
            ServiceAreas = serviceAreas
                .Select(sa => new SelectListItem { Value = sa.Id, Text = sa.Name })
                .ToList()
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "skills");
        return View(model);
    }

    [HttpGet("skills/edit")]
    public async Task<IActionResult> EditSkill(string? id)
    {
        var serviceAreas = await _serviceAreaService.GetAllAsync();
        var model = new EditSkillViewModel
        {
            ServiceAreas = serviceAreas
                .Select(sa => new SelectListItem { Value = sa.Id, Text = sa.Name })
                .ToList()
        };

        if (!string.IsNullOrEmpty(id))
        {
            var skill = await _skillService.GetByIdAsync(id);
            if (skill == null)
                return NotFound();

            model.Id = skill.Id;
            model.Name = skill.Name;
            model.Description = skill.Description;
            model.ServiceAreaId = skill.ServiceAreaId;
            model.IsActive = skill.IsActive;
        }

        return PartialView("_EditSkill", model);
    }

    [HttpPost("skills/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSkill(EditSkillViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var serviceAreas = await _serviceAreaService.GetAllAsync();
            model.ServiceAreas = serviceAreas
                .Select(sa => new SelectListItem { Value = sa.Id, Text = sa.Name })
                .ToList();
            return PartialView("_EditSkill", model);
        }

        if (string.IsNullOrEmpty(model.Id))
        {
            await _skillService.CreateAsync(model.Name, model.Description, model.ServiceAreaId);
            _logger.LogInformation("Skill {Name} created by {Admin}", model.Name, CurrentUserEmail);
        }
        else
        {
            await _skillService.UpdateAsync(model.Id, model.Name, model.Description, model.ServiceAreaId, model.IsActive);
            _logger.LogInformation("Skill {Name} updated by {Admin}", model.Name, CurrentUserEmail);
        }

        return Json(new { success = true });
    }

    [HttpPost("skills/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSkill(string id)
    {
        var skill = await _skillService.GetByIdAsync(id);
        if (skill == null)
            return NotFound();

        var deleted = await _skillService.DeleteAsync(id);
        if (!deleted)
        {
            return BadRequest("Cannot delete skill that is in use by resources or enhancements.");
        }

        _logger.LogInformation("Skill {Name} deleted by {Admin}", skill.Name, CurrentUserEmail);

        return Json(new { success = true });
    }

    [HttpGet("skills/by-service-area/{serviceAreaId}")]
    public async Task<IActionResult> GetSkillsByServiceArea(string serviceAreaId)
    {
        var skills = await _skillService.GetActiveByServiceAreaAsync(serviceAreaId);
        return Json(skills.Select(s => new { s.Id, s.Name }));
    }

    #endregion


    #region Work Phases

    [HttpGet("work-phases")]
    public async Task<IActionResult> WorkPhases(string? search)
    {
        var workPhases = await _workPhaseService.GetAllAsync(activeOnly: false);

        if (!string.IsNullOrEmpty(search))
        {
            workPhases = workPhases
                .Where(wp => wp.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                             wp.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var model = new WorkPhasesViewModel
        {
            WorkPhases = workPhases,
            SearchTerm = search
        };

        ViewBag.Sidebar = await GetSidebarViewModelAsync(currentPage: "workphases");
        return View(model);
    }

    [HttpGet("work-phases/edit")]
    public async Task<IActionResult> EditWorkPhase(string? id)
    {
        var model = new EditWorkPhaseViewModel();

        if (!string.IsNullOrEmpty(id))
        {
            var workPhase = await _workPhaseService.GetByIdAsync(id);
            if (workPhase == null)
                return NotFound();

            model.Id = workPhase.Id;
            model.Name = workPhase.Name;
            model.Code = workPhase.Code;
            model.Description = workPhase.Description;
            model.DefaultContributionPercent = workPhase.DefaultContributionPercent;
            model.DisplayOrder = workPhase.DisplayOrder;
            model.IsActive = workPhase.IsActive;
            model.ForEstimation = workPhase.ForEstimation;
            model.ForTimeRecording = workPhase.ForTimeRecording;
            model.ForConsolidation = workPhase.ForConsolidation;
        }

        return PartialView("_EditWorkPhase", model);
    }

    [HttpPost("work-phases/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveWorkPhase(EditWorkPhaseViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return PartialView("_EditWorkPhase", model);
        }

        if (string.IsNullOrEmpty(model.Id))
        {
            await _workPhaseService.CreateAsync(
                model.Name,
                model.Code,
                model.Description,
                model.DefaultContributionPercent,
                model.DisplayOrder,
                model.ForEstimation,
                model.ForTimeRecording,
                model.ForConsolidation);
            _logger.LogInformation("Work phase {Name} created by {Admin}", model.Name, CurrentUserEmail);
        }
        else
        {
            await _workPhaseService.UpdateAsync(
                model.Id,
                model.Name,
                model.Code,
                model.Description,
                model.DefaultContributionPercent,
                model.DisplayOrder,
                model.IsActive,
                model.ForEstimation,
                model.ForTimeRecording,
                model.ForConsolidation);
            _logger.LogInformation("Work phase {Name} updated by {Admin}", model.Name, CurrentUserEmail);
        }

        return Json(new { success = true });
    }

    [HttpPost("work-phases/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteWorkPhase(string id)
    {
        var workPhase = await _workPhaseService.GetByIdAsync(id);
        if (workPhase == null)
            return NotFound();

        var result = await _workPhaseService.DeleteAsync(id);
        if (!result.success)
        {
            return BadRequest(result.error);
        }

        _logger.LogInformation("Work phase {Name} deleted by {Admin}", workPhase.Name, CurrentUserEmail);
        return Json(new { success = true });
    }

    #endregion

}
