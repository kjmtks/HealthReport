using Microsoft.EntityFrameworkCore.Migrations;

namespace NCVC.App.Migrations
{
    public partial class RemoveMailSubjectFromCourse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImapMailSubject",
                table: "Course");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImapMailSubject",
                table: "Course",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}
