using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnhancementHistory",
                columns: table => new
                {
                    AuditId = table.Column<string>(type: "TEXT", nullable: false),
                    AuditAction = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    AuditAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AuditBy = table.Column<string>(type: "TEXT", nullable: true),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    WorkId = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    EstimatedStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceLine = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedLaborType = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedPriority = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedStatus = table.Column<string>(type: "TEXT", nullable: true),
                    ReturnedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InfStatus = table.Column<string>(type: "TEXT", nullable: true),
                    InfServiceLine = table.Column<string>(type: "TEXT", nullable: true),
                    InfLaborType = table.Column<string>(type: "TEXT", nullable: true),
                    InfPriority = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "TEXT", nullable: true),
                    TimeW1 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW2 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW3 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW4 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW5 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW6 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW7 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW8 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW9 = table.Column<decimal>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementHistory", x => x.AuditId);
                });

            migrationBuilder.CreateTable(
                name: "ResourceTypeLookups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EnhancementColumn = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowMultiple = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceTypeLookups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceAreas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeRecordingCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeRecordingCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkPhases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DefaultContributionPercent = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 100),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    ForEstimation = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    ForTimeRecording = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    ForConsolidation = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkPhases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    OrganizationType = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    HasLoginAccess = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: true),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CanConsolidate = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResourceTypeId = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resources_ResourceTypeLookups_ResourceTypeId",
                        column: x => x.ResourceTypeId,
                        principalTable: "ResourceTypeLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Enhancements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    SpawnedFromId = table.Column<string>(type: "TEXT", nullable: false),
                    WorkId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    RequestRaisedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedLaborType = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedActivityType = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedPriority = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    EstimatedStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    EstimatedStatus = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceLine = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SignITReference = table.Column<string>(type: "TEXT", nullable: true),
                    ReturnedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    InfEstimatedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InfStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    InfActivityType = table.Column<string>(type: "TEXT", nullable: true),
                    InfServiceLine = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    InfLaborType = table.Column<string>(type: "TEXT", nullable: true),
                    InfPriority = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "TEXT", nullable: true),
                    Dependencies = table.Column<string>(type: "TEXT", nullable: true),
                    InfCostCenter = table.Column<string>(type: "TEXT", nullable: true),
                    TimeW1 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW2 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW3 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW4 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW5 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW6 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW7 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW8 = table.Column<decimal>(type: "TEXT", nullable: true),
                    TimeW9 = table.Column<decimal>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LockedBy = table.Column<string>(type: "TEXT", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enhancements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enhancements_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Skills",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Skills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Skills_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NamedReports",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    FilterJson = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NamedReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NamedReports_Resources_UserId",
                        column: x => x.UserId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceServiceAreas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    JoinedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Permissions = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ReportsToResourceId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceServiceAreas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceServiceAreas_Resources_ReportsToResourceId",
                        column: x => x.ReportsToResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResourceServiceAreas_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceServiceAreas_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SavedFilters",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FilterJson = table.Column<string>(type: "TEXT", nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedFilters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedFilters_Resources_UserId",
                        column: x => x.UserId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedFilters_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserColumnPreferences",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false),
                    ColumnsJson = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "[]"),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserColumnPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserColumnPreferences_Resources_UserId",
                        column: x => x.UserId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserColumnPreferences_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Consolidations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BillableHours = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    SourceHours = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    InvoiceId = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedById = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consolidations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consolidations_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consolidations_Resources_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Consolidations_Resources_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Consolidations_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnhancementAttachments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    StoragePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    UploadedBy = table.Column<string>(type: "TEXT", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnhancementAttachments_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementAttachments_Resources_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EnhancementContacts",
                columns: table => new
                {
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementContacts", x => new { x.EnhancementId, x.ResourceId });
                    table.ForeignKey(
                        name: "FK_EnhancementContacts_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementContacts_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnhancementNotes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    NoteText = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnhancementNotes_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementNotes_Resources_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EnhancementNotificationRecipients",
                columns: table => new
                {
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementNotificationRecipients", x => new { x.EnhancementId, x.ResourceId });
                    table.ForeignKey(
                        name: "FK_EnhancementNotificationRecipients_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementNotificationRecipients_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnhancementResources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: true),
                    ChargeCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnhancementResources_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementResources_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementResources_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EnhancementSpocs",
                columns: table => new
                {
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementSpocs", x => new { x.EnhancementId, x.ResourceId });
                    table.ForeignKey(
                        name: "FK_EnhancementSpocs_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementSpocs_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnhancementSponsors",
                columns: table => new
                {
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    Id = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementSponsors", x => new { x.EnhancementId, x.ResourceId });
                    table.ForeignKey(
                        name: "FK_EnhancementSponsors_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementSponsors_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EnhancementTimeCategories",
                columns: table => new
                {
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    TimeCategoryId = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeRecordingCategoryId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementTimeCategories", x => new { x.EnhancementId, x.TimeCategoryId });
                    table.ForeignKey(
                        name: "FK_EnhancementTimeCategories_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementTimeCategories_TimeRecordingCategories_TimeCategoryId",
                        column: x => x.TimeCategoryId,
                        principalTable: "TimeRecordingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementTimeCategories_TimeRecordingCategories_TimeRecordingCategoryId",
                        column: x => x.TimeRecordingCategoryId,
                        principalTable: "TimeRecordingCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EnhancementTimeEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoursJson = table.Column<string>(type: "TEXT", nullable: false, defaultValue: "{}"),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementTimeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnhancementTimeEntries_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EstimationBreakdownItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    WorkPhaseId = table.Column<string>(type: "TEXT", nullable: false),
                    Hours = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimationBreakdownItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstimationBreakdownItems_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EstimationBreakdownItems_WorkPhases_WorkPhaseId",
                        column: x => x.WorkPhaseId,
                        principalTable: "WorkPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EstimationBreakdowns",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    RequirementsAndEstimation = table.Column<decimal>(type: "TEXT", nullable: true),
                    RequirementsAndEstimationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    VendorCoordination = table.Column<decimal>(type: "TEXT", nullable: true),
                    VendorCoordinationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    DesignFunctionalTechnical = table.Column<decimal>(type: "TEXT", nullable: true),
                    DesignFunctionalTechnicalNotes = table.Column<string>(type: "TEXT", nullable: true),
                    TestingSTI = table.Column<decimal>(type: "TEXT", nullable: true),
                    TestingSTINotes = table.Column<string>(type: "TEXT", nullable: true),
                    TestingUAT = table.Column<decimal>(type: "TEXT", nullable: true),
                    TestingUATNotes = table.Column<string>(type: "TEXT", nullable: true),
                    GoLiveDeploymentValidation = table.Column<decimal>(type: "TEXT", nullable: true),
                    GoLiveDeploymentValidationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    Hypercare = table.Column<decimal>(type: "TEXT", nullable: true),
                    HypercareNotes = table.Column<string>(type: "TEXT", nullable: true),
                    Documentation = table.Column<decimal>(type: "TEXT", nullable: true),
                    DocumentationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    PMLead = table.Column<decimal>(type: "TEXT", nullable: true),
                    PMLeadNotes = table.Column<string>(type: "TEXT", nullable: true),
                    Contingency = table.Column<decimal>(type: "TEXT", nullable: true),
                    ContingencyNotes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstimationBreakdowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstimationBreakdowns_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    WorkPhaseId = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Hours = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    ContributedHours = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedById = table.Column<string>(type: "TEXT", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Resources_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Resources_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimeEntries_WorkPhases_WorkPhaseId",
                        column: x => x.WorkPhaseId,
                        principalTable: "WorkPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EnhancementSkills",
                columns: table => new
                {
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    SkillId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementSkills", x => new { x.EnhancementId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_EnhancementSkills_Enhancements_EnhancementId",
                        column: x => x.EnhancementId,
                        principalTable: "Enhancements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EnhancementSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceSkills",
                columns: table => new
                {
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    SkillId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceSkills", x => new { x.ResourceId, x.SkillId });
                    table.ForeignKey(
                        name: "FK_ResourceSkills_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ResourceSkills_Skills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "Skills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsolidationSources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ConsolidationId = table.Column<string>(type: "TEXT", nullable: false),
                    TimeEntryId = table.Column<string>(type: "TEXT", nullable: false),
                    PulledHours = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidationSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsolidationSources_Consolidations_ConsolidationId",
                        column: x => x.ConsolidationId,
                        principalTable: "Consolidations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsolidationSources_TimeEntries_TimeEntryId",
                        column: x => x.TimeEntryId,
                        principalTable: "TimeEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consolidations_CreatedById",
                table: "Consolidations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Consolidations_EnhancementId",
                table: "Consolidations",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_Consolidations_ModifiedById",
                table: "Consolidations",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_Consolidations_ServiceAreaId",
                table: "Consolidations",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Consolidations_StartDate_EndDate",
                table: "Consolidations",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Consolidations_Status",
                table: "Consolidations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidationSources_ConsolidationId",
                table: "ConsolidationSources",
                column: "ConsolidationId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsolidationSources_TimeEntryId",
                table: "ConsolidationSources",
                column: "TimeEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementAttachments_EnhancementId",
                table: "EnhancementAttachments",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementAttachments_UploadedAt",
                table: "EnhancementAttachments",
                column: "UploadedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementAttachments_UploadedBy",
                table: "EnhancementAttachments",
                column: "UploadedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementContacts_ResourceId",
                table: "EnhancementContacts",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementHistory_AuditAt",
                table: "EnhancementHistory",
                column: "AuditAt");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementHistory_EnhancementId",
                table: "EnhancementHistory",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementNotes_CreatedAt",
                table: "EnhancementNotes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementNotes_CreatedBy",
                table: "EnhancementNotes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementNotes_EnhancementId",
                table: "EnhancementNotes",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementNotificationRecipients_ResourceId",
                table: "EnhancementNotificationRecipients",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementResources_EnhancementId",
                table: "EnhancementResources",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementResources_EnhancementId_ResourceId_ServiceAreaId",
                table: "EnhancementResources",
                columns: new[] { "EnhancementId", "ResourceId", "ServiceAreaId" });

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementResources_ResourceId",
                table: "EnhancementResources",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementResources_ServiceAreaId",
                table: "EnhancementResources",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Enhancements_ServiceAreaId",
                table: "Enhancements",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Enhancements_Status",
                table: "Enhancements",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Enhancements_WorkId",
                table: "Enhancements",
                column: "WorkId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementSkills_SkillId",
                table: "EnhancementSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementSpocs_ResourceId",
                table: "EnhancementSpocs",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementSponsors_ResourceId",
                table: "EnhancementSponsors",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementTimeCategories_TimeCategoryId",
                table: "EnhancementTimeCategories",
                column: "TimeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementTimeCategories_TimeRecordingCategoryId",
                table: "EnhancementTimeCategories",
                column: "TimeRecordingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementTimeEntries_EnhancementId",
                table: "EnhancementTimeEntries",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimationBreakdownItems_EnhancementId",
                table: "EstimationBreakdownItems",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimationBreakdownItems_EnhancementId_WorkPhaseId",
                table: "EstimationBreakdownItems",
                columns: new[] { "EnhancementId", "WorkPhaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EstimationBreakdownItems_WorkPhaseId",
                table: "EstimationBreakdownItems",
                column: "WorkPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimationBreakdowns_EnhancementId",
                table: "EstimationBreakdowns",
                column: "EnhancementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NamedReports_UserId",
                table: "NamedReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Email",
                table: "Resources",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_HasLoginAccess",
                table: "Resources",
                column: "HasLoginAccess");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_OrganizationType",
                table: "Resources",
                column: "OrganizationType");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_ResourceTypeId",
                table: "Resources",
                column: "ResourceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceServiceAreas_ReportsToResourceId",
                table: "ResourceServiceAreas",
                column: "ReportsToResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceServiceAreas_ResourceId",
                table: "ResourceServiceAreas",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceServiceAreas_ResourceId_ServiceAreaId",
                table: "ResourceServiceAreas",
                columns: new[] { "ResourceId", "ServiceAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceServiceAreas_ServiceAreaId",
                table: "ResourceServiceAreas",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceSkills_SkillId",
                table: "ResourceSkills",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedFilters_ServiceAreaId",
                table: "SavedFilters",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_SavedFilters_UserId_ServiceAreaId",
                table: "SavedFilters",
                columns: new[] { "UserId", "ServiceAreaId" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreas_Code",
                table: "ServiceAreas",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Skills_ServiceAreaId",
                table: "Skills",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_CreatedById",
                table: "TimeEntries",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_EnhancementId",
                table: "TimeEntries",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_ModifiedById",
                table: "TimeEntries",
                column: "ModifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_ResourceId",
                table: "TimeEntries",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_StartDate_EndDate",
                table: "TimeEntries",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_WorkPhaseId",
                table: "TimeEntries",
                column: "WorkPhaseId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecordingCategories_DisplayOrder",
                table: "TimeRecordingCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecordingCategories_IsActive",
                table: "TimeRecordingCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_UserColumnPreferences_ServiceAreaId",
                table: "UserColumnPreferences",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_UserColumnPreferences_UserId_ServiceAreaId",
                table: "UserColumnPreferences",
                columns: new[] { "UserId", "ServiceAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkPhases_Code",
                table: "WorkPhases",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkPhases_DisplayOrder",
                table: "WorkPhases",
                column: "DisplayOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsolidationSources");

            migrationBuilder.DropTable(
                name: "EnhancementAttachments");

            migrationBuilder.DropTable(
                name: "EnhancementContacts");

            migrationBuilder.DropTable(
                name: "EnhancementHistory");

            migrationBuilder.DropTable(
                name: "EnhancementNotes");

            migrationBuilder.DropTable(
                name: "EnhancementNotificationRecipients");

            migrationBuilder.DropTable(
                name: "EnhancementResources");

            migrationBuilder.DropTable(
                name: "EnhancementSkills");

            migrationBuilder.DropTable(
                name: "EnhancementSpocs");

            migrationBuilder.DropTable(
                name: "EnhancementSponsors");

            migrationBuilder.DropTable(
                name: "EnhancementTimeCategories");

            migrationBuilder.DropTable(
                name: "EnhancementTimeEntries");

            migrationBuilder.DropTable(
                name: "EstimationBreakdownItems");

            migrationBuilder.DropTable(
                name: "EstimationBreakdowns");

            migrationBuilder.DropTable(
                name: "NamedReports");

            migrationBuilder.DropTable(
                name: "ResourceServiceAreas");

            migrationBuilder.DropTable(
                name: "ResourceSkills");

            migrationBuilder.DropTable(
                name: "SavedFilters");

            migrationBuilder.DropTable(
                name: "UserColumnPreferences");

            migrationBuilder.DropTable(
                name: "Consolidations");

            migrationBuilder.DropTable(
                name: "TimeEntries");

            migrationBuilder.DropTable(
                name: "TimeRecordingCategories");

            migrationBuilder.DropTable(
                name: "Skills");

            migrationBuilder.DropTable(
                name: "Enhancements");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "WorkPhases");

            migrationBuilder.DropTable(
                name: "ServiceAreas");

            migrationBuilder.DropTable(
                name: "ResourceTypeLookups");
        }
    }
}
