using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using NCVC.App.Models;

namespace NCVC.App.Models
{

    public static class DbInitializer
    {
        public static void Initialize(DatabaseContext context)
        {
            context.Database.EnsureCreated();
        }
    }

    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        public DbSet<MailBox> MailBoxes { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Health> HealthList { get; set; }
        public DbSet<History> Histories { get; set; }
        public DbSet<CourseStudentAssignment> CourseStudentAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Staff>().ToTable("Staff");
            modelBuilder.Entity<Staff>().HasKey(x => x.Id);

            modelBuilder.Entity<Student>().ToTable("Student");
            modelBuilder.Entity<Student>().HasKey(x => x.Id);
            modelBuilder.Entity<Student>().HasMany(x => x.HealthList).WithOne(x => x.Student).HasForeignKey(x => x.StudentId);

            modelBuilder.Entity<Course>().ToTable("Course");
            modelBuilder.Entity<Course>().HasKey(x => x.Id);
            modelBuilder.Entity<Course>().HasOne(x => x.MailBox);
            modelBuilder.Entity<Course>().HasMany(x => x.Histories).WithOne(x => x.Course).HasForeignKey(x => x.CourseId);

            modelBuilder.Entity<MailBox>().ToTable("MailBox");
            modelBuilder.Entity<MailBox>().HasKey(x => x.Id);

            modelBuilder.Entity<Health>().ToTable("Health");
            modelBuilder.Entity<Health>().HasKey(x => x.Id);
            modelBuilder.Entity<Health>().HasOne(x => x.Student);

            modelBuilder.Entity<History>().ToTable("History");
            modelBuilder.Entity<History>().HasKey(x => x.Id);
            modelBuilder.Entity<History>().HasOne(x => x.Operator);
            modelBuilder.Entity<History>().HasOne(x => x.Course);

            modelBuilder.Entity<CourseStudentAssignment>().ToTable("CourseStudentAssignment");
            modelBuilder.Entity<CourseStudentAssignment>().HasKey(a => new { a.StudentId, a.CourseId });
            modelBuilder.Entity<CourseStudentAssignment>().HasOne(a => a.Student).WithMany(s => s.CourseAssignments).HasForeignKey(a => a.StudentId);
            modelBuilder.Entity<CourseStudentAssignment>().HasOne(a => a.Course).WithMany(c => c.StudentAssignments).HasForeignKey(a => a.CourseId);
        }
    }
}
