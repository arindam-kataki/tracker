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

        // Seed Resources
        if (!await context.Resources.AnyAsync())
        {
            var resources = new List<Resource>
            {
                // Internal resources
                new() { Name = "John Smith", Email = "john.smith@company.com", IsClientResource = false },
                new() { Name = "Jane Doe", Email = "jane.doe@company.com", IsClientResource = false },
                new() { Name = "Mike Johnson", Email = "mike.johnson@company.com", IsClientResource = false },
                // Client/Sponsor resources
                new() { Name = "Sarah Wilson (Client)", Email = "sarah.wilson@client.com", IsClientResource = true },
                new() { Name = "Tom Brown (Sponsor)", Email = "tom.brown@client.com", IsClientResource = true }
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
}
