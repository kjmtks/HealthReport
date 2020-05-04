using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NCVC.App.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace NCVC.App.Models
{

    public enum DateSpan
    {
        Day, Week, Month
    }
    public class Health
    {
        public int Id { get; set; }
        public string RawUserId { get; set; }
        public string RawUserName { get; set; }

        [Column(TypeName = "Date")]
        public DateTime MeasuredAt { get; set; }
        public DateTime UploadedAt { get; set; }
        public string TimeFrame { get; set; }

        public decimal BodyTemperature { get; set; }
        public string StringColumn1 { get; set; }
        public string StringColumn2{ get; set; }
        public string StringColumn3 { get; set; }
        public string StringColumn4 { get; set; }
        public string StringColumn5 { get; set; }
        public string StringColumn6 { get; set; }
        public string StringColumn7 { get; set; }
        public string StringColumn8 { get; set; }
        public string StringColumn9 { get; set; }
        public string StringColumn10 { get; set; }
        public string StringColumn11 { get; set; }
        public string StringColumn12 { get; set; }

        public bool IsInfected { get; set; } = false;
        public TimeSpan InfectedMeasuredTime1 { get; set; }
        public TimeSpan InfectedMeasuredTime2 { get; set; }
        public decimal InfectedBodyTemperature1 { get; set; }
        public decimal InfectedBodyTemperature2 { get; set; }
        public int InfectedOxygenSaturation1 { get; set; }
        public int InfectedOxygenSaturation2 { get; set; }
        public string InfectedStringColumn1 { get; set; }
        public string InfectedStringColumn2 { get; set; }
        public string InfectedStringColumn3 { get; set; }
        public string InfectedStringColumn4 { get; set; }
        public string InfectedStringColumn5 { get; set; }
        public string InfectedStringColumn6 { get; set; }
        public string InfectedStringColumn7 { get; set; }
        public string InfectedStringColumn8 { get; set; }
        public string InfectedStringColumn9 { get; set; }
        public string InfectedStringColumn10 { get; set; }


        public int MailIndex { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        public bool IsEmptyData { get; set; } = false;


        public bool IsWrongBodyTemperature() => !IsEmptyData && BodyTemperature >= (decimal)37.5;
        public bool IsWarnBodyTemperature() => !IsEmptyData && BodyTemperature >= (decimal)37;
        public bool IsWrongStringColumn1() => !IsEmptyData && StringColumn1 != "無" && StringColumn1 != "N";
        public bool IsWrongStringColumn2() => !IsEmptyData && StringColumn2 != "無" && StringColumn2 != "N";
        public bool IsWrongStringColumn3() => !IsEmptyData && StringColumn3 != "無" && StringColumn3 != "N";
        public bool IsWrongStringColumn4() => !IsEmptyData && StringColumn4 != "無" && StringColumn4 != "N";
        public bool IsWrongStringColumn5() => !IsEmptyData && StringColumn5 != "無" && StringColumn5 != "N";
        public bool IsWrongStringColumn6() => !IsEmptyData && StringColumn6 != "無" && StringColumn6 != "N";
        public bool IsWrongStringColumn7() => !IsEmptyData && StringColumn7 != "無" && StringColumn7 != "N";
        public bool IsWrongStringColumn8() => !IsEmptyData && StringColumn8 != "無" && StringColumn8 != "N";
        public bool IsWrongStringColumn9() => !IsEmptyData && !string.IsNullOrWhiteSpace(StringColumn9);
        public bool IsWrongStringColumn10() => !IsEmptyData && StringColumn10 != "N";
        public bool IsWrongStringColumn11() => !IsEmptyData && StringColumn11 != "N";
        public bool IsWrongStringColumn12() => !IsEmptyData && !string.IsNullOrWhiteSpace(StringColumn12);
        public bool HasWarnValue() => IsWarnBodyTemperature() && !HasWrongValue();
        public bool HasWrongValue() => IsWrongBodyTemperature()
            | IsWrongStringColumn1()
            | IsWrongStringColumn2()
            | IsWrongStringColumn3()
            | IsWrongStringColumn4()
            | IsWrongStringColumn5()
            | IsWrongStringColumn6()
            | IsWrongStringColumn7()
            | IsWrongStringColumn8()
            | IsWrongStringColumn9()
            | IsWrongStringColumn10()
            | IsWrongStringColumn11()
            | IsWrongStringColumn12();


        public static IEnumerable<Student> UnsubmittedStudents(DatabaseContext context, int courseId, DateTime date, TimeFrame timeframe = null)
        {
            var course = context.Courses.Include(x => x.StudentAssignments).ThenInclude(x => x.Student).Where(x => x.Id == courseId).FirstOrDefault();
            var students = course.StudentAssignments.Select(x => x.Student.Account);

            string[] existedStudents;
            if(timeframe != null)
            {
                existedStudents = context.HealthList.Include(x => x.Student).Where(x => x.MeasuredAt == date && x.TimeFrame == timeframe.Name).Select(x => x.Student.Account).ToArray();
            }
            else
            {
                existedStudents = context.HealthList.Include(x => x.Student).Where(x => x.MeasuredAt == date).Select(x => x.Student.Account).ToArray();
            }

            var unsubmitted = students.Except(existedStudents);
            return context.Students.Where(x => unsubmitted.Contains(x.Account)).OrderBy(x => x.Account);
        }
        public static IEnumerable<string> UnregisteredAccounts(DatabaseContext context, int courseId)
        {
            var course = context.Courses.Include(x => x.StudentAssignments).ThenInclude(x => x.Student).Where(x => x.Id == courseId).FirstOrDefault();
            var students = course.StudentAssignments.Select(x => x.Student.Account);

            return course.StudentAssignments.Select(x => x.Student.Account).Except(students);
        }

        private static DateTime? parseDateRhs(string dateStr)
        {
            if(dateStr.Contains("+"))
            {
                var xs = dateStr.Split("+", 2);
                var baseDate = parseDate(xs[0]);
                if(!baseDate.HasValue)
                {
                    return null;
                }
                if (!int.TryParse(xs[1], out var val))
                {
                    return null;
                }
                switch (baseDate.Value.Item2)
                {
                    case DateSpan.Day: return baseDate.Value.Item1.AddDays(val);
                    case DateSpan.Week: return baseDate.Value.Item1.AddDays(7*val);
                    case DateSpan.Month: return baseDate.Value.Item1.AddMonths(val);
                    default: return null;
                }
            }
            else if (dateStr.Contains("-"))
            {
                var xs = dateStr.Split("-", 2);
                var baseDate = parseDate(xs[0]);
                if (!baseDate.HasValue)
                {
                    return null;
                }
                if (!int.TryParse(xs[1], out var val))
                {
                    return null;
                }
                switch (baseDate.Value.Item2)
                {
                    case DateSpan.Day: return baseDate.Value.Item1.AddDays(-val);
                    case DateSpan.Week: return baseDate.Value.Item1.AddDays(-7 * val);
                    case DateSpan.Month: return baseDate.Value.Item1.AddMonths(-val);
                    default: return null;
                }
            }
            else
            {
                var baseDate = parseDate(dateStr);
                if (!baseDate.HasValue)
                {
                    return null;
                }
                return baseDate.Value.Item1;
            }
        }
        private static (DateTime, DateSpan)? parseDate(string dateStr)
        {
            if (dateStr == "today")
            {
                return (DateTime.Today, DateSpan.Day);
            }
            else if (dateStr == "thisweek")
            {
                var today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                var start = today.AddDays(-1 * diff).Date;
                return (start, DateSpan.Week);
            }
            else if (dateStr == "thismonth")
            {
                var today = DateTime.Today;
                var start = new DateTime(today.Year, today.Month, 1);
                return (start, DateSpan.Month);
            }
            else
            {
                if (DateTime.TryParse(dateStr, out var date))
                {
                    return (date, DateSpan.Day);
                }
                else
                {
                    return null;
                }
            }
        }

        public static (int, IQueryable<Health>) Search(DatabaseContext context, EnvironmentVariableService ev, int courseId, string filterString, int? page = null, int? numPerPage = null)
        {
            var course = context.Courses.Include(x => x.StudentAssignments).ThenInclude(x => x.Student).Where(x => x.Id == courseId).FirstOrDefault();
            var students = course.StudentAssignments.Select(x => x.Student.Account);

            var fc = new FilterCompiler(filterString);
            return fc.Filtering(context, ev.GetTimeFrames(), course.StartDate, course.NumOfDaysToSearch);
        }
    }
}
