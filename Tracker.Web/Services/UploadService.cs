using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Tracker.Web.Data;
using Tracker.Web.Entities;
using Tracker.Web.Services.Interfaces;
using Tracker.Web.ViewModels;

namespace Tracker.Web.Services;

public class UploadService : IUploadService
{
    private readonly TrackerDbContext _db;
    private readonly IEnhancementService _enhancementService;

    public UploadService(TrackerDbContext db, IEnhancementService enhancementService)
    {
        _db = db;
        _enhancementService = enhancementService;
    }

    public async Task<UploadPreviewResult> ParseCsvAsync(Stream fileStream, string serviceAreaId)
    {
        var result = new UploadPreviewResult();

        try
        {
            using var reader = new StreamReader(fileStream);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                PrepareHeaderForMatch = args => args.Header.ToLower().Replace("_", "").Replace(" ", "")
            };

            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<CsvUploadRow>().ToList();

            foreach (var record in records)
            {
                var row = new UploadRowViewModel
                {
                    WorkId = record.WorkId?.Trim() ?? string.Empty,
                    Description = record.Description?.Trim() ?? string.Empty,
                    Notes = record.Notes?.Trim()
                };

                // Find potential matches within the same service area
                if (!string.IsNullOrEmpty(row.WorkId))
                {
                    var matches = await _enhancementService.FindMatchesAsync(row.WorkId, row.Description, serviceAreaId);
                    if (matches.Any())
                    {
                        row.HasMatch = true;
                        row.MatchedEnhancementId = matches.First().Id;
                        row.MatchInfo = $"Match: {matches.First().WorkId} - {matches.First().Description}";
                    }
                }

                result.Rows.Add(row);
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Error parsing CSV: {ex.Message}";
        }

        return result;
    }

    public async Task<int> ImportEnhancementsAsync(List<UploadRowViewModel> rows, string serviceAreaId, string userId)
    {
        var imported = 0;

        foreach (var row in rows.Where(r => r.ShouldImport && !string.IsNullOrEmpty(r.WorkId)))
        {
            var enhancement = new Enhancement
            {
                WorkId = row.WorkId,
                Description = row.Description,
                Notes = row.Notes,
                ServiceAreaId = serviceAreaId,
                Status = "New"
            };

            await _enhancementService.CreateAsync(enhancement, userId);
            imported++;
        }

        return imported;
    }
}

public class CsvUploadRow
{
    public string? WorkId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}
