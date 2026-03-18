using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class RenameShortTermMemoryToWorkingMemory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Questions\" SET \"Category\" = 'Working Memory' WHERE \"Category\" = 'Short Term Memory'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"Questions\" SET \"Category\" = 'Short Term Memory' WHERE \"Category\" = 'Working Memory'");
        }
    }
}
