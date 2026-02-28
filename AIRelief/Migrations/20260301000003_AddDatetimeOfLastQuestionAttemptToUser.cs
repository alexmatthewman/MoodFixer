
#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class AddDatetimeOfLastQuestionAttemptToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DatetimeOfLastQuestionAttempt",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DatetimeOfLastQuestionAttempt",
                table: "Users");
        }
    }
}
