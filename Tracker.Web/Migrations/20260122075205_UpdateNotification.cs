using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnhancementNotificationRecipients",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    EnhancementId = table.Column<string>(type: "TEXT", nullable: false),
                    ResourceId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnhancementNotificationRecipients", x => x.Id);
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
                name: "IX_EnhancementNotificationRecipients_ResourceId",
                table: "EnhancementNotificationRecipients",
                column: "ResourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnhancementNotificationRecipients");
        }
    }
}
