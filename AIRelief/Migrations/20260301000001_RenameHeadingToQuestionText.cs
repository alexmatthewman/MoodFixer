using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class RenameHeadingToQuestionText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "heading",
                table: "Questions",
                newName: "QuestionText");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuestionText",
                table: "Questions",
                newName: "heading");
        }
    }
}
