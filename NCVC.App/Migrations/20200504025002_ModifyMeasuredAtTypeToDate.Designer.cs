﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NCVC.App.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NCVC.App.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20200504025002_ModifyMeasuredAtTypeToDate")]
    partial class ModifyMeasuredAtTypeToDate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("NCVC.App.Models.Course", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("EmailAddress")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FilterButtons")
                        .HasColumnType("text");

                    b.Property<string>("ImapHost")
                        .IsRequired()
                        .HasColumnType("character varying(256)")
                        .HasMaxLength(256);

                    b.Property<int>("ImapMailIndexOffset")
                        .HasColumnType("integer");

                    b.Property<string>("ImapMailUserAccount")
                        .IsRequired()
                        .HasColumnType("character varying(256)")
                        .HasMaxLength(256);

                    b.Property<string>("ImapMailUserPassword")
                        .IsRequired()
                        .HasColumnType("character varying(256)")
                        .HasMaxLength(256);

                    b.Property<int>("ImapPort")
                        .HasColumnType("integer");

                    b.Property<string>("InitialFilter")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.Property<string>("SecurityMode")
                        .HasColumnType("text");

                    b.Property<string>("StaffAccounts")
                        .HasColumnType("text");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.ToTable("Course");
                });

            modelBuilder.Entity("NCVC.App.Models.CourseStudentAssignment", b =>
                {
                    b.Property<int>("StudentId")
                        .HasColumnType("integer");

                    b.Property<int>("CourseId")
                        .HasColumnType("integer");

                    b.HasKey("StudentId", "CourseId");

                    b.HasIndex("CourseId");

                    b.ToTable("CourseStudentAssignment");
                });

            modelBuilder.Entity("NCVC.App.Models.Health", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<decimal>("BodyTemperature")
                        .HasColumnType("numeric");

                    b.Property<decimal>("InfectedBodyTemperature1")
                        .HasColumnType("numeric");

                    b.Property<decimal>("InfectedBodyTemperature2")
                        .HasColumnType("numeric");

                    b.Property<TimeSpan>("InfectedMeasuredTime1")
                        .HasColumnType("interval");

                    b.Property<TimeSpan>("InfectedMeasuredTime2")
                        .HasColumnType("interval");

                    b.Property<int>("InfectedOxygenSaturation1")
                        .HasColumnType("integer");

                    b.Property<int>("InfectedOxygenSaturation2")
                        .HasColumnType("integer");

                    b.Property<string>("InfectedStringColumn1")
                        .HasColumnType("text");

                    b.Property<string>("InfectedStringColumn10")
                        .HasColumnType("text");

                    b.Property<string>("InfectedStringColumn2")
                        .HasColumnType("text");

                    b.Property<string>("InfectedStringColumn3")
                        .HasColumnType("text");

                    b.Property<string>("InfectedStringColumn4")
                        .HasColumnType("text");

                    b.Property<string>("InfectedStringColumn5")
                        .HasColumnType("text");

                    b.Property<string>("InfectedStringColumn6")
                        .HasColumnType("text");

                    b.Property<string>("InfectedStringColumn7")
                        .HasColumnType("text");

                    b.Property<string>("InfectedStringColumn8")
                        .HasColumnType("text");

                    b.Property<string>("InfectedStringColumn9")
                        .HasColumnType("text");

                    b.Property<bool>("IsEmptyData")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsInfected")
                        .HasColumnType("boolean");

                    b.Property<int>("MailIndex")
                        .HasColumnType("integer");

                    b.Property<DateTime>("MeasuredAt")
                        .HasColumnType("Date");

                    b.Property<string>("RawUserId")
                        .HasColumnType("text");

                    b.Property<string>("RawUserName")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn1")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn10")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn11")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn12")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn2")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn3")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn4")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn5")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn6")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn7")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn8")
                        .HasColumnType("text");

                    b.Property<string>("StringColumn9")
                        .HasColumnType("text");

                    b.Property<int>("StudentId")
                        .HasColumnType("integer");

                    b.Property<string>("TimeFrame")
                        .HasColumnType("text");

                    b.Property<DateTime>("UploadedAt")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("StudentId");

                    b.ToTable("Health");
                });

            modelBuilder.Entity("NCVC.App.Models.History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Count")
                        .HasColumnType("integer");

                    b.Property<int>("CourseId")
                        .HasColumnType("integer");

                    b.Property<int>("LastIndex")
                        .HasColumnType("integer");

                    b.Property<DateTime>("OperatedAt")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("OperatorId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("OperatorId");

                    b.ToTable("History");
                });

            modelBuilder.Entity("NCVC.App.Models.Staff", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Account")
                        .IsRequired()
                        .HasColumnType("character varying(32)")
                        .HasMaxLength(32);

                    b.Property<string>("EncryptedPassword")
                        .HasColumnType("text");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsInitialized")
                        .HasColumnType("boolean");

                    b.Property<bool>("LdapUser")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("character varying(64)")
                        .HasMaxLength(64);

                    b.HasKey("Id");

                    b.ToTable("Staff");
                });

            modelBuilder.Entity("NCVC.App.Models.Student", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Account")
                        .HasColumnType("text");

                    b.Property<string>("Hash")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<string>("Tags")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Student");
                });

            modelBuilder.Entity("NCVC.App.Models.CourseStudentAssignment", b =>
                {
                    b.HasOne("NCVC.App.Models.Course", "Course")
                        .WithMany("StudentAssignments")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NCVC.App.Models.Student", "Student")
                        .WithMany("CourseAssignments")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NCVC.App.Models.Health", b =>
                {
                    b.HasOne("NCVC.App.Models.Student", "Student")
                        .WithMany("HealthList")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NCVC.App.Models.History", b =>
                {
                    b.HasOne("NCVC.App.Models.Course", "Course")
                        .WithMany("Histories")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NCVC.App.Models.Staff", "Operator")
                        .WithMany()
                        .HasForeignKey("OperatorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}