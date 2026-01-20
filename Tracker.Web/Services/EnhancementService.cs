using Microsoft.EntityFrameworkCore;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

public class EnhancementService : IEnhancementService
{
    private readonly TrackerDbContext _db;

    public EnhancementService(TrackerDbContext db)
    {
        _db = db;
    }

    public async Task<List<Enhancement>> GetByServiceAreaAsync(string serviceAreaId, string? statusFilter = null, string? searchTerm = null)
    {
        var filter = new EnhancementFilterViewModel
        {
            Search = searchTerm,
            Statuses = statusFilter?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>()
        };
        var result = await GetByServiceAreaPagedAsync(serviceAreaId, filter);
        return result.Items;
    }

    public async Task<PagedResult<Enhancement>> GetByServiceAreaPagedAsync(string serviceAreaId, EnhancementFilterViewModel filter)
    {
        var query = _db.Enhancements
            .Include(e => e.ServiceArea)
            .Include(e => e.Sponsors).ThenInclude(s => s.Resource)
            .Include(e => e.Spocs).ThenInclude(s => s.Resource)
            .Include(e => e.Resources).ThenInclude(r => r.Resource)
            .Include(e => e.Contacts).ThenInclude(c => c.Resource)
            .Where(e => e.ServiceAreaId == serviceAreaId);

        // Text search
        if (!string.IsNullOrEmpty(filter.Search))
        {
            var term = filter.Search.ToLower();
            query = query.Where(e => 
                e.WorkId.ToLower().Contains(term) || 
                e.Description.ToLower().Contains(term) ||
                (e.Notes != null && e.Notes.ToLower().Contains(term)));
        }

        // Multi-select filters
        if (filter.Statuses.Any())
            query = query.Where(e => e.Status != null && filter.Statuses.Contains(e.Status));

        if (filter.InfStatuses.Any())
            query = query.Where(e => e.InfStatus != null && filter.InfStatuses.Contains(e.InfStatus));

        if (filter.ServiceLines.Any())
            query = query.Where(e => e.ServiceLine != null && filter.ServiceLines.Contains(e.ServiceLine));

        // Numeric range filters
        if (filter.EstimatedHoursMin.HasValue)
            query = query.Where(e => e.EstimatedHours >= filter.EstimatedHoursMin.Value);
        if (filter.EstimatedHoursMax.HasValue)
            query = query.Where(e => e.EstimatedHours <= filter.EstimatedHoursMax.Value);

        if (filter.ReturnedHoursMin.HasValue)
            query = query.Where(e => e.ReturnedHours >= filter.ReturnedHoursMin.Value);
        if (filter.ReturnedHoursMax.HasValue)
            query = query.Where(e => e.ReturnedHours <= filter.ReturnedHoursMax.Value);

        // Date range filters
        if (filter.EstimatedStartDateFrom.HasValue)
            query = query.Where(e => e.EstimatedStartDate >= filter.EstimatedStartDateFrom.Value);
        if (filter.EstimatedStartDateTo.HasValue)
            query = query.Where(e => e.EstimatedStartDate <= filter.EstimatedStartDateTo.Value);

        if (filter.EstimatedEndDateFrom.HasValue)
            query = query.Where(e => e.EstimatedEndDate >= filter.EstimatedEndDateFrom.Value);
        if (filter.EstimatedEndDateTo.HasValue)
            query = query.Where(e => e.EstimatedEndDate <= filter.EstimatedEndDateTo.Value);

        if (filter.StartDateFrom.HasValue)
            query = query.Where(e => e.StartDate >= filter.StartDateFrom.Value);
        if (filter.StartDateTo.HasValue)
            query = query.Where(e => e.StartDate <= filter.StartDateTo.Value);

        if (filter.EndDateFrom.HasValue)
            query = query.Where(e => e.EndDate >= filter.EndDateFrom.Value);
        if (filter.EndDateTo.HasValue)
            query = query.Where(e => e.EndDate <= filter.EndDateTo.Value);

        if (filter.CreatedAtFrom.HasValue)
            query = query.Where(e => e.CreatedAt >= filter.CreatedAtFrom.Value);
        if (filter.CreatedAtTo.HasValue)
            query = query.Where(e => e.CreatedAt <= filter.CreatedAtTo.Value);

        if (filter.ModifiedAtFrom.HasValue)
            query = query.Where(e => e.ModifiedAt >= filter.ModifiedAtFrom.Value);
        if (filter.ModifiedAtTo.HasValue)
            query = query.Where(e => e.ModifiedAt <= filter.ModifiedAtTo.Value);

        // Get total count before paging
        var totalCount = await query.CountAsync();

        // Apply sorting
        var sortColumn = filter.SortColumn ?? "workId";
        var sortAsc = filter.SortOrder?.ToLower() != "desc";

        query = sortColumn.ToLower() switch
        {
            "workiddescription" => sortAsc ? query.OrderBy(e => e.WorkId) : query.OrderByDescending(e => e.WorkId),
            "workid" => sortAsc ? query.OrderBy(e => e.WorkId) : query.OrderByDescending(e => e.WorkId),
            "description" => sortAsc ? query.OrderBy(e => e.Description) : query.OrderByDescending(e => e.Description),
            "status" => sortAsc ? query.OrderBy(e => e.Status) : query.OrderByDescending(e => e.Status),
            "estimatedhours" => sortAsc ? query.OrderBy(e => e.EstimatedHours) : query.OrderByDescending(e => e.EstimatedHours),
            "estimatedstartdate" => sortAsc ? query.OrderBy(e => e.EstimatedStartDate) : query.OrderByDescending(e => e.EstimatedStartDate),
            "estimatedenddate" => sortAsc ? query.OrderBy(e => e.EstimatedEndDate) : query.OrderByDescending(e => e.EstimatedEndDate),
            "returnedhours" => sortAsc ? query.OrderBy(e => e.ReturnedHours) : query.OrderByDescending(e => e.ReturnedHours),
            "startdate" => sortAsc ? query.OrderBy(e => e.StartDate) : query.OrderByDescending(e => e.StartDate),
            "enddate" => sortAsc ? query.OrderBy(e => e.EndDate) : query.OrderByDescending(e => e.EndDate),
            "infstatus" => sortAsc ? query.OrderBy(e => e.InfStatus) : query.OrderByDescending(e => e.InfStatus),
            "serviceline" => sortAsc ? query.OrderBy(e => e.ServiceLine) : query.OrderByDescending(e => e.ServiceLine),
            "infserviceline" => sortAsc ? query.OrderBy(e => e.InfServiceLine) : query.OrderByDescending(e => e.InfServiceLine),
            "notes" => sortAsc ? query.OrderBy(e => e.Notes) : query.OrderByDescending(e => e.Notes),
            "createdat" => sortAsc ? query.OrderBy(e => e.CreatedAt) : query.OrderByDescending(e => e.CreatedAt),
            "createdby" => sortAsc ? query.OrderBy(e => e.CreatedBy) : query.OrderByDescending(e => e.CreatedBy),
            "modifiedat" => sortAsc ? query.OrderBy(e => e.ModifiedAt) : query.OrderByDescending(e => e.ModifiedAt),
            "modifiedby" => sortAsc ? query.OrderBy(e => e.ModifiedBy) : query.OrderByDescending(e => e.ModifiedBy),
            _ => sortAsc ? query.OrderBy(e => e.WorkId) : query.OrderByDescending(e => e.WorkId)
        };

        // Apply paging
        var page = Math.Max(1, filter.Page);
        var pageSize = filter.PageSize > 0 ? filter.PageSize : 25;
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Enhancement>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Enhancement?> GetByIdAsync(string id)
    {
        return await _db.Enhancements
            .Include(e => e.ServiceArea)
            .Include(e => e.EstimationBreakdown)
            .Include(e => e.Sponsors).ThenInclude(s => s.Resource)
            .Include(e => e.Spocs).ThenInclude(s => s.Resource)
            .Include(e => e.Resources).ThenInclude(r => r.Resource)
            .Include(e => e.Contacts).ThenInclude(c => c.Resource)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Enhancement?> GetByWorkIdAsync(string workId)
    {
        return await _db.Enhancements
            .Include(e => e.ServiceArea)
            .FirstOrDefaultAsync(e => e.WorkId.ToLower() == workId.ToLower());
    }

    public async Task<List<Enhancement>> FindMatchesAsync(string workId, string? description, string serviceAreaId)
    {
        var query = _db.Enhancements
            .Where(e => e.ServiceAreaId == serviceAreaId);

        var exactMatch = await query
            .Where(e => e.WorkId.ToLower() == workId.ToLower())
            .ToListAsync();

        if (exactMatch.Any())
            return exactMatch;

        var partialMatches = await query
            .Where(e => e.WorkId.ToLower().Contains(workId.ToLower()) ||
                       (description != null && e.Description.ToLower().Contains(description.ToLower())))
            .Take(5)
            .ToListAsync();

        return partialMatches;
    }

    public async Task<Enhancement> CreateAsync(Enhancement enhancement, string userId)
    {
        enhancement.CreatedBy = userId;
        enhancement.CreatedAt = DateTime.UtcNow;

        _db.Enhancements.Add(enhancement);
        await _db.SaveChangesAsync();

        await LogHistoryAsync(enhancement, "Insert", userId);

        return enhancement;
    }

    public async Task<Enhancement?> UpdateAsync(Enhancement enhancement, string userId)
    {
        var existing = await _db.Enhancements.FindAsync(enhancement.Id);
        if (existing == null)
            return null;

        existing.WorkId = enhancement.WorkId;
        existing.Description = enhancement.Description;
        existing.Notes = enhancement.Notes;
        existing.ServiceAreaId = enhancement.ServiceAreaId;
        existing.EstimatedHours = enhancement.EstimatedHours;
        existing.EstimatedStartDate = enhancement.EstimatedStartDate;
        existing.EstimatedEndDate = enhancement.EstimatedEndDate;
        existing.EstimationNotes = enhancement.EstimationNotes;
        existing.Status = enhancement.Status;
        existing.ServiceLine = enhancement.ServiceLine;
        existing.ReturnedHours = enhancement.ReturnedHours;
        existing.StartDate = enhancement.StartDate;
        existing.EndDate = enhancement.EndDate;
        existing.InfStatus = enhancement.InfStatus;
        existing.InfServiceLine = enhancement.InfServiceLine;
        existing.TimeW1 = enhancement.TimeW1;
        existing.TimeW2 = enhancement.TimeW2;
        existing.TimeW3 = enhancement.TimeW3;
        existing.TimeW4 = enhancement.TimeW4;
        existing.TimeW5 = enhancement.TimeW5;
        existing.TimeW6 = enhancement.TimeW6;
        existing.TimeW7 = enhancement.TimeW7;
        existing.TimeW8 = enhancement.TimeW8;
        existing.TimeW9 = enhancement.TimeW9;
        existing.ModifiedBy = userId;
        existing.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await LogHistoryAsync(existing, "Update", userId);

        return existing;
    }

    public async Task<bool> DeleteAsync(string id, string userId)
    {
        var enhancement = await _db.Enhancements.FindAsync(id);
        if (enhancement == null)
            return false;

        await LogHistoryAsync(enhancement, "Delete", userId);

        _db.Enhancements.Remove(enhancement);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BulkUpdateStatusAsync(List<string> ids, string status, string userId)
    {
        var enhancements = await _db.Enhancements
            .Where(e => ids.Contains(e.Id))
            .ToListAsync();

        foreach (var enhancement in enhancements)
        {
            enhancement.Status = status;
            enhancement.ModifiedBy = userId;
            enhancement.ModifiedAt = DateTime.UtcNow;
            await LogHistoryAsync(enhancement, "Update", userId);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> BulkUpdateAsync(BulkUpdateRequest request, string userId)
    {
        var enhancements = await _db.Enhancements
            .Include(e => e.Sponsors)
            .Include(e => e.Spocs)
            .Include(e => e.Resources)
            .Include(e => e.Contacts)
            .Where(e => request.SelectedIds.Contains(e.Id))
            .ToListAsync();

        foreach (var enhancement in enhancements)
        {
            // Update simple fields if provided
            if (!string.IsNullOrEmpty(request.Status))
                enhancement.Status = request.Status;
            
            if (!string.IsNullOrEmpty(request.InfStatus))
                enhancement.InfStatus = request.InfStatus;
            
            if (!string.IsNullOrEmpty(request.ServiceLine))
                enhancement.ServiceLine = request.ServiceLine;

            // Update dates (or clear them)
            if (request.ClearStartDate)
                enhancement.StartDate = null;
            else if (request.StartDate.HasValue)
                enhancement.StartDate = request.StartDate;

            if (request.ClearEndDate)
                enhancement.EndDate = null;
            else if (request.EndDate.HasValue)
                enhancement.EndDate = request.EndDate;

            if (request.ClearEstimatedStartDate)
                enhancement.EstimatedStartDate = null;
            else if (request.EstimatedStartDate.HasValue)
                enhancement.EstimatedStartDate = request.EstimatedStartDate;

            if (request.ClearEstimatedEndDate)
                enhancement.EstimatedEndDate = null;
            else if (request.EstimatedEndDate.HasValue)
                enhancement.EstimatedEndDate = request.EstimatedEndDate;

            // Update Sponsors (Client resources)
            if (request.ClearSponsors)
            {
                enhancement.Sponsors.Clear();
            }
            else if (request.SponsorIds != null && request.SponsorIds.Any())
            {
                enhancement.Sponsors.Clear();
                foreach (var sponsorId in request.SponsorIds)
                {
                    enhancement.Sponsors.Add(new EnhancementSponsor
                    {
                        EnhancementId = enhancement.Id,
                        ResourceId = sponsorId
                    });
                }
            }

            // Update SPOCs
            if (request.ClearSpocs)
            {
                enhancement.Spocs.Clear();
            }
            else if (request.SpocIds != null && request.SpocIds.Any())
            {
                enhancement.Spocs.Clear();
                foreach (var spocId in request.SpocIds)
                {
                    enhancement.Spocs.Add(new EnhancementSpoc
                    {
                        EnhancementId = enhancement.Id,
                        ResourceId = spocId
                    });
                }
            }

            // Update Resources (Internal)
            if (request.ClearResources)
            {
                enhancement.Resources.Clear();
            }
            else if (request.ResourceIds != null && request.ResourceIds.Any())
            {
                enhancement.Resources.Clear();
                foreach (var resourceId in request.ResourceIds)
                {
                    enhancement.Resources.Add(new EnhancementResource
                    {
                        EnhancementId = enhancement.Id,
                        ResourceId = resourceId
                    });
                }
            }

            // Legacy: Update contacts
            if (request.ClearContacts)
            {
                enhancement.Contacts.Clear();
            }
            else if (request.ContactIds != null && request.ContactIds.Any())
            {
                enhancement.Contacts.Clear();
                foreach (var contactId in request.ContactIds)
                {
                    enhancement.Contacts.Add(new EnhancementContact
                    {
                        EnhancementId = enhancement.Id,
                        ResourceId = contactId
                    });
                }
            }

            enhancement.ModifiedBy = userId;
            enhancement.ModifiedAt = DateTime.UtcNow;
            await LogHistoryAsync(enhancement, "BulkUpdate", userId);
        }

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<EstimationBreakdown?> GetBreakdownAsync(string enhancementId)
    {
        return await _db.EstimationBreakdowns
            .FirstOrDefaultAsync(eb => eb.EnhancementId == enhancementId);
    }

    public async Task<EstimationBreakdown> SaveBreakdownAsync(EstimationBreakdown breakdown)
    {
        var existing = await _db.EstimationBreakdowns
            .FirstOrDefaultAsync(eb => eb.EnhancementId == breakdown.EnhancementId);

        if (existing == null)
        {
            _db.EstimationBreakdowns.Add(breakdown);
        }
        else
        {
            existing.RequirementsAndEstimation = breakdown.RequirementsAndEstimation;
            existing.RequirementsAndEstimationNotes = breakdown.RequirementsAndEstimationNotes;
            existing.VendorCoordination = breakdown.VendorCoordination;
            existing.VendorCoordinationNotes = breakdown.VendorCoordinationNotes;
            existing.DesignFunctionalTechnical = breakdown.DesignFunctionalTechnical;
            existing.DesignFunctionalTechnicalNotes = breakdown.DesignFunctionalTechnicalNotes;
            existing.TestingSTI = breakdown.TestingSTI;
            existing.TestingSTINotes = breakdown.TestingSTINotes;
            existing.TestingUAT = breakdown.TestingUAT;
            existing.TestingUATNotes = breakdown.TestingUATNotes;
            existing.GoLiveDeploymentValidation = breakdown.GoLiveDeploymentValidation;
            existing.GoLiveDeploymentValidationNotes = breakdown.GoLiveDeploymentValidationNotes;
            existing.Hypercare = breakdown.Hypercare;
            existing.HypercareNotes = breakdown.HypercareNotes;
            existing.Documentation = breakdown.Documentation;
            existing.DocumentationNotes = breakdown.DocumentationNotes;
            existing.PMLead = breakdown.PMLead;
            existing.PMLeadNotes = breakdown.PMLeadNotes;
            existing.Contingency = breakdown.Contingency;
            existing.ContingencyNotes = breakdown.ContingencyNotes;
            breakdown = existing;
        }

        await _db.SaveChangesAsync();

        var enhancement = await _db.Enhancements.FindAsync(breakdown.EnhancementId);
        if (enhancement != null)
        {
            enhancement.EstimatedHours = breakdown.TotalHours;
            await _db.SaveChangesAsync();
        }

        return breakdown;
    }

    public async Task<List<string>> GetDistinctStatusesAsync()
    {
        return await _db.Enhancements
            .Where(e => e.Status != null)
            .Select(e => e.Status!)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
    }

    private async Task LogHistoryAsync(Enhancement enhancement, string action, string userId)
    {
        var history = new EnhancementHistory
        {
            AuditAction = action,
            AuditAt = DateTime.UtcNow,
            AuditBy = userId,
            EnhancementId = enhancement.Id,
            WorkId = enhancement.WorkId,
            Description = enhancement.Description,
            Notes = enhancement.Notes,
            ServiceAreaId = enhancement.ServiceAreaId,
            EstimatedHours = enhancement.EstimatedHours,
            EstimatedStartDate = enhancement.EstimatedStartDate,
            EstimatedEndDate = enhancement.EstimatedEndDate,
            EstimationNotes = enhancement.EstimationNotes,
            Status = enhancement.Status,
            ServiceLine = enhancement.ServiceLine,
            ReturnedHours = enhancement.ReturnedHours,
            StartDate = enhancement.StartDate,
            EndDate = enhancement.EndDate,
            InfStatus = enhancement.InfStatus,
            InfServiceLine = enhancement.InfServiceLine,
            TimeW1 = enhancement.TimeW1,
            TimeW2 = enhancement.TimeW2,
            TimeW3 = enhancement.TimeW3,
            TimeW4 = enhancement.TimeW4,
            TimeW5 = enhancement.TimeW5,
            TimeW6 = enhancement.TimeW6,
            TimeW7 = enhancement.TimeW7,
            TimeW8 = enhancement.TimeW8,
            TimeW9 = enhancement.TimeW9,
            CreatedBy = enhancement.CreatedBy,
            CreatedAt = enhancement.CreatedAt,
            ModifiedBy = enhancement.ModifiedBy,
            ModifiedAt = enhancement.ModifiedAt
        };

        _db.EnhancementHistory.Add(history);
        await _db.SaveChangesAsync();
    }
}
