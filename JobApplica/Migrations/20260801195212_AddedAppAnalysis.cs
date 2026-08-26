using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplica.Migrations
{
    /// <inheritdoc />
    public partial class AddedAppAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    JobApplicationId = table.Column<int>(type: "int", nullable: false),
                    MatchScore = table.Column<int>(type: "int", nullable: false),
                    MatchingSkills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MissingSkills = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Suggestions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAnalyses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationAnalyses_Jobapplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "Jobapplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ApplicationAnalyses_Resumes_ResumeId",
                        column: x => x.ResumeId,
                        principalTable: "Resumes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAnalyses_JobApplicationId",
                table: "ApplicationAnalyses",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAnalyses_ResumeId",
                table: "ApplicationAnalyses",
                column: "ResumeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationAnalyses");
        }
    }
}
