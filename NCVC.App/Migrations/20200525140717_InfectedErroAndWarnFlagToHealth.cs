using Microsoft.EntityFrameworkCore.Migrations;

namespace NCVC.App.Migrations
{
    public partial class InfectedErroAndWarnFlagToHealth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasInfectedError",
                table: "Health",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasInfectedWarning",
                table: "Health",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasInfectedError",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "HasInfectedWarning",
                table: "Health");
        }
    }
}
