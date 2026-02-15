using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class ChangedToOptionsAndCorrectionText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Answer1",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Answer2",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Answer3",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Answer4",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Answer5",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Answer6",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Answer7",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Answer8",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "wrongtext",
                table: "Questions",
                newName: "correctiontext");

            migrationBuilder.RenameColumn(
                name: "wrongimage",
                table: "Questions",
                newName: "correctionimage");

            migrationBuilder.AlterColumn<int>(
                name: "order",
                table: "Questions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "maintext",
                table: "Questions",
                type: "TEXT",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrectAnswer",
                table: "Questions",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Option1",
                table: "Questions",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Option2",
                table: "Questions",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Option3",
                table: "Questions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option4",
                table: "Questions",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Option5",
                table: "Questions",
                type: "TEXT",
                maxLength: 500,
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectAnswer",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Option1",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Option2",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Option3",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Option4",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Option5",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Option6",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Option7",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Option8",
                table: "Questions");

            migrationBuilder.RenameColumn(
                name: "correctiontext",
                table: "Questions",
                newName: "wrongtext");

            migrationBuilder.RenameColumn(
                name: "correctionimage",
                table: "Questions",
                newName: "wrongimage");

            migrationBuilder.AlterColumn<int>(
                name: "order",
                table: "Questions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "maintext",
                table: "Questions",
                type: "TEXT",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<string>(
                name: "Answer1",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer2",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer3",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer4",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer5",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer6",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer7",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Answer8",
                table: "Questions",
                type: "TEXT",
                nullable: true);
        }
    }
}
