using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingManagement.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablev2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Meetings",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "Meetings");
        }
    }
}
