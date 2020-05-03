using Microsoft.EntityFrameworkCore.Migrations;

namespace NCVC.App.Migrations
{
    public partial class AddIsEmptyDataToHealth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmptyData",
                table: "Health",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmptyData",
                table: "Health");
        }
    }
}
