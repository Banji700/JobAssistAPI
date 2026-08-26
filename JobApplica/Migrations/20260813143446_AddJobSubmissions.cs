using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplica.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSubmissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobsubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobApplicationId = table.Column<int>(type: "int", nullable: false),
                    JobSeekerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResumeId = table.Column<int>(type: "int", nullable: false),
                    ApplicationAnalysisId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobsubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobsubmissions_ApplicationAnalyses_ApplicationAnalysisId",
                        column: x => x.ApplicationAnalysisId,
                        principalTable: "ApplicationAnalyses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobsubmissions_AspNetUsers_JobSeekerId",
                        column: x => x.JobSeekerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobsubmissions_Jobapplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "Jobapplications",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Jobsubmissions_Resumes_ResumeId",
                        column: x => x.ResumeId,
                        principalTable: "Resumes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Jobsubmissions_ApplicationAnalysisId",
                table: "Jobsubmissions",
                column: "ApplicationAnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobsubmissions_JobApplicationId",
                table: "Jobsubmissions",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobsubmissions_JobSeekerId",
                table: "Jobsubmissions",
                column: "JobSeekerId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobsubmissions_ResumeId",
                table: "Jobsubmissions",
                column: "ResumeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Jobsubmissions");
        }
    }
}
