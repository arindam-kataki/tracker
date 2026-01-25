using Microsoft.EntityFrameworkCore;
using Tracker.Web.Entities;

namespace Tracker.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(TrackerDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        
        // Enable WAL mode for better concurrent access
        await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");

        // Create new tables if they don't exist (for existing databases)
        await CreateNewTablesIfNeededAsync(context);
        
        // Migrate existing resources to new type system
        await MigrateResourceTypesAsync(context);

        // Seed SuperAdmin user
        if (!await context.Users.AnyAsync())
        {
            var adminUser = new User
            {
                Id = "user-admin",
                Email = "admin@tracker.local",
                DisplayName = "System Administrator",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = UserRole.SuperAdmin,
                IsActive = true
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // Seed Time Recording Categories (Business Areas)
        await SeedTimeRecordingCategoriesAsync(context);
    }

    private static async Task SeedTimeRecordingCategoriesAsync(TrackerDbContext context)
    {
        if (await context.TimeRecordingCategories.AnyAsync())
            return;

        var categories = new List<TimeRecordingCategory>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Custom Apps", Description = "Custom application development", DisplayOrder = 1, IsActive = true },
            new() { Id = Guid.NewGuid().ToString(), Name = "SCM", Description = "Supply Chain Management", DisplayOrder = 2, IsActive = true },
            new() { Id = Guid.NewGuid().ToString(), Name = "ERP", Description = "Enterprise Resource Planning", DisplayOrder = 3, IsActive = true },
            new() { Id = Guid.NewGuid().ToString(), Name = "MRP", Description = "Material Requirements Planning", DisplayOrder = 4, IsActive = true },
            new() { Id = Guid.NewGuid().ToString(), Name = "CRM", Description = "Customer Relationship Management", DisplayOrder = 5, IsActive = true },
            new() { Id = Guid.NewGuid().ToString(), Name = "BI/Analytics", Description = "Business Intelligence & Reporting", DisplayOrder = 6, IsActive = true },
            new() { Id = Guid.NewGuid().ToString(), Name = "Integration", Description = "API, ETL, system integrations", DisplayOrder = 7, IsActive = true },
            new() { Id = Guid.NewGuid().ToString(), Name = "Infrastructure", Description = "Servers, networks, cloud", DisplayOrder = 8, IsActive = true },
            new() { Id = Guid.NewGuid().ToString(), Name = "Security", Description = "Security & compliance work", DisplayOrder = 9, IsActive = true },
            new() { Id = Guid.NewGuid().ToString(), Name = "Other", Description = "Miscellaneous", DisplayOrder = 10, IsActive = true },
        };

        context.TimeRecordingCategories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task CreateNewTablesIfNeededAsync(TrackerDbContext context)
    {
        // Check if SavedFilters table exists
        if (!await TableExistsAsync(context, "SavedFilters"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE SavedFilters (" +
                "Id TEXT PRIMARY KEY, " +
                "UserId TEXT NOT NULL, " +
                "ServiceAreaId TEXT NOT NULL, " +
                "Name TEXT NOT NULL, " +
                "FilterJson TEXT NOT NULL, " +
                "IsDefault INTEGER NOT NULL DEFAULT 0, " +
                "CreatedAt TEXT NOT NULL, " +
                "ModifiedAt TEXT, " +
                "FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE, " +
                "FOREIGN KEY (ServiceAreaId) REFERENCES ServiceAreas(Id) ON DELETE CASCADE)");
            
            await context.Database.ExecuteSqlRawAsync(
                "CREATE INDEX IX_SavedFilters_UserId_ServiceAreaId ON SavedFilters(UserId, ServiceAreaId)");
        }

        // Check if UserColumnPreferences table exists
        if (!await TableExistsAsync(context, "UserColumnPreferences"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE UserColumnPreferences (" +
                "Id TEXT PRIMARY KEY, " +
                "UserId TEXT NOT NULL, " +
                "ServiceAreaId TEXT NOT NULL, " +
                "ColumnsJson TEXT NOT NULL, " +
                "ModifiedAt TEXT NOT NULL, " +
                "FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE, " +
                "FOREIGN KEY (ServiceAreaId) REFERENCES ServiceAreas(Id) ON DELETE CASCADE)");
            
            await context.Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IX_UserColumnPreferences_UserId_ServiceAreaId ON UserColumnPreferences(UserId, ServiceAreaId)");
        }

        // New tables for Enhancement Details feature
        
        // EnhancementNotes table
        if (!await TableExistsAsync(context, "EnhancementNotes"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE EnhancementNotes (" +
                "Id TEXT PRIMARY KEY, " +
                "EnhancementId TEXT NOT NULL, " +
                "NoteText TEXT NOT NULL, " +
                "CreatedBy TEXT, " +
                "CreatedAt TEXT NOT NULL, " +
                "ModifiedBy TEXT, " +
                "ModifiedAt TEXT, " +
                "FOREIGN KEY (EnhancementId) REFERENCES Enhancements(Id) ON DELETE CASCADE, " +
                "FOREIGN KEY (CreatedBy) REFERENCES Users(Id) ON DELETE SET NULL)");
            
            await context.Database.ExecuteSqlRawAsync(
                "CREATE INDEX IX_EnhancementNotes_EnhancementId ON EnhancementNotes(EnhancementId)");
            await context.Database.ExecuteSqlRawAsync(
                "CREATE INDEX IX_EnhancementNotes_CreatedAt ON EnhancementNotes(CreatedAt)");
        }

        // EnhancementAttachments table
        if (!await TableExistsAsync(context, "EnhancementAttachments"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE EnhancementAttachments (" +
                "Id TEXT PRIMARY KEY, " +
                "EnhancementId TEXT NOT NULL, " +
                "FileName TEXT NOT NULL, " +
                "StoredFileName TEXT NOT NULL, " +
                "ContentType TEXT NOT NULL, " +
                "FileSize INTEGER NOT NULL, " +
                "StoragePath TEXT NOT NULL, " +
                "UploadedBy TEXT, " +
                "UploadedAt TEXT NOT NULL, " +
                "FOREIGN KEY (EnhancementId) REFERENCES Enhancements(Id) ON DELETE CASCADE, " +
                "FOREIGN KEY (UploadedBy) REFERENCES Users(Id) ON DELETE SET NULL)");
            
            await context.Database.ExecuteSqlRawAsync(
                "CREATE INDEX IX_EnhancementAttachments_EnhancementId ON EnhancementAttachments(EnhancementId)");
        }

        // TimeRecordingCategories table
        if (!await TableExistsAsync(context, "TimeRecordingCategories"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE TimeRecordingCategories (" +
                "Id TEXT PRIMARY KEY, " +
                "Name TEXT NOT NULL, " +
                "Description TEXT, " +
                "DisplayOrder INTEGER NOT NULL DEFAULT 0, " +
                "IsActive INTEGER NOT NULL DEFAULT 1, " +
                "CreatedAt TEXT NOT NULL)");
            
            await context.Database.ExecuteSqlRawAsync(
                "CREATE INDEX IX_TimeRecordingCategories_DisplayOrder ON TimeRecordingCategories(DisplayOrder)");
        }

        // EnhancementTimeCategories table (junction)
        if (!await TableExistsAsync(context, "EnhancementTimeCategories"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE EnhancementTimeCategories (" +
                "EnhancementId TEXT NOT NULL, " +
                "TimeCategoryId TEXT NOT NULL, " +
                "DisplayOrder INTEGER NOT NULL DEFAULT 0, " +
                "PRIMARY KEY (EnhancementId, TimeCategoryId), " +
                "FOREIGN KEY (EnhancementId) REFERENCES Enhancements(Id) ON DELETE CASCADE, " +
                "FOREIGN KEY (TimeCategoryId) REFERENCES TimeRecordingCategories(Id) ON DELETE CASCADE)");
        }

        // EnhancementTimeEntries table
        if (!await TableExistsAsync(context, "EnhancementTimeEntries"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE EnhancementTimeEntries (" +
                "Id TEXT PRIMARY KEY, " +
                "EnhancementId TEXT NOT NULL, " +
                "PeriodStart TEXT NOT NULL, " +
                "PeriodEnd TEXT NOT NULL, " +
                "HoursJson TEXT NOT NULL, " +
                "Notes TEXT, " +
                "CreatedBy TEXT, " +
                "CreatedAt TEXT NOT NULL, " +
                "ModifiedBy TEXT, " +
                "ModifiedAt TEXT, " +
                "FOREIGN KEY (EnhancementId) REFERENCES Enhancements(Id) ON DELETE CASCADE)");
            
            await context.Database.ExecuteSqlRawAsync(
                "CREATE INDEX IX_EnhancementTimeEntries_EnhancementId ON EnhancementTimeEntries(EnhancementId)");
        }
    }

    private static async Task MigrateResourceTypesAsync(TrackerDbContext context)
    {
        // Migrate old IsClientResource to new Type system
        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "UPDATE Resources SET Type = 0 WHERE IsClientResource = 1 AND Type = 2");
        }
        catch
        {
            // Ignore if column doesn't exist
        }
    }

    private static async Task<bool> TableExistsAsync(TrackerDbContext context, string tableName)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'";
        var result = await command.ExecuteScalarAsync();
        
        return Convert.ToInt32(result) > 0;
    }

    private static async Task<bool> ColumnExistsAsync(TrackerDbContext context, string tableName, string columnName)
    {
        var connection = context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
            await connection.OpenAsync();
        
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({tableName})";
        
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (reader.GetString(1).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        
        return false;
    }
}
