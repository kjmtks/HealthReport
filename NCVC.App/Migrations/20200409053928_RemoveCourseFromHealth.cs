using Microsoft.EntityFrameworkCore.Migrations;

namespace NCVC.App.Migrations
{
    public partial class RemoveCourseFromHealth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Health_Course_CourseId",
                table: "Health");

            migrationBuilder.DropIndex(
                name: "IX_Health_CourseId",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Health");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Health",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Health_CourseId",
                table: "Health",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Health_Course_CourseId",
                table: "Health",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
