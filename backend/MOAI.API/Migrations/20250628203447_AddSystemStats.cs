using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SystemStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActiveUserCount = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    UptimeMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemStats", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SystemStats");
        }
    }
}
