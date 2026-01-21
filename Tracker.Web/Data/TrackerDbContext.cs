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
    public DbSet<EnhancementSponsor> EnhancementSponsors => Set<EnhancementSponsor>();
    public DbSet<EnhancementSpoc> EnhancementSpocs => Set<EnhancementSpoc>();
    public DbSet<EnhancementHistory> EnhancementHistory => Set<EnhancementHistory>();
    public DbSet<SavedFilter> SavedFilters => Set<SavedFilter>();
    public DbSet<UserColumnPreference> UserColumnPreferences => Set<UserColumnPreference>();
    public DbSet<NamedReport> NamedReports => Set<NamedReport>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<ResourceSkill> ResourceSkills => Set<ResourceSkill>();
    public DbSet<ResourceTypeLookup> ResourceTypeLookups => Set<ResourceTypeLookup>();
    public DbSet<EnhancementSkill> EnhancementSkills => Set<EnhancementSkill>();

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
            entity.HasOne(e => e.ResourceType)
                .WithMany(rt => rt.Resources)
                .HasForeignKey(e => e.ResourceTypeId)
                .OnDelete(DeleteBehavior.SetNull);
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

            // Ignore computed display properties
            entity.Ignore(e => e.SponsorsDisplay);
            entity.Ignore(e => e.SpocsDisplay);
            entity.Ignore(e => e.ResourcesDisplay);

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

        // EnhancementContact (junction) - Legacy
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

        // EnhancementSponsor (junction) - Client sponsors
        modelBuilder.Entity<EnhancementSponsor>(entity =>
        {
            entity.HasKey(e => new { e.EnhancementId, e.ResourceId });

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.Sponsors)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Resource)
                .WithMany(r => r.EnhancementSponsors)
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EnhancementSpoc (junction) - Infy SPOC
        modelBuilder.Entity<EnhancementSpoc>(entity =>
        {
            entity.HasKey(e => new { e.EnhancementId, e.ResourceId });

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.Spocs)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Resource)
                .WithMany(r => r.EnhancementSpocs)
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

        // NamedReport
        modelBuilder.Entity<NamedReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ServiceAreaIdsJson).IsRequired().HasDefaultValue("[]");
            entity.Property(e => e.FilterJson).IsRequired().HasDefaultValue("{}");
            entity.Property(e => e.ColumnsJson).IsRequired().HasDefaultValue("[]");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ResourceTypeLookup
        modelBuilder.Entity<ResourceTypeLookup>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Skill
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.ServiceArea)
                .WithMany()
                .HasForeignKey(e => e.ServiceAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ResourceSkill (junction)
        modelBuilder.Entity<ResourceSkill>(entity =>
        {
            entity.HasKey(e => new { e.ResourceId, e.SkillId });
            entity.HasOne(e => e.Resource)
                .WithMany(r => r.Skills)
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Skill)
                .WithMany(s => s.ResourceSkills)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EnhancementSkill (junction)
        modelBuilder.Entity<EnhancementSkill>(entity =>
        {
            entity.HasKey(e => new { e.EnhancementId, e.SkillId });
            entity.HasOne(e => e.Enhancement)
                .WithMany()
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Skill)
                .WithMany(s => s.EnhancementSkills)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
