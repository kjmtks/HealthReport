using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NCVC.App.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    NumOfDaysToSearch = table.Column<int>(nullable: false),
                    EmailAddress = table.Column<string>(nullable: false),
                    ImapHost = table.Column<string>(maxLength: 256, nullable: false),
                    ImapPort = table.Column<int>(nullable: false),
                    ImapMailUserAccount = table.Column<string>(maxLength: 256, nullable: false),
                    ImapMailUserPassword = table.Column<string>(maxLength: 256, nullable: false),
                    ImapMailIndexOffset = table.Column<int>(nullable: false),
                    FilterButtons = table.Column<string>(nullable: true),
                    InitialFilter = table.Column<string>(nullable: true),
                    SecurityMode = table.Column<string>(nullable: true),
                    StaffAccounts = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Account = table.Column<string>(maxLength: 32, nullable: false),
                    EncryptedPassword = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 64, nullable: false),
                    IsAdmin = table.Column<bool>(nullable: false),
                    IsInitialized = table.Column<bool>(nullable: false),
                    LdapUser = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Account = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Tags = table.Column<string>(nullable: true),
                    Hash = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "History",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Count = table.Column<int>(nullable: false),
                    OperatorId = table.Column<int>(nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    LastIndex = table.Column<int>(nullable: false),
                    OperatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_History", x => x.Id);
                    table.ForeignKey(
                        name: "FK_History_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_History_Staff_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseStudentAssignment",
                columns: table => new
                {
                    CourseId = table.Column<int>(nullable: false),
                    StudentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseStudentAssignment", x => new { x.StudentId, x.CourseId });
                    table.ForeignKey(
                        name: "FK_CourseStudentAssignment_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseStudentAssignment_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Health",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RawUserId = table.Column<string>(nullable: true),
                    RawUserName = table.Column<string>(nullable: true),
                    MeasuredAt = table.Column<DateTime>(type: "Date", nullable: false),
                    UploadedAt = table.Column<DateTime>(nullable: false),
                    TimeFrame = table.Column<string>(nullable: true),
                    BodyTemperature = table.Column<decimal>(nullable: false),
                    StringColumn1 = table.Column<string>(nullable: true),
                    StringColumn2 = table.Column<string>(nullable: true),
                    StringColumn3 = table.Column<string>(nullable: true),
                    StringColumn4 = table.Column<string>(nullable: true),
                    StringColumn5 = table.Column<string>(nullable: true),
                    StringColumn6 = table.Column<string>(nullable: true),
                    StringColumn7 = table.Column<string>(nullable: true),
                    StringColumn8 = table.Column<string>(nullable: true),
                    StringColumn9 = table.Column<string>(nullable: true),
                    StringColumn10 = table.Column<string>(nullable: true),
                    StringColumn11 = table.Column<string>(nullable: true),
                    StringColumn12 = table.Column<string>(nullable: true),
                    IsInfected = table.Column<bool>(nullable: false),
                    InfectedMeasuredTime1 = table.Column<TimeSpan>(nullable: false),
                    InfectedMeasuredTime2 = table.Column<TimeSpan>(nullable: false),
                    InfectedBodyTemperature1 = table.Column<decimal>(nullable: false),
                    InfectedBodyTemperature2 = table.Column<decimal>(nullable: false),
                    InfectedOxygenSaturation1 = table.Column<int>(nullable: false),
                    InfectedOxygenSaturation2 = table.Column<int>(nullable: false),
                    InfectedStringColumn1 = table.Column<string>(nullable: true),
                    InfectedStringColumn2 = table.Column<string>(nullable: true),
                    InfectedStringColumn3 = table.Column<string>(nullable: true),
                    InfectedStringColumn4 = table.Column<string>(nullable: true),
                    InfectedStringColumn5 = table.Column<string>(nullable: true),
                    InfectedStringColumn6 = table.Column<string>(nullable: true),
                    InfectedStringColumn7 = table.Column<string>(nullable: true),
                    InfectedStringColumn8 = table.Column<string>(nullable: true),
                    InfectedStringColumn9 = table.Column<string>(nullable: true),
                    InfectedStringColumn10 = table.Column<string>(nullable: true),
                    MailIndex = table.Column<int>(nullable: false),
                    StudentId = table.Column<int>(nullable: false),
                    IsEmptyData = table.Column<bool>(nullable: false),
                    HasError = table.Column<bool>(nullable: false),
                    HasWarning = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Health", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Health_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseStudentAssignment_CourseId",
                table: "CourseStudentAssignment",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Health_StudentId",
                table: "Health",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_History_CourseId",
                table: "History",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_History_OperatorId",
                table: "History",
                column: "OperatorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseStudentAssignment");

            migrationBuilder.DropTable(
                name: "Health");

            migrationBuilder.DropTable(
                name: "History");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Staff");
        }
    }
}
