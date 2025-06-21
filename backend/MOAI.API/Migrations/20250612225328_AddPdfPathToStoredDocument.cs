using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MOAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfPathToStoredDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Documents",
                newName: "PdfPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PdfPath",
                table: "Documents",
                newName: "Content");
        }
    }
}
