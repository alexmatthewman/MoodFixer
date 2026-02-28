using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class RenameLowercaseQuestionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "maintext",
                table: "Questions",
                newName: "MainText");

            migrationBuilder.RenameColumn(
                name: "image",
                table: "Questions",
                newName: "Image");

            migrationBuilder.RenameColumn(
                name: "explanationtext",
                table: "Questions",
                newName: "ExplanationText");

            migrationBuilder.RenameColumn(
                name: "explanationimage",
                table: "Questions",
                newName: "ExplanationImage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MainText",
                table: "Questions",
                newName: "maintext");

            migrationBuilder.RenameColumn(
                name: "Image",
                table: "Questions",
                newName: "image");

            migrationBuilder.RenameColumn(
                name: "ExplanationText",
                table: "Questions",
                newName: "explanationtext");

            migrationBuilder.RenameColumn(
                name: "ExplanationImage",
                table: "Questions",
                newName: "explanationimage");
        }
    }
}
