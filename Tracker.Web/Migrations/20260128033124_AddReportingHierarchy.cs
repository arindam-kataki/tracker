using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddReportingHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_Resources_ResourceId",
                table: "TimeEntries");

            migrationBuilder.AddColumn<string>(
                name: "ReportsToResourceId",
                table: "ResourceServiceAreas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceServiceAreas_ReportsToResourceId",
                table: "ResourceServiceAreas",
                column: "ReportsToResourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ResourceServiceAreas_Resources_ReportsToResourceId",
                table: "ResourceServiceAreas",
                column: "ReportsToResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_Resources_ResourceId",
                table: "TimeEntries",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ResourceServiceAreas_Resources_ReportsToResourceId",
                table: "ResourceServiceAreas");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_Resources_ResourceId",
                table: "TimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_ResourceServiceAreas_ReportsToResourceId",
                table: "ResourceServiceAreas");

            migrationBuilder.DropColumn(
                name: "ReportsToResourceId",
                table: "ResourceServiceAreas");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_Resources_ResourceId",
                table: "TimeEntries",
                column: "ResourceId",
                principalTable: "Resources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
