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

        // Seed ServiceAreas
        if (!await context.ServiceAreas.AnyAsync())
        {
            var serviceAreas = new List<ServiceArea>
            {
                new() { Id = "sa-infra", Name = "Infrastructure", Code = "INFRA", DisplayOrder = 1 },
                new() { Id = "sa-apps", Name = "Applications", Code = "APPS", DisplayOrder = 2 },
                new() { Id = "sa-security", Name = "Security", Code = "SEC", DisplayOrder = 3 },
                new() { Id = "sa-data", Name = "Data Services", Code = "DATA", DisplayOrder = 4 }
            };
            context.ServiceAreas.AddRange(serviceAreas);
            await context.SaveChangesAsync();
        }

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

        // Seed Resources with new types
        if (!await context.Resources.AnyAsync())
        {
            var resources = new List<Resource>
            {
                // Client sponsors
                new() { Name = "Sarah Wilson", Email = "sarah.wilson@client.com", Type = ResourceType.Client },
                new() { Name = "Tom Brown", Email = "tom.brown@client.com", Type = ResourceType.Client },
                // SPOC resources
                new() { Name = "Raj Kumar", Email = "raj.kumar@infosys.com", Type = ResourceType.SPOC },
                new() { Name = "Priya Sharma", Email = "priya.sharma@infosys.com", Type = ResourceType.SPOC },
                // Internal resources
                new() { Name = "John Smith", Email = "john.smith@infosys.com", Type = ResourceType.Internal },
                new() { Name = "Jane Doe", Email = "jane.doe@infosys.com", Type = ResourceType.Internal },
                new() { Name = "Mike Johnson", Email = "mike.johnson@infosys.com", Type = ResourceType.Internal }
            };
            context.Resources.AddRange(resources);
            await context.SaveChangesAsync();
        }
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
                "FilterJson TEXT NOT NULL DEFAULT '{{}}', " +
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
                "ColumnsJson TEXT NOT NULL DEFAULT '[]', " +
                "ModifiedAt TEXT NOT NULL, " +
                "FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE, " +
                "FOREIGN KEY (ServiceAreaId) REFERENCES ServiceAreas(Id) ON DELETE CASCADE)");
            
            await context.Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IX_UserColumnPreferences_UserId_ServiceAreaId ON UserColumnPreferences(UserId, ServiceAreaId)");
        }

        // Check if EnhancementSponsors table exists
        if (!await TableExistsAsync(context, "EnhancementSponsors"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE EnhancementSponsors (" +
                "EnhancementId TEXT NOT NULL, " +
                "ResourceId TEXT NOT NULL, " +
                "PRIMARY KEY (EnhancementId, ResourceId), " +
                "FOREIGN KEY (EnhancementId) REFERENCES Enhancements(Id) ON DELETE CASCADE, " +
                "FOREIGN KEY (ResourceId) REFERENCES Resources(Id) ON DELETE CASCADE)");
        }

        // Check if EnhancementSpocs table exists
        if (!await TableExistsAsync(context, "EnhancementSpocs"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE EnhancementSpocs (" +
                "EnhancementId TEXT NOT NULL, " +
                "ResourceId TEXT NOT NULL, " +
                "PRIMARY KEY (EnhancementId, ResourceId), " +
                "FOREIGN KEY (EnhancementId) REFERENCES Enhancements(Id) ON DELETE CASCADE, " +
                "FOREIGN KEY (ResourceId) REFERENCES Resources(Id) ON DELETE CASCADE)");
        }

        // Add Type column to Resources if it doesn't exist
        if (!await ColumnExistsAsync(context, "Resources", "Type"))
        {
            await context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Resources ADD COLUMN Type INTEGER NOT NULL DEFAULT 2"); // Default to Internal
        }
    }

    private static async Task MigrateResourceTypesAsync(TrackerDbContext context)
    {
        // Migrate old IsClientResource to new Type system
        // Client resources (IsClientResource = 1) -> Type = 0 (Client)
        // Non-client resources (IsClientResource = 0) -> Type = 2 (Internal)
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
