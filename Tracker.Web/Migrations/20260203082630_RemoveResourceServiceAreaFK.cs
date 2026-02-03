using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class RemoveResourceServiceAreaFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EnhancementResources_ServiceAreas_ServiceAreaId",
                table: "EnhancementResources");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_EnhancementResources_ServiceAreas_ServiceAreaId",
                table: "EnhancementResources",
                column: "ServiceAreaId",
                principalTable: "ServiceAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
