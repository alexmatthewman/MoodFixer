using Microsoft.EntityFrameworkCore.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class UpdateQuestionModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop removed columns
            migrationBuilder.DropColumn(name: "order",        table: "Questions");
            migrationBuilder.DropColumn(name: "backvalue",    table: "Questions");
            migrationBuilder.DropColumn(name: "nextvalue",    table: "Questions");
            migrationBuilder.DropColumn(name: "Option6",      table: "Questions");
            migrationBuilder.DropColumn(name: "Option7",      table: "Questions");
            migrationBuilder.DropColumn(name: "Option8",      table: "Questions");

            // Rename correctiontext -> explanationtext
            migrationBuilder.RenameColumn(
                name: "correctiontext",
                table: "Questions",
                newName: "explanationtext");

            // Rename correctionimage -> explanationimage
            migrationBuilder.RenameColumn(
                name: "correctionimage",
                table: "Questions",
                newName: "explanationimage");

            // Add new columns
            migrationBuilder.AddColumn<int>(
                name: "AttemptsShown",
                table: "Questions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AttemptsCorrect",
                table: "Questions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BestAnswersRaw",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Questions",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            // Set existing questions to Category = 'Trial'
            migrationBuilder.Sql("UPDATE Questions SET Category = 'Trial'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove new columns
            migrationBuilder.DropColumn(name: "AttemptsShown",  table: "Questions");
            migrationBuilder.DropColumn(name: "AttemptsCorrect", table: "Questions");
            migrationBuilder.DropColumn(name: "BestAnswersRaw", table: "Questions");
            migrationBuilder.DropColumn(name: "Category",       table: "Questions");

            // Rename explanationtext -> correctiontext
            migrationBuilder.RenameColumn(
                name: "explanationtext",
                table: "Questions",
                newName: "correctiontext");

            // Rename explanationimage -> correctionimage
            migrationBuilder.RenameColumn(
                name: "explanationimage",
                table: "Questions",
                newName: "correctionimage");

            // Re-add removed columns
            migrationBuilder.AddColumn<int>(
                name: "order",
                table: "Questions",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "backvalue",
                table: "Questions",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nextvalue",
                table: "Questions",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option6",
                table: "Questions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option7",
                table: "Questions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option8",
                table: "Questions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }
    }
}
