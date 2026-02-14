using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeetingManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Address = table.Column<string>(type: "NVARCHAR(255)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(255)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    UpdateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    rowStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(255)", nullable: false),
                    CompanyId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    UpdateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    rowStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MeetingRooms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Name = table.Column<string>(type: "NVARCHAR(255)", nullable: false),
                    CompanyId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    UpdateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    rowStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingRooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeetingRooms_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    FullName = table.Column<string>(type: "NVARCHAR(50)", nullable: false),
                    Address = table.Column<string>(type: "NVARCHAR(255)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "NVARCHAR(50)", nullable: true),
                    Birthday = table.Column<DateOnly>(type: "DATE", nullable: true),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartmentId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    CompanyId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Gender = table.Column<byte>(type: "TINYINT", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    UpdateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    rowStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Users_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Title = table.Column<string>(type: "NVARCHAR(255)", nullable: false),
                    StartAt = table.Column<DateTime>(type: "DATETIME2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "DATETIME2", nullable: false),
                    Type = table.Column<byte>(type: "TINYINT", nullable: false),
                    Status = table.Column<byte>(type: "TINYINT", nullable: false, defaultValue: (byte)1),
                    Description = table.Column<string>(type: "NVARCHAR(255)", nullable: true),
                    Organization = table.Column<string>(type: "NVARCHAR(255)", nullable: true),
                    Url = table.Column<string>(type: "NVARCHAR(255)", nullable: true),
                    CompanyId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    DepartmentId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    RoomId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    UpdateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    rowStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meetings_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Meetings_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Meetings_MeetingRooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "MeetingRooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Username = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Password = table.Column<string>(type: "NVARCHAR(150)", maxLength: 150, nullable: false),
                    UserId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    UpdateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    rowStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Controller = table.Column<string>(type: "NVARCHAR(50)", nullable: false),
                    Action = table.Column<string>(type: "NVARCHAR(50)", nullable: false),
                    FullPermission = table.Column<bool>(type: "BIT", nullable: false),
                    View = table.Column<bool>(type: "BIT", nullable: false),
                    Edit = table.Column<bool>(type: "BIT", nullable: false),
                    Delete = table.Column<bool>(type: "BIT", nullable: false),
                    Insert = table.Column<bool>(type: "BIT", nullable: false),
                    EditAll = table.Column<bool>(type: "BIT", nullable: false),
                    DeleteAll = table.Column<bool>(type: "BIT", nullable: false),
                    InsertAll = table.Column<bool>(type: "BIT", nullable: false),
                    UserId = table.Column<string>(type: "NVARCHAR(100)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Permissions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MeetingUsers",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    MeetingId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    Role = table.Column<byte>(type: "TINYINT", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "BIT", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingUsers", x => new { x.UserId, x.MeetingId });
                    table.ForeignKey(
                        name: "FK_MeetingUsers_Meetings_MeetingId",
                        column: x => x.MeetingId,
                        principalTable: "Meetings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MeetingUsers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    TokenHash = table.Column<string>(type: "NVARCHAR(1000)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "DATETIME2", nullable: false),
                    LoginAt = table.Column<DateTime>(type: "DATETIME2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "DATETIME2", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "NVARCHAR(1000)", nullable: true),
                    AccountId = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdateAt = table.Column<DateTime>(type: "DATETIME2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    UpdateBy = table.Column<string>(type: "NVARCHAR(100)", nullable: false),
                    rowStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Departments_CompanyId",
                table: "Departments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingRooms_CompanyId",
                table: "MeetingRooms",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CompanyId",
                table: "Meetings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_DepartmentId",
                table: "Meetings",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_RoomId",
                table: "Meetings",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_MeetingUsers_MeetingId",
                table: "MeetingUsers",
                column: "MeetingId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_UserId",
                table: "Permissions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_AccountId",
                table: "RefreshToken",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_TokenHash",
                table: "RefreshToken",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_CompanyId",
                table: "Users",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DepartmentId",
                table: "Users",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MeetingUsers");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "Meetings");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "MeetingRooms");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
