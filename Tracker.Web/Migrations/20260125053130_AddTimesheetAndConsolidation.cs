using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddTimesheetAndConsolidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActivityType",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dependencies",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfCostCenter",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InfEstimatedHours",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfPriority",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LaborType",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAt",
                table: "Enhancements",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LockedBy",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestRaisedDate",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SignITReference",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                table: "Enhancements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpawnedFromId",
                table: "Enhancements",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

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
                        name: "FK_Consolidations_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Consolidations_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Consolidations_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "TimeEntries",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    WorkPhaseId = table.Column<string>(type: "TEXT", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
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
                        name: "FK_TimeEntries_Resources_ResourceId",
                        column: x => x.ResourceId,
                        principalTable: "Resources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimeEntries_Users_ModifiedById",
                        column: x => x.ModifiedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TimeEntries_WorkPhases_WorkPhaseId",
                        column: x => x.WorkPhaseId,
                        principalTable: "WorkPhases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "EstimationBreakdownItems");

            migrationBuilder.DropTable(
                name: "Consolidations");

            migrationBuilder.DropTable(
                name: "TimeEntries");

            migrationBuilder.DropTable(
                name: "WorkPhases");

            migrationBuilder.DropColumn(
                name: "ActivityType",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "Dependencies",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "InfCostCenter",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "InfEstimatedHours",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "InfPriority",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "LaborType",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "LockedAt",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "LockedBy",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "RequestRaisedDate",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "SignITReference",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "Source",
                table: "Enhancements");

            migrationBuilder.DropColumn(
                name: "SpawnedFromId",
                table: "Enhancements");
        }
    }
}
