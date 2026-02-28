using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class AddQueryFrequencyToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QueryFrequency",
                table: "Users",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QueryFrequency",
                table: "Users");
        }
    }
}
