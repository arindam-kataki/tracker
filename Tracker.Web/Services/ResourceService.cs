using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

public class ResourceService : IResourceService
{
    private readonly TrackerDbContext _db;
    private readonly ILogger<ResourceService> _logger;

    public ResourceService(TrackerDbContext db, ILogger<ResourceService> logger)
    {
        _db = db;
        _logger = logger;
    }

    #region Legacy Methods

    public async Task<List<ResourceListItem>> GetAllAsync(string? search = null, string? typeFilter = null)
    {
        var query = _db.Resources
            .Include(r => r.ResourceType)
            .Include(r => r.Skills).ThenInclude(rs => rs.Skill)
            .Include(r => r.ServiceAreas).ThenInclude(rsa => rsa.ServiceArea)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(s) || (r.Email != null && r.Email.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(typeFilter))
            query = query.Where(r => r.ResourceTypeId == typeFilter);

        return await query
            .OrderBy(r => r.OrganizationType).ThenBy(r => r.Name)
            .Select(r => new ResourceListItem
            {
                Id = r.Id,
                Name = r.Name,
                Email = r.Email,
                ResourceTypeId = r.ResourceTypeId,
                ResourceTypeName = r.ResourceType != null ? r.ResourceType.Name : null,
                OrganizationType = r.OrganizationType,
                IsActive = r.IsActive,
                HasLogin = r.UserId != null,
                UserId = r.UserId,
                SkillNames = r.Skills.Select(s => s.Skill.Name).ToList(),
                ServiceAreas = r.ServiceAreas.Select(sa => new ResourceServiceAreaSummary
                {
                    ServiceAreaId = sa.ServiceAreaId,
                    Code = sa.ServiceArea != null ? sa.ServiceArea.Code : "",
                    Name = sa.ServiceArea != null ? sa.ServiceArea.Name : "",
                    IsPrimary = sa.IsPrimary,
                    Permissions = sa.Permissions
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<Resource?> GetByIdAsync(string id)
    {
        return await _db.Resources
            .Include(r => r.ResourceType)
            .Include(r => r.Skills).ThenInclude(rs => rs.Skill)
            .Include(r => r.ServiceAreas).ThenInclude(rsa => rsa.ServiceArea)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Resource> CreateAsync(string name, string? email, string? resourceTypeId, List<string>? skillIds = null)
    {
        var resource = new Resource
        {
            Name = name,
            Email = email,
            ResourceTypeId = resourceTypeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Infer OrganizationType from ResourceType
        if (!string.IsNullOrEmpty(resourceTypeId))
        {
            var rt = await _db.ResourceTypeLookups.FindAsync(resourceTypeId);
            if (rt != null)
            {
                var typeName = rt.Name.ToLower();
                if (typeName.Contains("client") || typeName.Contains("sponsor"))
                    resource.OrganizationType = OrganizationType.Client;
                else if (typeName.Contains("vendor"))
                    resource.OrganizationType = OrganizationType.Vendor;
                else
                    resource.OrganizationType = OrganizationType.Implementor;
            }
        }

        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();

        if (skillIds != null && skillIds.Any())
        {
            foreach (var skillId in skillIds)
                _db.ResourceSkills.Add(new ResourceSkill { ResourceId = resource.Id, SkillId = skillId });
            await _db.SaveChangesAsync();
        }

        return resource;
    }

    public async Task UpdateAsync(string id, string name, string? email, string? resourceTypeId, bool isActive, List<string>? skillIds = null)
    {
        var resource = await _db.Resources.Include(r => r.Skills).FirstOrDefaultAsync(r => r.Id == id);
        if (resource == null) return;

        resource.Name = name;
        resource.Email = email;
        resource.ResourceTypeId = resourceTypeId;
        resource.IsActive = isActive;
        resource.UpdatedAt = DateTime.UtcNow;

        // Update OrganizationType
        if (!string.IsNullOrEmpty(resourceTypeId))
        {
            var rt = await _db.ResourceTypeLookups.FindAsync(resourceTypeId);
            if (rt != null)
            {
                var typeName = rt.Name.ToLower();
                if (typeName.Contains("client") || typeName.Contains("sponsor"))
                    resource.OrganizationType = OrganizationType.Client;
                else if (typeName.Contains("vendor"))
                    resource.OrganizationType = OrganizationType.Vendor;
                else
                    resource.OrganizationType = OrganizationType.Implementor;
            }
        }

        // Update skills
        _db.ResourceSkills.RemoveRange(resource.Skills);
        if (skillIds != null && skillIds.Any())
        {
            foreach (var skillId in skillIds)
                _db.ResourceSkills.Add(new ResourceSkill { ResourceId = resource.Id, SkillId = skillId });
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var resource = await _db.Resources
            .Include(r => r.Skills)
            .Include(r => r.ServiceAreas)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (resource == null) return;

        _db.ResourceSkills.RemoveRange(resource.Skills);
        _db.ResourceServiceAreas.RemoveRange(resource.ServiceAreas);
        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Resource>> GetActiveAsync()
    {
        return await _db.Resources
            .Include(r => r.ResourceType)
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetResourceTypesSelectListAsync()
    {
        return await _db.ResourceTypeLookups
            .Where(rt => rt.IsActive)
            .OrderBy(rt => rt.DisplayOrder).ThenBy(rt => rt.Name)
            .Select(rt => new SelectListItem { Value = rt.Id, Text = rt.Name })
            .ToListAsync();
    }

    public async Task<List<SelectListItem>> GetSkillsSelectListAsync(List<string>? selectedIds = null)
    {
        selectedIds ??= new List<string>();
        var skills = await _db.Skills
            .Include(s => s.ServiceArea)
            .Where(s => s.IsActive)
            .OrderBy(s => s.ServiceArea!.Code).ThenBy(s => s.Name)
            .ToListAsync();

        return skills.Select(s => new SelectListItem
        {
            Value = s.Id,
            Text = s.Name,
            Group = new SelectListGroup { Name = s.ServiceArea?.Code ?? "General" },
            Selected = selectedIds.Contains(s.Id)
        }).ToList();
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

    #endregion

    #region New Methods

    public async Task<List<ResourceListItem>> GetAllWithFiltersAsync(string? search, OrganizationType? orgType, string? serviceAreaId)
    {
        var query = _db.Resources
            .Include(r => r.ResourceType)
            .Include(r => r.Skills).ThenInclude(rs => rs.Skill)
            .Include(r => r.ServiceAreas).ThenInclude(rsa => rsa.ServiceArea)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(s) || (r.Email != null && r.Email.ToLower().Contains(s)));
        }

        if (orgType.HasValue)
            query = query.Where(r => r.OrganizationType == orgType.Value);

        if (!string.IsNullOrEmpty(serviceAreaId))
            query = query.Where(r => r.ServiceAreas.Any(sa => sa.ServiceAreaId == serviceAreaId));

        return await query
            .OrderBy(r => r.OrganizationType).ThenBy(r => r.Name)
            .Select(r => new ResourceListItem
            {
                Id = r.Id,
                Name = r.Name,
                Email = r.Email,
                ResourceTypeId = r.ResourceTypeId,
                ResourceTypeName = r.ResourceType != null ? r.ResourceType.Name : null,
                OrganizationType = r.OrganizationType,
                IsActive = r.IsActive,
                HasLogin = r.UserId != null,
                UserId = r.UserId,
                SkillNames = r.Skills.Select(s => s.Skill.Name).ToList(),
                ServiceAreas = r.ServiceAreas.Select(sa => new ResourceServiceAreaSummary
                {
                    ServiceAreaId = sa.ServiceAreaId,
                    Code = sa.ServiceArea != null ? sa.ServiceArea.Code : "",
                    Name = sa.ServiceArea != null ? sa.ServiceArea.Name : "",
                    IsPrimary = sa.IsPrimary,
                    Permissions = sa.Permissions
                }).ToList()
            })
            .ToListAsync();
    }

    public async Task<EditResourceViewModel?> GetForEditAsync(string id)
    {
        var resource = await GetByIdAsync(id);
        if (resource == null) return null;

        var model = new EditResourceViewModel
        {
            Id = resource.Id,
            Name = resource.Name,
            Email = resource.Email,
            Phone = resource.Phone,
            OrganizationType = resource.OrganizationType,
            IsActive = resource.IsActive,
            ResourceTypeId = resource.ResourceTypeId,
            SkillIds = resource.Skills.Select(s => s.SkillId).ToList(),
            SelectedSkillIds = resource.Skills.Select(s => s.SkillId).ToList(),
            ServiceAreaMemberships = resource.ServiceAreas
                .OrderByDescending(sa => sa.IsPrimary)
                .ThenBy(sa => sa.ServiceArea?.Code)
                .Select(sa =>
                {
                    var vm = new EditResourceServiceAreaViewModel
                    {
                        Id = sa.Id,
                        ServiceAreaId = sa.ServiceAreaId,
                        ServiceAreaCode = sa.ServiceArea?.Code ?? "",
                        ServiceAreaName = sa.ServiceArea?.Name ?? "",
                        IsPrimary = sa.IsPrimary
                    };
                    vm.FromPermissions(sa.Permissions);
                    return vm;
                }).ToList()
        };

        model.ResourceTypes = await GetResourceTypesSelectListAsync();
        model.AvailableSkills = await GetSkillsSelectListAsync(model.SkillIds);
        model.OrganizationTypeOptions = GetOrganizationTypesSelectList(resource.OrganizationType);
        model.AvailableServiceAreas = await GetAvailableServiceAreasAsync();
        model.AvailableSkillsGrouped = await GetSkillsGroupedByServiceAreaAsync(
            resource.Id, resource.ServiceAreas.Select(sa => sa.ServiceAreaId).ToList());

        return model;
    }

    public async Task<ResourceOperationResult> CreateResourceAsync(EditResourceViewModel model)
    {
        try
        {
            if (!string.IsNullOrEmpty(model.Email))
            {
                if (await _db.Resources.AnyAsync(r => r.Email == model.Email))
                    return new ResourceOperationResult { Success = false, Errors = new List<string> { "Email already exists." } };
            }

            var resource = new Resource
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                OrganizationType = model.OrganizationType,
                IsActive = model.IsActive,
                ResourceTypeId = model.ResourceTypeId,
                CreatedAt = DateTime.UtcNow
            };

            _db.Resources.Add(resource);

            foreach (var sa in model.ServiceAreaMemberships)
            {
                _db.ResourceServiceAreas.Add(new ResourceServiceArea
                {
                    ResourceId = resource.Id,
                    ServiceAreaId = sa.ServiceAreaId,
                    IsPrimary = sa.IsPrimary,
                    Permissions = sa.ToPermissions(),
                    JoinedAt = DateTime.UtcNow
                });
            }

            var skillIds = model.SelectedSkillIds ?? model.SkillIds ?? new List<string>();
            foreach (var skillId in skillIds)
                _db.ResourceSkills.Add(new ResourceSkill { ResourceId = resource.Id, SkillId = skillId });

            await _db.SaveChangesAsync();
            return new ResourceOperationResult { Success = true, ResourceId = resource.Id, Message = "Resource created." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource");
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "Error creating resource." } };
        }
    }

    public async Task<ResourceOperationResult> UpdateResourceAsync(EditResourceViewModel model)
    {
        try
        {
            var resource = await _db.Resources
                .Include(r => r.ServiceAreas)
                .Include(r => r.Skills)
                .FirstOrDefaultAsync(r => r.Id == model.Id);

            if (resource == null)
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "Resource not found." } };

            if (!string.IsNullOrEmpty(model.Email) && model.Email != resource.Email)
            {
                if (await _db.Resources.AnyAsync(r => r.Email == model.Email && r.Id != model.Id))
                    return new ResourceOperationResult { Success = false, Errors = new List<string> { "Email already exists." } };
            }

            resource.Name = model.Name;
            resource.Email = model.Email;
            resource.Phone = model.Phone;
            resource.OrganizationType = model.OrganizationType;
            resource.IsActive = model.IsActive;
            resource.ResourceTypeId = model.ResourceTypeId;
            resource.UpdatedAt = DateTime.UtcNow;

            _db.ResourceServiceAreas.RemoveRange(resource.ServiceAreas);
            foreach (var sa in model.ServiceAreaMemberships)
            {
                _db.ResourceServiceAreas.Add(new ResourceServiceArea
                {
                    ResourceId = resource.Id,
                    ServiceAreaId = sa.ServiceAreaId,
                    IsPrimary = sa.IsPrimary,
                    Permissions = sa.ToPermissions(),
                    JoinedAt = DateTime.UtcNow
                });
            }

            _db.ResourceSkills.RemoveRange(resource.Skills);
            var skillIds = model.SelectedSkillIds ?? model.SkillIds ?? new List<string>();
            foreach (var skillId in skillIds)
                _db.ResourceSkills.Add(new ResourceSkill { ResourceId = resource.Id, SkillId = skillId });

            await _db.SaveChangesAsync();
            return new ResourceOperationResult { Success = true, ResourceId = resource.Id, Message = "Resource updated." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource");
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "Error updating resource." } };
        }
    }

    public async Task<ResourceOperationResult> DeleteResourceAsync(string id)
    {
        try
        {
            var resource = await _db.Resources
                .Include(r => r.Skills)
                .Include(r => r.ServiceAreas)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null)
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "Resource not found." } };

            _db.ResourceSkills.RemoveRange(resource.Skills);
            _db.ResourceServiceAreas.RemoveRange(resource.ServiceAreas);
            _db.Resources.Remove(resource);
            await _db.SaveChangesAsync();

            return new ResourceOperationResult { Success = true, Message = "Resource deleted." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource");
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "Error deleting resource." } };
        }
    }

