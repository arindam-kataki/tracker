using Microsoft.EntityFrameworkCore;
using Tracker.Web.Entities;

namespace Tracker.Web.Data;

public class TrackerDbContext : DbContext
{
    public TrackerDbContext(DbContextOptions<TrackerDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ServiceArea> ServiceAreas => Set<ServiceArea>();
    public DbSet<UserServiceArea> UserServiceAreas => Set<UserServiceArea>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Enhancement> Enhancements => Set<Enhancement>();
    public DbSet<EstimationBreakdown> EstimationBreakdowns => Set<EstimationBreakdown>();
    public DbSet<EnhancementContact> EnhancementContacts => Set<EnhancementContact>();
    public DbSet<EnhancementResource> EnhancementResources => Set<EnhancementResource>();
    public DbSet<EnhancementHistory> EnhancementHistory => Set<EnhancementHistory>();
    public DbSet<SavedFilter> SavedFilters => Set<SavedFilter>();
    public DbSet<UserColumnPreference> UserColumnPreferences => Set<UserColumnPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // ServiceArea
        modelBuilder.Entity<ServiceArea>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // UserServiceArea (junction)
        modelBuilder.Entity<UserServiceArea>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ServiceAreaId });
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.ServiceAreas)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ServiceArea)
                .WithMany(s => s.UserServiceAreas)
                .HasForeignKey(e => e.ServiceAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Resource
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
        });

        // Enhancement
        modelBuilder.Entity<Enhancement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WorkId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.ServiceLine).HasMaxLength(100);
            entity.Property(e => e.InfStatus).HasMaxLength(50);
            entity.Property(e => e.InfServiceLine).HasMaxLength(100);
            entity.Property(e => e.RowVersion).IsRowVersion();
            
            entity.HasIndex(e => e.WorkId);
            entity.HasIndex(e => e.ServiceAreaId);
            entity.HasIndex(e => e.Status);

            entity.HasOne(e => e.ServiceArea)
                .WithMany(s => s.Enhancements)
                .HasForeignKey(e => e.ServiceAreaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EstimationBreakdown (1:1 with Enhancement)
        modelBuilder.Entity<EstimationBreakdown>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EnhancementId).IsUnique();
            
            entity.HasOne(e => e.Enhancement)
                .WithOne(e => e.EstimationBreakdown)
                .HasForeignKey<EstimationBreakdown>(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Ignore(e => e.TotalHours);
        });

        // EnhancementContact (junction)
        modelBuilder.Entity<EnhancementContact>(entity =>
        {
            entity.HasKey(e => new { e.EnhancementId, e.ResourceId });
            
            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.Contacts)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Resource)
                .WithMany(r => r.EnhancementContacts)
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EnhancementResource (junction)
        modelBuilder.Entity<EnhancementResource>(entity =>
        {
            entity.HasKey(e => new { e.EnhancementId, e.ResourceId });
            
            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.Resources)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Resource)
                .WithMany(r => r.EnhancementResources)
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EnhancementHistory
        modelBuilder.Entity<EnhancementHistory>(entity =>
        {
            entity.HasKey(e => e.AuditId);
            entity.Property(e => e.AuditAction).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.EnhancementId);
            entity.HasIndex(e => e.AuditAt);
        });

        // SavedFilter
        modelBuilder.Entity<SavedFilter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FilterJson).IsRequired();
            
            entity.HasIndex(e => new { e.UserId, e.ServiceAreaId });
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ServiceArea)
                .WithMany()
                .HasForeignKey(e => e.ServiceAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // UserColumnPreference
        modelBuilder.Entity<UserColumnPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ColumnsJson).IsRequired();
            
            entity.HasIndex(e => new { e.UserId, e.ServiceAreaId }).IsUnique();
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.ServiceArea)
                .WithMany()
                .HasForeignKey(e => e.ServiceAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
