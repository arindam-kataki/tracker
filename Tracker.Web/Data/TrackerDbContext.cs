using Microsoft.EntityFrameworkCore;
using Tracker.Web.Entities;

namespace Tracker.Web.Data;

public class TrackerDbContext : DbContext
{
    public TrackerDbContext(DbContextOptions<TrackerDbContext> options) : base(options)
    {
    }

    // === Core DbSets ===
    public DbSet<ServiceArea> ServiceAreas => Set<ServiceArea>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceServiceArea> ResourceServiceAreas { get; set; } = null!;
    
    // === Enhancement DbSets ===
    public DbSet<Enhancement> Enhancements => Set<Enhancement>();
    public DbSet<EstimationBreakdown> EstimationBreakdowns => Set<EstimationBreakdown>();
    public DbSet<EnhancementContact> EnhancementContacts => Set<EnhancementContact>();
    public DbSet<EnhancementResource> EnhancementResources => Set<EnhancementResource>();
    public DbSet<EnhancementSponsor> EnhancementSponsors => Set<EnhancementSponsor>();
    public DbSet<EnhancementSpoc> EnhancementSpocs => Set<EnhancementSpoc>();
    public DbSet<EnhancementHistory> EnhancementHistory => Set<EnhancementHistory>();
    public DbSet<EnhancementSkill> EnhancementSkills => Set<EnhancementSkill>();
    
    // === Enhancement Details DbSets ===
    public DbSet<Note> EnhancementNotes => Set<Note>();
    public DbSet<EnhancementAttachment> EnhancementAttachments => Set<EnhancementAttachment>();
    public DbSet<EnhancementNotificationRecipient> EnhancementNotificationRecipients => Set<EnhancementNotificationRecipient>();
    
    // === Time Recording DbSets ===
    public DbSet<TimeRecordingCategory> TimeRecordingCategories => Set<TimeRecordingCategory>();
    public DbSet<EnhancementTimeCategory> EnhancementTimeCategories => Set<EnhancementTimeCategory>();
    public DbSet<EnhancementTimeEntry> EnhancementTimeEntries => Set<EnhancementTimeEntry>();
    public DbSet<WorkPhase> WorkPhases => Set<WorkPhase>();
    public DbSet<EstimationBreakdownItem> EstimationBreakdownItems => Set<EstimationBreakdownItem>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    
    // === Consolidation/Billing DbSets ===
    public DbSet<Consolidation> Consolidations => Set<Consolidation>();
    public DbSet<ConsolidationSource> ConsolidationSources => Set<ConsolidationSource>();
    
    // === User Preferences DbSets (renamed from User* to Resource*) ===
    public DbSet<SavedFilter> SavedFilters => Set<SavedFilter>();
    public DbSet<ResourceColumnPreference> ResourceColumnPreferences => Set<ResourceColumnPreference>();
    public DbSet<NamedReport> NamedReports => Set<NamedReport>();
    
