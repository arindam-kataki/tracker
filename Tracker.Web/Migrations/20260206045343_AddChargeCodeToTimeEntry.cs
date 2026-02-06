using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tracker.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddChargeCodeToTimeEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChargeCode",
                table: "TimeEntries",
                type: "TEXT",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChargeCode",
                table: "TimeEntries");
        }
    }
}
