using Microsoft.EntityFrameworkCore;
using Tracker.Web.Entities;

namespace Tracker.Web.Data;

public class TrackerDbContext : DbContext
{
    public TrackerDbContext(DbContextOptions<TrackerDbContext> options) : base(options)
    {
    }

    // Existing DbSets
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

    // New DbSets for Enhancement Details
    public DbSet<Note> EnhancementNotes => Set<Note>();
    public DbSet<EnhancementAttachment> EnhancementAttachments => Set<EnhancementAttachment>();
    public DbSet<TimeRecordingCategory> TimeRecordingCategories => Set<TimeRecordingCategory>();
    public DbSet<EnhancementTimeCategory> EnhancementTimeCategories => Set<EnhancementTimeCategory>();
    public DbSet<EnhancementTimeEntry> EnhancementTimeEntries => Set<EnhancementTimeEntry>();
    public DbSet<EnhancementNotificationRecipient> EnhancementNotificationRecipients => Set<EnhancementNotificationRecipient>();
    public DbSet<WorkPhase> WorkPhases => Set<WorkPhase>();
    public DbSet<EstimationBreakdownItem> EstimationBreakdownItems => Set<EstimationBreakdownItem>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<Consolidation> Consolidations => Set<Consolidation>();
    public DbSet<ConsolidationSource> ConsolidationSources => Set<ConsolidationSource>();
    public DbSet<ResourceServiceArea> ResourceServiceAreas { get; set; } = null!;

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
            entity.Ignore(e => e.SkillsDisplay);
            entity.Ignore(e => e.TotalRecordedHours);

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
            entity.Property(e => e.ColumnsJson).IsRequired().HasDefaultValue("[]");

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
                .WithMany(e => e.Skills)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Skill)
                .WithMany(s => s.EnhancementSkills)
                .HasForeignKey(e => e.SkillId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // NEW: Enhancement Notes
        // ============================================
        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NoteText).IsRequired();

            entity.HasIndex(e => e.EnhancementId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.NoteHistory)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ============================================
        // NEW: Enhancement Attachments
        // ============================================
        modelBuilder.Entity<EnhancementAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.StoredFileName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StoragePath).IsRequired().HasMaxLength(500);

            entity.HasIndex(e => e.EnhancementId);
            entity.HasIndex(e => e.UploadedAt);

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.Attachments)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UploadedByUser)
                .WithMany()
                .HasForeignKey(e => e.UploadedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ============================================
        // NEW: Time Recording Categories (Business Areas)
        // ============================================
        modelBuilder.Entity<TimeRecordingCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);
        });

        // ============================================
        // NEW: Enhancement Time Categories (junction)
        // ============================================
        modelBuilder.Entity<EnhancementTimeCategory>(entity =>
        {
            entity.HasKey(e => new { e.EnhancementId, e.TimeCategoryId });

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.TimeCategories)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TimeCategory)
                .WithMany(tc => tc.EnhancementTimeCategories)
                .HasForeignKey(e => e.TimeCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // NEW: Enhancement Time Entries
        // ============================================
        modelBuilder.Entity<EnhancementTimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HoursJson).IsRequired().HasDefaultValue("{}");

            entity.HasIndex(e => e.EnhancementId);
            entity.HasIndex(e => new { e.EnhancementId, e.PeriodStart, e.PeriodEnd });

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.TimeEntries)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EnhancementNotificationRecipient
        modelBuilder.Entity<EnhancementNotificationRecipient>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.EnhancementId);
            entity.HasIndex(e => new { e.EnhancementId, e.ResourceId }).IsUnique();

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.NotificationRecipients)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Resource)
                .WithMany()
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<WorkPhase>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
    entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
    entity.Property(e => e.Description).HasMaxLength(500);
    entity.Property(e => e.DefaultContributionPercent).HasDefaultValue(100);
    entity.Property(e => e.IsActive).HasDefaultValue(true);
    entity.Property(e => e.ForEstimation).HasDefaultValue(true);
    entity.Property(e => e.ForTimeRecording).HasDefaultValue(true);
    entity.Property(e => e.ForConsolidation).HasDefaultValue(true);

    entity.HasIndex(e => e.Code).IsUnique();
    entity.HasIndex(e => e.DisplayOrder);
});

        // EstimationBreakdownItem
        modelBuilder.Entity<EstimationBreakdownItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Hours).HasPrecision(10, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasIndex(e => e.EnhancementId);
            entity.HasIndex(e => e.WorkPhaseId);
            entity.HasIndex(e => new { e.EnhancementId, e.WorkPhaseId }).IsUnique();

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.EstimationBreakdownItems)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkPhase)
                .WithMany(wp => wp.EstimationItems)
                .HasForeignKey(e => e.WorkPhaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // TimeEntry
        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Hours).HasPrecision(10, 2);
            entity.Property(e => e.ContributedHours).HasPrecision(10, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasIndex(e => e.EnhancementId);
            entity.HasIndex(e => e.ResourceId);
            entity.HasIndex(e => e.WorkPhaseId);
            entity.HasIndex(e => new { e.StartDate, e.EndDate });

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.TimeEntriesNew)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Resource)
                .WithMany(r => r.TimeEntries)
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.WorkPhase)
                .WithMany(wp => wp.TimeEntries)
                .HasForeignKey(e => e.WorkPhaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Ignore computed properties
            entity.Ignore(e => e.TotalPulledHours);
            entity.Ignore(e => e.RemainingHours);
        });

        // Consolidation
        modelBuilder.Entity<Consolidation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BillableHours).HasPrecision(10, 2);
            entity.Property(e => e.SourceHours).HasPrecision(10, 2);
            entity.Property(e => e.Status).HasDefaultValue(ConsolidationStatus.Draft);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasIndex(e => e.EnhancementId);
            entity.HasIndex(e => e.ServiceAreaId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.StartDate, e.EndDate });

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.Consolidations)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceArea)
                .WithMany()
                .HasForeignKey(e => e.ServiceAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ModifiedBy)
                .WithMany()
                .HasForeignKey(e => e.ModifiedById)
                .OnDelete(DeleteBehavior.SetNull);

            // Ignore computed properties
            entity.Ignore(e => e.IsManual);
            entity.Ignore(e => e.PeriodDisplay);
            entity.Ignore(e => e.DateRangeDisplay);
        });

        // ConsolidationSource
        modelBuilder.Entity<ConsolidationSource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PulledHours).HasPrecision(10, 2);

            entity.HasIndex(e => e.ConsolidationId);
            entity.HasIndex(e => e.TimeEntryId);

            entity.HasOne(e => e.Consolidation)
                .WithMany(c => c.Sources)
                .HasForeignKey(e => e.ConsolidationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TimeEntry)
                .WithMany(te => te.ConsolidationSources)
                .HasForeignKey(e => e.TimeEntryId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // ResourceServiceArea configuration
        modelBuilder.Entity<ResourceServiceArea>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ResourceId);
            entity.HasIndex(e => e.ServiceAreaId);

            // Unique constraint: one membership per resource+servicearea
            entity.HasIndex(e => new { e.ResourceId, e.ServiceAreaId }).IsUnique();

            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
            entity.Property(e => e.JoinedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Permissions).HasDefaultValue(Permissions.None);

            entity.HasOne(e => e.Resource)
                .WithMany(r => r.ServiceAreas)
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceArea)
                .WithMany()
                .HasForeignKey(e => e.ServiceAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Update Resource configuration
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.OrganizationType).HasDefaultValue(OrganizationType.Implementor);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Optional link to User
            entity.HasOne(e => e.User)
                .WithOne()
                .HasForeignKey<Resource>(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

    }
}
