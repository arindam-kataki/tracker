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

    #region Resource CRUD

    public async Task<List<ResourceListItem>> GetAllAsync(string? search = null, string? typeFilter = null)
    {
        var query = BuildResourceQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(s) || (r.Email != null && r.Email.ToLower().Contains(s)));
        }

        if (!string.IsNullOrWhiteSpace(typeFilter))
            query = query.Where(r => r.ResourceTypeId == typeFilter);

        return await ProjectToListItems(query);
    }

    public async Task<List<ResourceListItem>> GetAllWithFiltersAsync(string? search, OrganizationType? orgType, string? serviceAreaId)
    {
        var query = BuildResourceQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(s) || (r.Email != null && r.Email.ToLower().Contains(s)));
        }

        if (orgType.HasValue)
            query = query.Where(r => r.OrganizationType == orgType.Value);

        if (!string.IsNullOrEmpty(serviceAreaId))
            query = query.Where(r => r.ServiceAreas.Any(sa => sa.ServiceAreaId == serviceAreaId));

        return await ProjectToListItems(query);
    }

    private IQueryable<Resource> BuildResourceQuery()
    {
        return _db.Resources
            .Include(r => r.ResourceType)
            .Include(r => r.Skills).ThenInclude(rs => rs.Skill)
            .Include(r => r.ServiceAreas).ThenInclude(rsa => rsa.ServiceArea)
            .AsQueryable();
    }

    private async Task<List<ResourceListItem>> ProjectToListItems(IQueryable<Resource> query)
    {
        var resources = await query
            .OrderBy(r => r.OrganizationType).ThenBy(r => r.Name)
            .ToListAsync();

        return resources.Select(r => new ResourceListItem
        {
            Id = r.Id,
            Name = r.Name,
            Email = r.Email,
            OrganizationType = r.OrganizationType,
            IsActive = r.IsActive,
            HasLoginAccess = r.HasLoginAccess,
            IsAdmin = r.IsAdmin,
            LastLoginAt = r.LastLoginAt,
            ResourceTypeId = r.ResourceTypeId,
            ResourceTypeName = r.ResourceType?.Name,
            SkillNames = r.Skills.Select(s => s.Skill.Name).ToList(),
            ServiceAreas = r.ServiceAreas.Select(sa => new ResourceServiceAreaSummary
            {
                ServiceAreaId = sa.ServiceAreaId,
                Code = sa.ServiceArea?.Code ?? "",
                Name = sa.ServiceArea?.Name ?? "",
                IsPrimary = sa.IsPrimary,
                Permissions = sa.Permissions
            }).ToList()
        }).ToList();
    }

    public async Task<Resource?> GetByIdAsync(string id)
    {
        return await _db.Resources
            .Include(r => r.ResourceType)
            .Include(r => r.Skills).ThenInclude(rs => rs.Skill)
            .Include(r => r.ServiceAreas).ThenInclude(rsa => rsa.ServiceArea)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<EditResourceViewModel?> GetForEditAsync(string id)
    {
        var resource = await GetByIdAsync(id);
        if (resource == null) return null;

        var isLastAdmin = resource.IsAdmin && await IsLastAdminAsync(id);

        var model = new EditResourceViewModel
        {
            Id = resource.Id,
            Name = resource.Name,
            Email = resource.Email,
            Phone = resource.Phone,
            OrganizationType = resource.OrganizationType,
            IsActive = resource.IsActive,
            HasLoginAccess = resource.HasLoginAccess,
            IsAdmin = resource.IsAdmin,
            CanConsolidate = resource.CanConsolidate,
            IsLastAdmin = isLastAdmin,
            LastLoginAt = resource.LastLoginAt,
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
                    return new ResourceOperationResult { Success = false, Errors = new List<string> { "A resource with this email already exists." } };
            }

            var resource = new Resource
            {
                Name = model.Name,
                Email = model.Email,
                Phone = model.Phone,
                OrganizationType = model.OrganizationType,
                IsActive = model.IsActive,
                HasLoginAccess = model.HasLoginAccess,
                IsAdmin = model.HasLoginAccess && model.IsAdmin,
                CanConsolidate = model.CanConsolidate,
                ResourceTypeId = model.ResourceTypeId,
                CreatedAt = DateTime.UtcNow
            };

            // If login access and password provided, hash it
            if (model.HasLoginAccess && !string.IsNullOrEmpty(model.NewPassword))
            {
                resource.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            _db.Resources.Add(resource);

            // Add service area memberships
            foreach (var sa in model.ServiceAreaMemberships ?? new List<EditResourceServiceAreaViewModel>())
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

            // Add skills
            var skillIds = model.SelectedSkillIds ?? model.SkillIds ?? new List<string>();
            foreach (var skillId in skillIds)
                _db.ResourceSkills.Add(new ResourceSkill { ResourceId = resource.Id, SkillId = skillId });

            await _db.SaveChangesAsync();
            _logger.LogInformation("Created resource {Name} with ID {Id}", resource.Name, resource.Id);

            return new ResourceOperationResult { Success = true, ResourceId = resource.Id, Message = "Resource created successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource {Name}", model.Name);
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred while creating the resource." } };
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

            // Check last admin protection
            if (resource.IsAdmin && !model.IsAdmin)
            {
                if (await IsLastAdminAsync(resource.Id))
                    return new ResourceOperationResult { Success = false, Errors = new List<string> { "Cannot remove admin access from the last administrator." } };
            }

            if (resource.IsAdmin && !model.IsActive)
            {
                if (await IsLastAdminAsync(resource.Id))
                    return new ResourceOperationResult { Success = false, Errors = new List<string> { "Cannot deactivate the last administrator." } };
            }

            // Email uniqueness check
            if (!string.IsNullOrEmpty(model.Email) && model.Email != resource.Email)
            {
                if (await _db.Resources.AnyAsync(r => r.Email == model.Email && r.Id != model.Id))
                    return new ResourceOperationResult { Success = false, Errors = new List<string> { "A resource with this email already exists." } };
            }

            resource.Name = model.Name;
            resource.Email = model.Email;
            resource.Phone = model.Phone;
            resource.OrganizationType = model.OrganizationType;
            resource.IsActive = model.IsActive;
            resource.HasLoginAccess = model.HasLoginAccess;
            resource.IsAdmin = model.HasLoginAccess && model.IsAdmin;
            resource.CanConsolidate = model.CanConsolidate;
            resource.ResourceTypeId = model.ResourceTypeId;
            resource.UpdatedAt = DateTime.UtcNow;

            // If disabling login, clear auth fields
            if (!model.HasLoginAccess)
            {
                resource.IsAdmin = false;
                // Don't clear password hash - in case they want to re-enable later
            }

            // If new password provided, update it
            if (model.HasLoginAccess && !string.IsNullOrEmpty(model.NewPassword))
            {
                resource.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            }

            // Update service area memberships
            _db.ResourceServiceAreas.RemoveRange(resource.ServiceAreas);
            foreach (var sa in model.ServiceAreaMemberships ?? new List<EditResourceServiceAreaViewModel>())
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

            // Update skills
            _db.ResourceSkills.RemoveRange(resource.Skills);
            var skillIds = model.SelectedSkillIds ?? model.SkillIds ?? new List<string>();
            foreach (var skillId in skillIds)
                _db.ResourceSkills.Add(new ResourceSkill { ResourceId = resource.Id, SkillId = skillId });

            await _db.SaveChangesAsync();
            _logger.LogInformation("Updated resource {Name} with ID {Id}", resource.Name, resource.Id);

            return new ResourceOperationResult { Success = true, ResourceId = resource.Id, Message = "Resource updated successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource {Id}", model.Id);
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred while updating the resource." } };
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

            // Last admin protection
            if (resource.IsAdmin && resource.IsActive)
            {
                if (await IsLastAdminAsync(id))
                    return new ResourceOperationResult { Success = false, Errors = new List<string> { "Cannot delete the last administrator." } };
            }

            _db.ResourceSkills.RemoveRange(resource.Skills);
            _db.ResourceServiceAreas.RemoveRange(resource.ServiceAreas);
            _db.Resources.Remove(resource);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Deleted resource {Name} with ID {Id}", resource.Name, id);
            return new ResourceOperationResult { Success = true, Message = "Resource deleted successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource {Id}", id);
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred while deleting the resource." } };
        }
    }

    #endregion

    #region Legacy Methods

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
        var resource = await _db.Resources.Include(r => r.Skills).Include(r => r.ServiceAreas).FirstOrDefaultAsync(r => r.Id == id);
        if (resource == null) return;

        _db.ResourceSkills.RemoveRange(resource.Skills);
        _db.ResourceServiceAreas.RemoveRange(resource.ServiceAreas);
        _db.Resources.Remove(resource);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Resource>> GetActiveAsync()
    {
        return await _db.Resources.Include(r => r.ResourceType).Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
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

    #region Authentication & Password Management

    public async Task<ResourceOperationResult> SetPasswordAsync(string resourceId, string password)
    {
        try
        {
            var resource = await _db.Resources.FindAsync(resourceId);
            if (resource == null)
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "Resource not found." } };

            if (string.IsNullOrEmpty(password) || password.Length < 8)
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "Password must be at least 8 characters." } };

            resource.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            resource.HasLoginAccess = true;
            await _db.SaveChangesAsync();

            return new ResourceOperationResult { Success = true, Message = "Password set successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting password for resource {Id}", resourceId);
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred." } };
        }
    }

    public async Task<ResourceOperationResult> ResetPasswordAsync(string resourceId, string newPassword)
    {
        return await SetPasswordAsync(resourceId, newPassword);
    }

    public async Task<ResourceOperationResult> ChangePasswordAsync(string resourceId, string currentPassword, string newPassword)
    {
        try
        {
            var resource = await _db.Resources.FindAsync(resourceId);
            if (resource == null)
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "Resource not found." } };

            if (string.IsNullOrEmpty(resource.PasswordHash) || !BCrypt.Net.BCrypt.Verify(currentPassword, resource.PasswordHash))
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "Current password is incorrect." } };

            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 8)
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "New password must be at least 8 characters." } };

            resource.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _db.SaveChangesAsync();

            return new ResourceOperationResult { Success = true, Message = "Password changed successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for resource {Id}", resourceId);
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred." } };
        }
    }

    public async Task<ResourceOperationResult> SetLoginAccessAsync(string resourceId, bool hasAccess)
    {
        try
        {
            var resource = await _db.Resources.FindAsync(resourceId);
            if (resource == null)
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "Resource not found." } };

            // If disabling login for admin, check last admin
            if (!hasAccess && resource.IsAdmin && resource.IsActive)
            {
                if (await IsLastAdminAsync(resourceId))
                    return new ResourceOperationResult { Success = false, Errors = new List<string> { "Cannot disable login for the last administrator." } };
            }

            resource.HasLoginAccess = hasAccess;
            if (!hasAccess)
                resource.IsAdmin = false;

            await _db.SaveChangesAsync();

            return new ResourceOperationResult { Success = true, Message = hasAccess ? "Login access enabled." : "Login access disabled." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting login access for resource {Id}", resourceId);
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred." } };
        }
    }

    public async Task<ResourceOperationResult> SetAdminStatusAsync(string resourceId, bool isAdmin)
    {
        try
        {
            var resource = await _db.Resources.FindAsync(resourceId);
            if (resource == null)
                return new ResourceOperationResult { Success = false, Errors = new List<string> { "Resource not found." } };

            // If removing admin, check last admin
            if (!isAdmin && resource.IsAdmin && resource.IsActive)
            {
                if (await IsLastAdminAsync(resourceId))
                    return new ResourceOperationResult { Success = false, Errors = new List<string> { "Cannot remove admin status from the last administrator." } };
            }

            // Can only be admin if has login access
            resource.IsAdmin = isAdmin && resource.HasLoginAccess;
            await _db.SaveChangesAsync();

            return new ResourceOperationResult { Success = true, Message = isAdmin ? "Admin status granted." : "Admin status removed." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting admin status for resource {Id}", resourceId);
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred." } };
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
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred." } };
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
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred." } };
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
            return new ResourceOperationResult { Success = false, Errors = new List<string> { "An error occurred." } };
        }
    }

    #endregion

    #region Skills

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
        return await _db.ResourceSkills.Where(rs => rs.ResourceId == resourceId).Select(rs => rs.SkillId).ToListAsync();
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

    #region Lookups

    public async Task<List<SelectListItem>> GetResourceTypesSelectListAsync()
    {
        return await _db.ResourceTypeLookups
            .Where(rt => rt.IsActive)
            .OrderBy(rt => rt.DisplayOrder).ThenBy(rt => rt.Name)
            .Select(rt => new SelectListItem { Value = rt.Id, Text = rt.Name })
            .ToListAsync();
    }

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

    #region Admin Queries

    public async Task<int> GetActiveAdminCountAsync()
    {
        return await _db.Resources.CountAsync(r => r.IsAdmin && r.IsActive && r.HasLoginAccess);
    }

    public async Task<bool> IsLastAdminAsync(string resourceId)
    {
        var adminCount = await _db.Resources.CountAsync(r => r.IsAdmin && r.IsActive && r.HasLoginAccess && r.Id != resourceId);
        return adminCount == 0;
    }

    #endregion


    /// <summary>
    /// Gets resources that can be selected as a manager for a given service area.
    /// Excludes the specified resource (can't report to yourself).
    /// Only includes active resources with membership in the specified service area.
    /// </summary>
    public async Task<List<SelectListItem>> GetPotentialManagersAsync(string serviceAreaId, string? excludeResourceId = null)
    {
        var query = _db.ResourceServiceAreas
            .Include(rsa => rsa.Resource)
            .Where(rsa => rsa.ServiceAreaId == serviceAreaId
                       && rsa.Resource.IsActive);

        if (!string.IsNullOrEmpty(excludeResourceId))
        {
            query = query.Where(rsa => rsa.ResourceId != excludeResourceId);
        }

        var managers = await query
            .OrderBy(rsa => rsa.Resource.Name)
            .Select(rsa => new SelectListItem
            {
                Value = rsa.ResourceId,
                Text = rsa.Resource.Name
            })
            .ToListAsync();

        // Add empty option at the beginning
        managers.Insert(0, new SelectListItem { Value = "", Text = "-- No Manager --" });

        return managers;
    }

    /// <summary>
    /// Validates that a ReportsTo assignment doesn't create a circular reference.
    /// Returns true if the assignment is valid (no circular reference).
    /// </summary>
    public async Task<bool> ValidateReportsToAsync(string resourceId, string serviceAreaId, string? reportsToResourceId)
    {
        if (string.IsNullOrEmpty(reportsToResourceId))
            return true; // No manager is always valid

        if (resourceId == reportsToResourceId)
            return false; // Can't report to yourself

        // Check for circular references by walking up the chain
        var visited = new HashSet<string> { resourceId };
        var currentManagerId = reportsToResourceId;

        while (!string.IsNullOrEmpty(currentManagerId))
        {
            if (visited.Contains(currentManagerId))
                return false; // Circular reference detected

            visited.Add(currentManagerId);

            // Get the manager's manager in this service area
            var managerMembership = await _db.ResourceServiceAreas
                .Where(rsa => rsa.ResourceId == currentManagerId && rsa.ServiceAreaId == serviceAreaId)
                .Select(rsa => rsa.ReportsToResourceId)
                .FirstOrDefaultAsync();

            currentManagerId = managerMembership;
        }

        return true;
    }


    /// <summary>
    /// Gets the direct reports for a resource within a service area.
    /// </summary>
    public async Task<List<ResourceListItem>> GetDirectReportsAsync(string resourceId, string serviceAreaId)
    {
        var directReports = await _db.ResourceServiceAreas
            .Include(rsa => rsa.Resource)
                .ThenInclude(r => r.ServiceAreas)
                    .ThenInclude(sa => sa.ServiceArea)
            .Where(rsa => rsa.ReportsToResourceId == resourceId
                       && rsa.ServiceAreaId == serviceAreaId
                       && rsa.Resource.IsActive)
            .OrderBy(rsa => rsa.Resource.Name)
            .Select(rsa => rsa.Resource)
            .ToListAsync();

        return directReports.Select(r => new ResourceListItem
        {
            Id = r.Id,
            Name = r.Name,
            Email = r.Email,
            OrganizationType = r.OrganizationType,
            IsActive = r.IsActive,
            HasLoginAccess = r.HasLoginAccess,
            IsAdmin = r.IsAdmin,
            LastLoginAt = r.LastLoginAt,
            ResourceTypeId = r.ResourceTypeId,
            ServiceAreas = r.ServiceAreas.Select(sa => new ResourceServiceAreaSummary
            {
                ServiceAreaId = sa.ServiceAreaId,
                Code = sa.ServiceArea?.Code ?? "",
                Name = sa.ServiceArea?.Name ?? "",
                IsPrimary = sa.IsPrimary,
                Permissions = sa.Permissions
            }).ToList()
        }).ToList();
    }


    /// <summary>
    /// Gets the full reporting chain (hierarchy) for a resource within a service area.
    /// Returns list from immediate manager up to top of hierarchy.
    /// </summary>
    public async Task<List<(string ResourceId, string Name)>> GetReportingChainAsync(string resourceId, string serviceAreaId)
    {
        var chain = new List<(string ResourceId, string Name)>();
        var visited = new HashSet<string> { resourceId };

        var membership = await _db.ResourceServiceAreas
            .Include(rsa => rsa.ReportsTo)
            .Where(rsa => rsa.ResourceId == resourceId && rsa.ServiceAreaId == serviceAreaId)
            .FirstOrDefaultAsync();

        while (membership?.ReportsToResourceId != null)
        {
            if (visited.Contains(membership.ReportsToResourceId))
                break; // Safety: break on circular reference

            visited.Add(membership.ReportsToResourceId);
            chain.Add((membership.ReportsToResourceId, membership.ReportsTo?.Name ?? "Unknown"));

            membership = await _db.ResourceServiceAreas
                .Include(rsa => rsa.ReportsTo)
                .Where(rsa => rsa.ResourceId == membership.ReportsToResourceId && rsa.ServiceAreaId == serviceAreaId)
                .FirstOrDefaultAsync();
        }

        return chain;
    }
}
