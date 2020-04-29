using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace NCVC.App.Migrations
{
    public partial class AddInfectionAttributesToHealth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "InfectedBodyTemperature1",
                table: "Health",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InfectedBodyTemperature2",
                table: "Health",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "InfectedMeasuredTime1",
                table: "Health",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "InfectedMeasuredTime2",
                table: "Health",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<int>(
                name: "InfectedOxygenSaturation1",
                table: "Health",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InfectedOxygenSaturation2",
                table: "Health",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn1",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn10",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn2",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn3",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn4",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn5",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn6",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn7",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn8",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InfectedStringColumn9",
                table: "Health",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInfected",
                table: "Health",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InfectedBodyTemperature1",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedBodyTemperature2",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedMeasuredTime1",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedMeasuredTime2",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedOxygenSaturation1",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedOxygenSaturation2",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn1",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn10",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn2",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn3",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn4",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn5",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn6",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn7",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn8",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "InfectedStringColumn9",
                table: "Health");

            migrationBuilder.DropColumn(
                name: "IsInfected",
                table: "Health");
        }
    }
}
