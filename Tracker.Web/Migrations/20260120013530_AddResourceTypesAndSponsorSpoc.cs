using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddResourceTypesAndSponsorSpoc : Migration
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
                    ReturnedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InfStatus = table.Column<string>(type: "TEXT", nullable: true),
                    InfServiceLine = table.Column<string>(type: "TEXT", nullable: true),
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
                name: "Resources",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resources", x => x.Id);
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
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Enhancements",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    WorkId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    ServiceAreaId = table.Column<string>(type: "TEXT", nullable: false),
                    EstimatedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    EstimatedStartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimatedEndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EstimationNotes = table.Column<string>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ServiceLine = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ReturnedHours = table.Column<decimal>(type: "TEXT", nullable: true),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InfStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    InfServiceLine = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
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
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
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
                        name: "FK_SavedFilters_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedFilters_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
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
                    ColumnsJson = table.Column<string>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserColumnPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserColumnPreferences_ServiceAreas_ServiceAreaId",
                        column: x => x.ServiceAreaId,
                        principalTable: "ServiceAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserColumnPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "EnhancementResources",
                columns: table => new
                {
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementResources", x => new { x.EnhancementId, x.ResourceId });
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
                name: "IX_EnhancementResources_ResourceId",
                table: "EnhancementResources",
                column: "ResourceId");

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
                name: "IX_EnhancementSpocs_ResourceId",
                table: "EnhancementSpocs",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementSponsors_ResourceId",
                table: "EnhancementSponsors",
                column: "ResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_EstimationBreakdowns_EnhancementId",
                table: "EstimationBreakdowns",
                column: "EnhancementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Resources_Type",
                table: "Resources",
                column: "Type");

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
                name: "IX_UserColumnPreferences_ServiceAreaId",
                table: "UserColumnPreferences",
                column: "ServiceAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_UserColumnPreferences_UserId_ServiceAreaId",
                table: "UserColumnPreferences",
                columns: new[] { "UserId", "ServiceAreaId" },
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnhancementContacts");

            migrationBuilder.DropTable(
                name: "EnhancementHistory");

            migrationBuilder.DropTable(
                name: "EnhancementResources");

            migrationBuilder.DropTable(
                name: "EnhancementSpocs");

            migrationBuilder.DropTable(
                name: "EnhancementSponsors");

            migrationBuilder.DropTable(
                name: "EstimationBreakdowns");

            migrationBuilder.DropTable(
                name: "SavedFilters");

            migrationBuilder.DropTable(
                name: "UserColumnPreferences");

            migrationBuilder.DropTable(
                name: "UserServiceAreas");

            migrationBuilder.DropTable(
                name: "Resources");

            migrationBuilder.DropTable(
                name: "Enhancements");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "ServiceAreas");
        }
    }
}
