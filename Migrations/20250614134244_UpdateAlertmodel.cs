using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL_IoT_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAlertmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SensorId",
                schema: "PBL_IOT",
                table: "Alerts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SensorId",
                schema: "PBL_IOT",
                table: "Alerts");
        }
    }
}
