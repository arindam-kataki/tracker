using Microsoft.EntityFrameworkCore;
using Tracker.Web.Entities;

namespace Tracker.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(TrackerDbContext context)
    {
        await context.Database.EnsureCreatedAsync();
        
        // Enable WAL mode for better concurrent access (SQLite)
        try
        {
            await context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");
        }
        catch
        {
            // Ignore if not SQLite
        }

        // Create new tables if they don't exist (for existing databases)
        await CreateNewTablesIfNeededAsync(context);
        
        // Migrate existing resources to new type system
        await MigrateResourceTypesAsync(context);

        // Seed Admin Resource (replaces User seeding)
        await SeedAdminResourceAsync(context);

        // Seed Time Recording Categories (Business Areas)
        await SeedTimeRecordingCategoriesAsync(context);
    }

    /// <summary>
    /// Seeds the default administrator resource with login access.
    /// This replaces the old User seeding - Resources now handle authentication.
    /// </summary>
    private static async Task SeedAdminResourceAsync(TrackerDbContext context)
    {
        // Check if any admin resource exists
        if (await context.Resources.AnyAsync(r => r.IsAdmin && r.HasLoginAccess))
            return;

        // Check if there's an existing resource with admin email that needs upgrading
        var existingAdmin = await context.Resources
            .FirstOrDefaultAsync(r => r.Email != null && r.Email.ToLower() == "admin@tracker.local");

        if (existingAdmin != null)
        {
            // Upgrade existing resource to admin
            existingAdmin.HasLoginAccess = true;
            existingAdmin.IsAdmin = true;
            existingAdmin.CanConsolidate = true;
            if (string.IsNullOrEmpty(existingAdmin.PasswordHash))
            {
                existingAdmin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!");
            }
            await context.SaveChangesAsync();
            return;
        }

        // Create new admin resource
        var adminResource = new Resource
        {
            Id = "resource-admin-001",
            Name = "System Administrator",
            Email = "admin@tracker.local",
            OrganizationType = OrganizationType.Implementor,
            IsActive = true,
            
            // Authentication fields
            HasLoginAccess = true,
            IsAdmin = true,
            CanConsolidate = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            
            CreatedAt = DateTime.UtcNow
        };

        context.Resources.Add(adminResource);
        await context.SaveChangesAsync();

        Console.WriteLine("==============================================");
        Console.WriteLine("  DEFAULT ADMIN CREATED");
        Console.WriteLine("  Email: admin@tracker.local");
        Console.WriteLine("  Password: Admin123!");
        Console.WriteLine("  ** CHANGE THIS PASSWORD IMMEDIATELY **");
        Console.WriteLine("==============================================");
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
            new() { Id = Guid.NewGuid().ToString(), Name = "Other", Description = "Miscellaneous", DisplayOrder = 10, IsActive = true }
        };

        context.TimeRecordingCategories.AddRange(categories);
        await context.SaveChangesAsync();
    }

    private static async Task CreateNewTablesIfNeededAsync(TrackerDbContext context)
    {
        // This method creates tables that might not exist in older databases
        // EF will handle this automatically with migrations, but this is a safety net
        
        try
        {
            // Check if ResourceServiceAreas table exists by trying to query it
            _ = await context.ResourceServiceAreas.AnyAsync();
        }
        catch
        {
            // Table doesn't exist - it will be created by migration
            // Just log and continue
            Console.WriteLine("Note: ResourceServiceAreas table will be created by migration.");
        }
    }

    private static async Task MigrateResourceTypesAsync(TrackerDbContext context)
    {
        // Migrate resources from legacy ResourceType to OrganizationType
        var resourcesNeedingMigration = await context.Resources
            .Include(r => r.ResourceType)
            .Where(r => r.ResourceType != null && r.OrganizationType == OrganizationType.Implementor)
            .ToListAsync();

        if (!resourcesNeedingMigration.Any())
            return;

        foreach (var resource in resourcesNeedingMigration)
        {
            var typeName = resource.ResourceType?.Name?.ToLower() ?? "";
            
            if (typeName.Contains("client") || typeName.Contains("sponsor") || typeName.Contains("customer"))
            {
                resource.OrganizationType = OrganizationType.Client;
            }
            else if (typeName.Contains("vendor") || typeName.Contains("contractor") || typeName.Contains("external"))
            {
                resource.OrganizationType = OrganizationType.Vendor;
            }
            else
            {
                resource.OrganizationType = OrganizationType.Implementor;
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Migrated {resourcesNeedingMigration.Count} resources to new OrganizationType system.");
    }

    /// <summary>
    /// Migrates existing Users to Resources (one-time migration helper).
    /// Call this manually if you have existing Users that need to be converted.
    /// </summary>
    /// 
    /*
    public static async Task MigrateUsersToResourcesAsync(TrackerDbContext context)
    {
        // This is a helper method for manual migration if needed
        // It matches Users to Resources by email and copies auth fields
        
        var users = await context.Users.ToListAsync();
        
        foreach (var user in users)
        {
            var resource = await context.Resources
                .FirstOrDefaultAsync(r => r.Email != null && r.Email.ToLower() == user.Email.ToLower());

            if (resource != null)
            {
                // Update existing resource with user's auth info
                resource.HasLoginAccess = true;
                resource.PasswordHash = user.PasswordHash;
                resource.IsAdmin = user.Role == UserRole.SuperAdmin;
                resource.CanConsolidate = user.CanConsolidate;
                resource.LastLoginAt = user.LastLoginAt;
            }
            else
            {
                // Create new resource from user
                var newResource = new Resource
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = user.DisplayName,
                    Email = user.Email,
                    OrganizationType = OrganizationType.Implementor,
                    IsActive = user.IsActive,
                    HasLoginAccess = true,
                    PasswordHash = user.PasswordHash,
                    IsAdmin = user.Role == UserRole.SuperAdmin,
                    CanConsolidate = user.CanConsolidate,
                    LastLoginAt = user.LastLoginAt,
                    CreatedAt = user.CreatedAt
                };
                context.Resources.Add(newResource);
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"Migrated {users.Count} users to resources.");
    }

    */
}
