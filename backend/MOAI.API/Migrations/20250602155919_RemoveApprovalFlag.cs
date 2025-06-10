using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOAI.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveApprovalFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Approved",
                table: "Documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "Documents",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
