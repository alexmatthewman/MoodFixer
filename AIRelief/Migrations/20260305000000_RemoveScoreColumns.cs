#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace AIRelief.Migrations
{
    /// <inheritdoc />
    public partial class RemoveScoreColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "CausalReasoningScore",       table: "UserStatistics");
            migrationBuilder.DropColumn(name: "CognitiveReflectionScore",   table: "UserStatistics");
            migrationBuilder.DropColumn(name: "ConfidenceCalibrationScore", table: "UserStatistics");
            migrationBuilder.DropColumn(name: "MetacognitionScore",         table: "UserStatistics");
            migrationBuilder.DropColumn(name: "ReadingComprehensionScore",  table: "UserStatistics");
            migrationBuilder.DropColumn(name: "ShortTermMemoryScore",       table: "UserStatistics");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(name: "CausalReasoningScore",       table: "UserStatistics", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "CognitiveReflectionScore",   table: "UserStatistics", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "ConfidenceCalibrationScore", table: "UserStatistics", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "MetacognitionScore",         table: "UserStatistics", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "ReadingComprehensionScore",  table: "UserStatistics", nullable: false, defaultValue: 0);
            migrationBuilder.AddColumn<int>(name: "ShortTermMemoryScore",       table: "UserStatistics", nullable: false, defaultValue: 0);
        }
    }
}
