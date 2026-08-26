using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobApplica.Migrations
{
    /// <inheritdoc />
    public partial class addedcascasde : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationAnalyses_Jobapplications_JobApplicationId",
                table: "ApplicationAnalyses");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationAnalyses_Jobapplications_JobApplicationId",
                table: "ApplicationAnalyses",
                column: "JobApplicationId",
                principalTable: "Jobapplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationAnalyses_Jobapplications_JobApplicationId",
                table: "ApplicationAnalyses");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationAnalyses_Jobapplications_JobApplicationId",
                table: "ApplicationAnalyses",
                column: "JobApplicationId",
                principalTable: "Jobapplications",
                principalColumn: "Id");
        }
    }
}