    // === Lookup DbSets ===
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<ResourceSkill> ResourceSkills => Set<ResourceSkill>();
    public DbSet<ResourceTypeLookup> ResourceTypeLookups => Set<ResourceTypeLookup>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ============================================
        // ServiceArea
        // ============================================
        modelBuilder.Entity<ServiceArea>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // ============================================
        // Resource (unified entity for people and authentication)
        // ============================================
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.OrganizationType).HasDefaultValue(OrganizationType.Implementor);
            
            // Authentication fields
            entity.Property(e => e.HasLoginAccess).HasDefaultValue(false);
            entity.Property(e => e.IsAdmin).HasDefaultValue(false);
            entity.Property(e => e.CanConsolidate).HasDefaultValue(false);
            
            // Legacy ResourceType relationship
            entity.HasOne(e => e.ResourceType)
                .WithMany(rt => rt.Resources)
                .HasForeignKey(e => e.ResourceTypeId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // Indexes
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.OrganizationType);
            entity.HasIndex(e => e.HasLoginAccess);
            
            // Ignore computed properties
            entity.Ignore(e => e.OrganizationTypeDisplay);
            entity.Ignore(e => e.OrganizationTypeBadgeClass);
            entity.Ignore(e => e.PrimaryServiceArea);
            entity.Ignore(e => e.RoleString);
            entity.Ignore(e => e.DisplayName);
        });

        // ============================================
        // ResourceServiceArea (junction with permissions)
        // ============================================
        modelBuilder.Entity<ResourceServiceArea>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.ResourceId);
            entity.HasIndex(e => e.ServiceAreaId);
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

            // ReportsTo relationship
            entity.HasIndex(e => e.ReportsToResourceId);
            entity.HasOne(e => e.ReportsTo)
                .WithMany()
                .HasForeignKey(e => e.ReportsToResourceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ============================================
        // Enhancement
        // ============================================
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

        // ============================================
        // EstimationBreakdown (1:1 with Enhancement)
        // ============================================
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

        // ============================================
        // Enhancement Junction Tables
        // ============================================
        
        // EnhancementContact
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

        // EnhancementResource
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

        // EnhancementSponsor
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

        // EnhancementSpoc
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

        // ============================================
        // SavedFilter (FK now to Resource)
        // ============================================
        modelBuilder.Entity<SavedFilter>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FilterJson).IsRequired();

            entity.HasIndex(e => new { e.ResourceId, e.ServiceAreaId });

            entity.HasOne(e => e.Resource)
                .WithMany()
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceArea)
                .WithMany()
                .HasForeignKey(e => e.ServiceAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // ResourceColumnPreference (renamed from UserColumnPreference)
        // ============================================
        modelBuilder.Entity<ResourceColumnPreference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ColumnsJson).IsRequired().HasDefaultValue("[]");

            entity.HasIndex(e => new { e.ResourceId, e.ServiceAreaId }).IsUnique();

            entity.HasOne(e => e.Resource)
                .WithMany()
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ServiceArea)
                .WithMany()
                .HasForeignKey(e => e.ServiceAreaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // NamedReport (FK now to Resource)
        // ============================================
        modelBuilder.Entity<NamedReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ServiceAreaIdsJson).IsRequired().HasDefaultValue("[]");
            entity.Property(e => e.FilterJson).IsRequired().HasDefaultValue("{}");
            entity.Property(e => e.ColumnsJson).IsRequired().HasDefaultValue("[]");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.ResourceId);

            entity.HasOne(e => e.Resource)
                .WithMany()
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Ignore computed properties
            entity.Ignore(e => e.ServiceAreaIdsJson);
            entity.Ignore(e => e.ColumnsJson);
        });

        // ============================================
        // Lookup Tables
        // ============================================
        
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
        // Enhancement Notes (FK now to Resource)
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

            entity.HasOne(e => e.CreatedByResource)
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ============================================
        // Enhancement Attachments (FK now to Resource)
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
            entity.HasIndex(e => e.UploadedBy);

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.Attachments)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UploadedByResource)
                .WithMany()
                .HasForeignKey(e => e.UploadedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ============================================
        // Time Recording Categories
        // ============================================
        modelBuilder.Entity<TimeRecordingCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasIndex(e => e.DisplayOrder);
            entity.HasIndex(e => e.IsActive);
        });

        // EnhancementTimeCategory
        modelBuilder.Entity<EnhancementTimeCategory>(entity =>
        {
            entity.HasKey(e => new { e.EnhancementId, e.TimeCategoryId });

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.TimeCategories)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.TimeCategory)
                .WithMany()
                .HasForeignKey(e => e.TimeCategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EnhancementTimeEntry (legacy - different from TimeEntry)
        modelBuilder.Entity<EnhancementTimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HoursJson).IsRequired().HasDefaultValue("{}");

            entity.HasIndex(e => e.EnhancementId);

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.TimeEntries)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // EnhancementNotificationRecipient
        modelBuilder.Entity<EnhancementNotificationRecipient>(entity =>
        {
            entity.HasKey(e => new { e.EnhancementId, e.ResourceId });

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.NotificationRecipients)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Resource)
                .WithMany()
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ============================================
        // WorkPhase
        // ============================================
        modelBuilder.Entity<WorkPhase>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
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

        // ============================================
        // TimeEntry (FK now to Resource for CreatedBy/ModifiedBy)
        // ============================================
        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Hours).HasPrecision(10, 2);
            entity.Property(e => e.ContributedHours).HasPrecision(10, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasIndex(e => e.EnhancementId);
            entity.HasIndex(e => e.ResourceId);
            entity.HasIndex(e => e.WorkPhaseId);
            entity.HasIndex(e => e.CreatedById);
            entity.HasIndex(e => e.ModifiedById);
            entity.HasIndex(e => new { e.StartDate, e.EndDate });

            entity.HasOne(e => e.Enhancement)
                .WithMany(e => e.TimeEntriesNew)
                .HasForeignKey(e => e.EnhancementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Resource)
                .WithMany(r => r.TimeEntries)
                .HasForeignKey(e => e.ResourceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkPhase)
                .WithMany(wp => wp.TimeEntries)
                .HasForeignKey(e => e.WorkPhaseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Now FK to Resource instead of User
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
            entity.Ignore(e => e.AvailableHours);
            entity.Ignore(e => e.IsFullyConsolidated);
            //entity.Ignore(e => e.DateRangeDisplay);
        });

        // ============================================
        // Consolidation (FK now to Resource for CreatedBy/ModifiedBy)
        // ============================================
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

            // Now FK to Resource instead of User
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
    }
}