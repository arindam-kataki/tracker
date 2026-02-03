using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AllocatedHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AllocationHours",
                table: "EnhancementResources",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllocationHours",
                table: "EnhancementResources");
        }
    }
}
