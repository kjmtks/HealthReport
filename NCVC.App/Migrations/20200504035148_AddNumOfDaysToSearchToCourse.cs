using Microsoft.EntityFrameworkCore.Migrations;

namespace NCVC.App.Migrations
{
    public partial class AddNumOfDaysToSearchToCourse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumOfDaysToSearch",
                table: "Course",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NumOfDaysToSearch",
                table: "Course");
        }
    }
}
