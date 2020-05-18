using Microsoft.EntityFrameworkCore.Migrations;

namespace NCVC.App.Migrations
{
    public partial class MoveLastMailIndexToMailBox : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_Course_CourseId",
                table: "History");

            migrationBuilder.DropIndex(
                name: "IX_History_CourseId",
                table: "History");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "History");

            migrationBuilder.AddColumn<int>(
                name: "MailBoxId",
                table: "History",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_History_MailBoxId",
                table: "History",
                column: "MailBoxId");

            migrationBuilder.AddForeignKey(
                name: "FK_History_MailBox_MailBoxId",
                table: "History",
                column: "MailBoxId",
                principalTable: "MailBox",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_History_MailBox_MailBoxId",
                table: "History");

            migrationBuilder.DropIndex(
                name: "IX_History_MailBoxId",
                table: "History");

            migrationBuilder.DropColumn(
                name: "MailBoxId",
                table: "History");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "History",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_History_CourseId",
                table: "History",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_History_Course_CourseId",
                table: "History",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
