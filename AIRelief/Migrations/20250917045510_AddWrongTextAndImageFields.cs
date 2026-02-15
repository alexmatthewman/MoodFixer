using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class AddWrongTextAndImageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "wrongimage",
                table: "Questions",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "wrongtext",
                table: "Questions",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "wrongimage",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "wrongtext",
                table: "Questions");
        }
    }
}
