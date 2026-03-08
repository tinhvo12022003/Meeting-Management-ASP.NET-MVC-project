using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablev6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CompanyId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_RoomId",
                table: "Meetings");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId_DepartmentId",
                table: "Users",
                columns: new[] { "CompanyId", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CompanyId_DepartmentId",
                table: "Meetings",
                columns: new[] { "CompanyId", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_RoomId_StartAt_EndAt",
                table: "Meetings",
                columns: new[] { "RoomId", "StartAt", "EndAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CompanyId_DepartmentId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CompanyId_DepartmentId",
                table: "Meetings");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_RoomId_StartAt_EndAt",
                table: "Meetings");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CompanyId",
                table: "Meetings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_RoomId",
                table: "Meetings",
                column: "RoomId");
        }
    }
}
