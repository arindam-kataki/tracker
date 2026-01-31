using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailabilityAndBilling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consolidations_Users_CreatedById",
                table: "Consolidations");

            migrationBuilder.DropForeignKey(
                name: "FK_Consolidations_Users_ModifiedById",
                table: "Consolidations");

            migrationBuilder.DropForeignKey(
                name: "FK_EnhancementAttachments_Users_UploadedBy",
                table: "EnhancementAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_EnhancementNotes_Users_CreatedBy",
                table: "EnhancementNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_NamedReports_Users_UserId",
                table: "NamedReports");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedFilters_Users_UserId",
                table: "SavedFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_Users_CreatedById",
                table: "TimeEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_Users_ModifiedById",
                table: "TimeEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_UserColumnPreferences_Users_UserId",
                table: "UserColumnPreferences");

            migrationBuilder.DropTable(
                name: "UserServiceAreas");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_EnhancementTimeEntries_EnhancementId_PeriodStart_PeriodEnd",
                table: "EnhancementTimeEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnhancementNotificationRecipients",
                table: "EnhancementNotificationRecipients");

            migrationBuilder.DropIndex(
                name: "IX_EnhancementNotificationRecipients_EnhancementId",
                table: "EnhancementNotificationRecipients");

            migrationBuilder.DropIndex(
                name: "IX_EnhancementNotificationRecipients_EnhancementId_ResourceId",
                table: "EnhancementNotificationRecipients");

            migrationBuilder.DropColumn(
                name: "ColumnsJson",
                table: "NamedReports");

            migrationBuilder.DropColumn(
                name: "ServiceAreaIdsJson",
                table: "NamedReports");

            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "Enhancements",
                newName: "InfLaborType");

            migrationBuilder.RenameColumn(
                name: "LaborType",
                table: "Enhancements",
                newName: "InfActivityType");

            migrationBuilder.RenameColumn(
                name: "ActivityType",
                table: "Enhancements",
                newName: "EstimatedStatus");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAdmin",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "HasLoginAccess",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resources",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<bool>(
                name: "CanConsolidate",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<decimal>(
                name: "BillableTargetPercent",
                table: "Resources",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DefaultBillRate",
                table: "Resources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmploymentType",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Resources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExemptionStatus",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "InternalCostRate",
                table: "Resources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBillable",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JobTitle",
                table: "Resources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Resources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OvertimeMultiplier",
                table: "Resources",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StandardHoursPerWeek",
                table: "Resources",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "Resources",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TimeRecordingCategoryId",
                table: "EnhancementTimeCategories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedActivityType",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedLaborType",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedPriority",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "EnhancementNotificationRecipients",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "EnhancementHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedLaborType",
                table: "EnhancementHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedPriority",
                table: "EnhancementHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstimatedStatus",
                table: "EnhancementHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfLaborType",
                table: "EnhancementHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfPriority",
                table: "EnhancementHistory",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnhancementNotificationRecipients",
                table: "EnhancementNotificationRecipients",
                columns: new[] { "EnhancementId", "ResourceId" });

            migrationBuilder.CreateTable(
                name: "ClientRates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    ClientId = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    BillRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientRates_Resources_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientRates_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyHolidays",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    HoursOff = table.Column<decimal>(type: "TEXT", nullable: false),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: true),
                    Location = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsRecurringAnnually = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyHolidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyHolidays_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ResourceAvailabilities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    HoursPerDay = table.Column<decimal>(type: "TEXT", nullable: true),
                    IsRecurring = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecurrencePattern = table.Column<int>(type: "INTEGER", nullable: false),
                    RecurrenceEndDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedById = table.Column<string>(type: "TEXT", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RejectionReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedById = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceAvailabilities_Resources_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ResourceAvailabilities_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceRates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    BillRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CostRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceRates_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceSchedules",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    MondayHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    TuesdayHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    WednesdayHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    ThursdayHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    FridayHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    SaturdayHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    SundayHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourceSchedules_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceAreaRates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceTypeId = table.Column<string>(type: "TEXT", nullable: false),
                    ClientId = table.Column<string>(type: "TEXT", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    BillRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    CostRate = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: true),
                    Currency = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false, defaultValue: "USD"),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedById = table.Column<string>(type: "TEXT", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceAreaRates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceAreaRates_ResourceTypeLookups_ResourceTypeId",
                        column: x => x.ResourceTypeId,
                        principalTable: "ResourceTypeLookups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServiceAreaRates_Resources_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ServiceAreaRates_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_EnhancementTimeCategories_TimeRecordingCategoryId",
                table: "EnhancementTimeCategories",
                column: "TimeRecordingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRates_ClientId",
                table: "ClientRates",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRates_EffectiveFrom",
                table: "ClientRates",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRates_ResourceId_ClientId",
                table: "ClientRates",
                columns: new[] { "ResourceId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyHolidays_Date",
                table: "CompanyHolidays",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyHolidays_IsActive",
                table: "CompanyHolidays",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyHolidays_ServiceAreaId",
                table: "CompanyHolidays",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAvailabilities_ApprovedById",
                table: "ResourceAvailabilities",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAvailabilities_EndDate",
                table: "ResourceAvailabilities",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAvailabilities_ResourceId",
                table: "ResourceAvailabilities",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAvailabilities_StartDate",
                table: "ResourceAvailabilities",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceAvailabilities_Status",
                table: "ResourceAvailabilities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceRates_EffectiveFrom",
                table: "ResourceRates",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceRates_ResourceId",
                table: "ResourceRates",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceSchedules_EffectiveFrom",
                table: "ResourceSchedules",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_ResourceSchedules_ResourceId",
                table: "ResourceSchedules",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreaRates_ClientId",
                table: "ServiceAreaRates",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreaRates_EffectiveFrom",
                table: "ServiceAreaRates",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreaRates_IsActive",
                table: "ServiceAreaRates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreaRates_ResourceTypeId",
                table: "ServiceAreaRates",
                column: "ResourceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceAreaRates_ServiceAreaId_ResourceTypeId",
                table: "ServiceAreaRates",
                columns: new[] { "ServiceAreaId", "ResourceTypeId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Consolidations_Resources_CreatedById",
                table: "Consolidations",
                column: "CreatedById",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Consolidations_Resources_ModifiedById",
                table: "Consolidations",
                column: "ModifiedById",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EnhancementAttachments_Resources_UploadedBy",
                table: "EnhancementAttachments",
                column: "UploadedBy",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EnhancementNotes_Resources_CreatedBy",
                table: "EnhancementNotes",
                column: "CreatedBy",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EnhancementTimeCategories_TimeRecordingCategories_TimeRecordingCategoryId",
                table: "EnhancementTimeCategories",
                column: "TimeRecordingCategoryId",
                principalTable: "TimeRecordingCategories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_NamedReports_Resources_UserId",
                table: "NamedReports",
                column: "UserId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedFilters_Resources_UserId",
                table: "SavedFilters",
                column: "UserId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_Resources_CreatedById",
                table: "TimeEntries",
                column: "CreatedById",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_Resources_ModifiedById",
                table: "TimeEntries",
                column: "ModifiedById",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserColumnPreferences_Resources_UserId",
                table: "UserColumnPreferences",
                column: "UserId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consolidations_Resources_CreatedById",
                table: "Consolidations");

            migrationBuilder.DropForeignKey(
                name: "FK_Consolidations_Resources_ModifiedById",
                table: "Consolidations");

            migrationBuilder.DropForeignKey(
                name: "FK_EnhancementAttachments_Resources_UploadedBy",
                table: "EnhancementAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_EnhancementNotes_Resources_CreatedBy",
                table: "EnhancementNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_EnhancementTimeCategories_TimeRecordingCategories_TimeRecordingCategoryId",
                table: "EnhancementTimeCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_NamedReports_Resources_UserId",
                table: "NamedReports");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedFilters_Resources_UserId",
                table: "SavedFilters");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_Resources_CreatedById",
                table: "TimeEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_Resources_ModifiedById",
                table: "TimeEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_UserColumnPreferences_Resources_UserId",
                table: "UserColumnPreferences");

            migrationBuilder.DropTable(
                name: "ClientRates");

            migrationBuilder.DropTable(
                name: "CompanyHolidays");

            migrationBuilder.DropTable(
                name: "ResourceAvailabilities");

            migrationBuilder.DropTable(
                name: "ResourceRates");

            migrationBuilder.DropTable(
                name: "ResourceSchedules");

            migrationBuilder.DropTable(
                name: "ServiceAreaRates");

            migrationBuilder.DropIndex(
                name: "IX_Resources_Email",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_HasLoginAccess",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_OrganizationType",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_EnhancementTimeCategories_TimeRecordingCategoryId",
                table: "EnhancementTimeCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EnhancementNotificationRecipients",
                table: "EnhancementNotificationRecipients");

            migrationBuilder.DropColumn(
                name: "BillableTargetPercent",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "DefaultBillRate",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "ExemptionStatus",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "InternalCostRate",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "IsBillable",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "OvertimeMultiplier",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "StandardHoursPerWeek",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Resources");

            migrationBuilder.DropColumn(
                name: "TimeRecordingCategoryId",
                table: "EnhancementTimeCategories");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "EstimatedActivityType",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "EstimatedLaborType",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "EstimatedPriority",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "EnhancementHistory");

            migrationBuilder.DropColumn(
                name: "EstimatedLaborType",
                table: "EnhancementHistory");

            migrationBuilder.DropColumn(
                name: "EstimatedPriority",
                table: "EnhancementHistory");

            migrationBuilder.DropColumn(
                name: "EstimatedStatus",
                table: "EnhancementHistory");

            migrationBuilder.DropColumn(
                name: "InfLaborType",
                table: "EnhancementHistory");

            migrationBuilder.DropColumn(
                name: "InfPriority",
                table: "EnhancementHistory");

            migrationBuilder.RenameColumn(
                name: "InfLaborType",
                table: "Enhancements",
                newName: "Priority");

            migrationBuilder.RenameColumn(
                name: "InfActivityType",
                table: "Enhancements",
                newName: "LaborType");

            migrationBuilder.RenameColumn(
                name: "EstimatedStatus",
                table: "Enhancements",
                newName: "ActivityType");

            migrationBuilder.AlterColumn<bool>(
                name: "IsAdmin",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "HasLoginAccess",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Resources",
                type: "TEXT",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "CanConsolidate",
                table: "Resources",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ColumnsJson",
                table: "NamedReports",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ServiceAreaIdsJson",
                table: "NamedReports",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "EnhancementNotificationRecipients",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_EnhancementNotificationRecipients",
                table: "EnhancementNotificationRecipients",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CanConsolidate = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserServiceAreas",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserServiceAreas", x => new { x.UserId, x.ServiceAreaId });
                    table.ForeignKey(
                        name: "FK_UserServiceAreas_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserServiceAreas_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementTimeEntries_EnhancementId_PeriodStart_PeriodEnd",
                table: "EnhancementTimeEntries",
                columns: new[] { "EnhancementId", "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementNotificationRecipients_EnhancementId",
                table: "EnhancementNotificationRecipients",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementNotificationRecipients_EnhancementId_ResourceId",
                table: "EnhancementNotificationRecipients",
                columns: new[] { "EnhancementId", "ResourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserServiceAreas_ServiceAreaId",
                table: "UserServiceAreas",
                column: "ServiceAreaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consolidations_Users_CreatedById",
                table: "Consolidations",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Consolidations_Users_ModifiedById",
                table: "Consolidations",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EnhancementAttachments_Users_UploadedBy",
                table: "EnhancementAttachments",
                column: "UploadedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EnhancementNotes_Users_CreatedBy",
                table: "EnhancementNotes",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_NamedReports_Users_UserId",
                table: "NamedReports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SavedFilters_Users_UserId",
                table: "SavedFilters",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_Users_CreatedById",
                table: "TimeEntries",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_Users_ModifiedById",
                table: "TimeEntries",
                column: "ModifiedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserColumnPreferences_Users_UserId",
                table: "UserColumnPreferences",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
