using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NCVC.App.Migrations
{
    public partial class AddMailBoxesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailAddress",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ImapHost",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ImapMailIndexOffset",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ImapMailUserAccount",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ImapMailUserPassword",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ImapPort",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "SecurityMode",
                table: "Course");

            migrationBuilder.AddColumn<int>(
                name: "MailBoxId",
                table: "Course",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MailBox",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    EmailAddress = table.Column<string>(nullable: false),
                    ImapHost = table.Column<string>(maxLength: 256, nullable: false),
                    ImapPort = table.Column<int>(nullable: false),
                    ImapMailUserAccount = table.Column<string>(maxLength: 256, nullable: false),
                    ImapMailUserPassword = table.Column<string>(maxLength: 256, nullable: false),
                    ImapMailIndexOffset = table.Column<int>(nullable: false),
                    SecurityMode = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailBox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Course_MailBoxId",
                table: "Course",
                column: "MailBoxId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_MailBox_MailBoxId",
                table: "Course",
                column: "MailBoxId",
                principalTable: "MailBox",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_MailBox_MailBoxId",
                table: "Course");

            migrationBuilder.DropTable(
                name: "MailBox");

            migrationBuilder.DropIndex(
                name: "IX_Course_MailBoxId",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "MailBoxId",
                table: "Course");

            migrationBuilder.AddColumn<string>(
                name: "EmailAddress",
                table: "Course",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImapHost",
                table: "Course",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ImapMailIndexOffset",
                table: "Course",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImapMailUserAccount",
                table: "Course",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ImapMailUserPassword",
                table: "Course",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ImapPort",
                table: "Course",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SecurityMode",
                table: "Course",
                type: "text",
                nullable: true);
        }
    }
}
