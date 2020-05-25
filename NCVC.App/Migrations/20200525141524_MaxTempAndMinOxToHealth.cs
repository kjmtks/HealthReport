using Microsoft.EntityFrameworkCore.Migrations;

namespace NCVC.App.Migrations
{
    public partial class MaxTempAndMinOxToHealth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxInfectedBodyTemperature",
                table: "Health",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinInfectedOxygenSaturation",
                table: "Health",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxInfectedBodyTemperature",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "MinInfectedOxygenSaturation",
                table: "Health");
        }
    }
}
