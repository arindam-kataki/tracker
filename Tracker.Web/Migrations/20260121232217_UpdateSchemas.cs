using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ColumnsJson",
                table: "UserColumnPreferences",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]",
                oldClrType: typeof(string),
                oldType: "TEXT");

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
                        name: "FK_EnhancementAttachments_Users_UploadedBy",
                        column: x => x.UploadedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                        name: "FK_EnhancementNotes_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                name: "EnhancementTimeCategories",
                columns: table => new
                {
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    TimeCategoryId = table.Column<string>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false)
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
                });

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
                name: "IX_EnhancementTimeCategories_TimeCategoryId",
                table: "EnhancementTimeCategories",
                column: "TimeCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementTimeEntries_EnhancementId",
                table: "EnhancementTimeEntries",
                column: "EnhancementId");

            migrationBuilder.CreateIndex(
                name: "IX_EnhancementTimeEntries_EnhancementId_PeriodStart_PeriodEnd",
                table: "EnhancementTimeEntries",
                columns: new[] { "EnhancementId", "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecordingCategories_DisplayOrder",
                table: "TimeRecordingCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecordingCategories_IsActive",
                table: "TimeRecordingCategories",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnhancementAttachments");

            migrationBuilder.DropTable(
                name: "EnhancementNotes");

            migrationBuilder.DropTable(
                name: "EnhancementTimeCategories");

            migrationBuilder.DropTable(
                name: "EnhancementTimeEntries");

            migrationBuilder.DropTable(
                name: "TimeRecordingCategories");

            migrationBuilder.AlterColumn<string>(
                name: "ColumnsJson",
                table: "UserColumnPreferences",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "[]");
        }
    }
}