    #endregion

    #region Service Area Memberships

    public async Task<ResourceOperationResult> AddServiceAreaMembershipAsync(string resourceId, string serviceAreaId, bool isPrimary, Permissions permissions)
    {
        try
        {
            if (await _db.ResourceServiceAreas.AnyAsync(rsa => rsa.ResourceId == resourceId && rsa.ServiceAreaId == serviceAreaId))
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "Already a member." } };

            if (isPrimary)
            {
                var existing = await _db.ResourceServiceAreas.Where(rsa => rsa.ResourceId == resourceId && rsa.IsPrimary).ToListAsync();
                foreach (var e in existing) e.IsPrimary = false;
            }

            _db.ResourceServiceAreas.Add(new ResourceServiceArea
            {
                ResourceId = resourceId,
                ServiceAreaId = serviceAreaId,
                IsPrimary = isPrimary,
                Permissions = permissions,
                JoinedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return new ResourceOperationResult { Success = true, Message = "Membership added." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding membership");
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "Error adding membership." } };
        }
    }

    public async Task<ResourceOperationResult> RemoveServiceAreaMembershipAsync(string membershipId)
    {
        try
        {
            var m = await _db.ResourceServiceAreas.FindAsync(membershipId);
            if (m == null) return new ResourceOperationResult { Success = false, Errors = new List<string> { "Not found." } };

            _db.ResourceServiceAreas.Remove(m);
            await _db.SaveChangesAsync();
            return new ResourceOperationResult { Success = true, Message = "Removed." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing membership");
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "Error." } };
        }
    }

    public async Task<ResourceOperationResult> UpdateServiceAreaPermissionsAsync(string membershipId, Permissions permissions)
    {
        try
        {
            var m = await _db.ResourceServiceAreas.FindAsync(membershipId);
            if (m == null) return new ResourceOperationResult { Success = false, Errors = new List<string> { "Not found." } };

            m.Permissions = permissions;
            await _db.SaveChangesAsync();
            return new ResourceOperationResult { Success = true, Message = "Updated." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permissions");
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "Error." } };
        }
    }

    #endregion

    #region Lookups

    public List<SelectListItem> GetOrganizationTypesSelectList(OrganizationType? selected = null)
    {
        return Enum.GetValues<OrganizationType>()
            .Select(ot => new SelectListItem
            {
                Value = ((int)ot).ToString(),
                Text = ot.ToDisplayString(),
                Selected = selected.HasValue && selected.Value == ot
            }).ToList();
    }

    public async Task<List<SelectListItem>> GetServiceAreasSelectListAsync(string? selected = null)
    {
        return await _db.ServiceAreas
            .Where(sa => sa.IsActive)
            .OrderBy(sa => sa.DisplayOrder).ThenBy(sa => sa.Code)
            .Select(sa => new SelectListItem
            {
                Value = sa.Id,
                Text = $"{sa.Code} - {sa.Name}",
                Selected = sa.Id == selected
            }).ToListAsync();
    }

    public async Task<List<ServiceAreaOption>> GetAvailableServiceAreasAsync()
    {
        return await _db.ServiceAreas
            .Where(sa => sa.IsActive)
            .OrderBy(sa => sa.DisplayOrder).ThenBy(sa => sa.Code)
            .Select(sa => new ServiceAreaOption { Id = sa.Id, Code = sa.Code, Name = sa.Name })
            .ToListAsync();
    }

    public async Task<List<SkillGroupViewModel>> GetSkillsGroupedByServiceAreaAsync(string? resourceId = null, List<string>? memberServiceAreaIds = null)
    {
        var selectedSkillIds = new List<string>();
        if (!string.IsNullOrEmpty(resourceId))
            selectedSkillIds = await GetResourceSkillIdsAsync(resourceId);

        var serviceAreas = await _db.ServiceAreas.Where(sa => sa.IsActive).OrderBy(sa => sa.DisplayOrder).ThenBy(sa => sa.Code).ToListAsync();
        var skills = await _db.Skills.Where(s => s.IsActive).OrderBy(s => s.Name).ToListAsync();

        return serviceAreas.Select(sa => new SkillGroupViewModel
        {
            ServiceAreaId = sa.Id,
            ServiceAreaCode = sa.Code,
            ServiceAreaName = sa.Name,
            IsMember = memberServiceAreaIds?.Contains(sa.Id) ?? false,
            Skills = skills.Where(s => s.ServiceAreaId == sa.Id)
                .Select(s => new SkillOptionViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    IsSelected = selectedSkillIds.Contains(s.Id)
                }).ToList()
        }).ToList();
    }

    #endregion

    #region Permission Queries

    public async Task<List<Resource>> GetResourcesForColumnAsync(string serviceAreaId, string columnName)
    {
        var allowedOrgTypes = EnhancementColumnRules.GetAllowedOrgTypes(columnName);
        if (allowedOrgTypes.Length == 0) return new List<Resource>();

        return await _db.Resources
            .Include(r => r.ServiceAreas)
            .Where(r => r.IsActive && allowedOrgTypes.Contains(r.OrganizationType) && r.ServiceAreas.Any(sa => sa.ServiceAreaId == serviceAreaId))
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<bool> HasPermissionAsync(string resourceId, string serviceAreaId, Permissions permission)
    {
        var m = await _db.ResourceServiceAreas.FirstOrDefaultAsync(rsa => rsa.ResourceId == resourceId && rsa.ServiceAreaId == serviceAreaId);
        return m?.HasPermission(permission) ?? false;
    }

    #endregion
}
