using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class AddTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TenantCode",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "relief");

            migrationBuilder.AddColumn<string>(
                name: "TenantCode",
                table: "Groups",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "relief");

            migrationBuilder.CreateTable(
                name: "Translations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Language = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Market = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantCode",
                table: "Users",
                column: "TenantCode");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_TenantCode",
                table: "Groups",
                column: "TenantCode");

            migrationBuilder.CreateIndex(
                name: "IX_Translations_Key_Language_Market",
                table: "Translations",
                columns: new[] { "Key", "Language", "Market" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Translations");

            migrationBuilder.DropIndex(
                name: "IX_Users_TenantCode",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Groups_TenantCode",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "TenantCode",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantCode",
                table: "Groups");
        }
    }
}
