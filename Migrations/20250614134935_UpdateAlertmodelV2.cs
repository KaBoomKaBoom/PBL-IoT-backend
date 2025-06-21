using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PBL_IoT_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAlertmodelV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_Users_UserId",
                schema: "PBL_IOT",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_UserId",
                schema: "PBL_IOT",
                table: "Alerts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Alerts_UserId",
                schema: "PBL_IOT",
                table: "Alerts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_Users_UserId",
                schema: "PBL_IOT",
                table: "Alerts",
                column: "UserId",
                principalSchema: "PBL_IOT",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
