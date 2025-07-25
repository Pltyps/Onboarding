using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MOAI.API.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // USERS
            migrationBuilder.AlterColumn<string>("Role", "Users", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.AlterColumn<string>("PasswordHash", "Users", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.AlterColumn<string>("Email", "Users", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.AlterColumn<string>("Department", "Users", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.AlterColumn<int>("Id", "Users", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // SYSTEMSTATS
            migrationBuilder.AlterColumn<int>("UptimeMinutes", "SystemStats", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER");
            migrationBuilder.AlterColumn<int>("TotalRequests", "SystemStats", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER");
            migrationBuilder.Sql(@"ALTER TABLE ""SystemStats"" ALTER COLUMN ""Timestamp"" TYPE timestamp with time zone USING ""Timestamp""::timestamp with time zone;");
            migrationBuilder.AlterColumn<int>("ActiveUserCount", "SystemStats", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER");
            migrationBuilder.AlterColumn<int>("Id", "SystemStats", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // DOCUMENTS
            migrationBuilder.AlterColumn<int>("Version", "Documents", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER");
            migrationBuilder.AlterColumn<string>("UploadedBy", "Documents", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.Sql(@"ALTER TABLE ""Documents"" ALTER COLUMN ""UploadedAt"" TYPE timestamp with time zone USING ""UploadedAt""::timestamp with time zone;");
            migrationBuilder.AlterColumn<string>("PdfPath", "Documents", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.Sql(@"ALTER TABLE ""Documents"" ALTER COLUMN ""IsActive"" DROP DEFAULT;");
            migrationBuilder.Sql(@"ALTER TABLE ""Documents"" ALTER COLUMN ""IsActive"" TYPE boolean USING ""IsActive""::boolean;");
            migrationBuilder.AlterColumn<long>("FileSizeBytes", "Documents", "bigint", nullable: false, oldClrType: typeof(int), oldType: "INTEGER");
            migrationBuilder.AlterColumn<string>("FileName", "Documents", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.AlterColumn<string>("Department", "Documents", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.AlterColumn<string>("Content", "Documents", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.AlterColumn<int>("Id", "Documents", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // CHATSESSIONS
            migrationBuilder.AlterColumn<string>("UserEmail", "ChatSessions", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.AlterColumn<string>("Title", "ChatSessions", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.Sql(@"ALTER TABLE ""ChatSessions"" ALTER COLUMN ""CreatedAt"" TYPE timestamp with time zone USING ""CreatedAt""::timestamp with time zone;");
            migrationBuilder.AlterColumn<int>("Id", "ChatSessions", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            // CHATMESSAGES
            migrationBuilder.Sql(@"ALTER TABLE ""ChatMessages"" ALTER COLUMN ""Timestamp"" TYPE timestamp with time zone USING ""Timestamp""::timestamp with time zone;");
            migrationBuilder.AlterColumn<string>("Role", "ChatMessages", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.AlterColumn<string>("Message", "ChatMessages", "text", nullable: false, oldClrType: typeof(string), oldType: "TEXT");
            migrationBuilder.Sql("ALTER TABLE \"ChatMessages\" ALTER COLUMN \"IsHelpful\" TYPE boolean USING \"IsHelpful\"::boolean;");
            migrationBuilder.AlterColumn<int>("ChatSessionId", "ChatMessages", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER");
            migrationBuilder.AlterColumn<int>("Id", "ChatMessages", "integer", nullable: false, oldClrType: typeof(int), oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // You can leave the Down method as-is or regenerate it from EF Core (not critical for PostgreSQL issues)
        }
    }
}
