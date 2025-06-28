using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMessageRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHelpful",
                table: "ChatMessages",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHelpful",
                table: "ChatMessages");
        }
    }
}
