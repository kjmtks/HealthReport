﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NCVC.App.Models
{

    public class Health
    {
        public int Id { get; set; }
        public string RawUserId { get; set; }
        public string RawUserName { get; set; }
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


        public int MailIndex { get; set; }

        //public int CourseId { get; set; }
        //public virtual Course Course { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; }



        public bool IsWrongBodyTemperature() => BodyTemperature >= (decimal)37.5;
        public bool IsWarnBodyTemperature() => BodyTemperature >= (decimal)37;
        public bool IsWrongStringColumn1() => StringColumn1 != "無";
        public bool IsWrongStringColumn2() => StringColumn2 != "無";
        public bool IsWrongStringColumn3() => StringColumn3 != "無";
        public bool IsWrongStringColumn4() => StringColumn4 != "無";
        public bool IsWrongStringColumn5() => StringColumn5 != "無";
        public bool IsWrongStringColumn6() => StringColumn6 != "無";
        public bool IsWrongStringColumn7() => StringColumn7 != "無";
        public bool IsWrongStringColumn8() => StringColumn8 != "無";
        public bool IsWrongStringColumn9() => !string.IsNullOrWhiteSpace(StringColumn9);
        public bool IsWrongStringColumn10() => StringColumn10 != "N";
        public bool IsWrongStringColumn11() => StringColumn11 != "N";
        public bool IsWrongStringColumn12() => !string.IsNullOrWhiteSpace(StringColumn12);
        public bool HasWarnValue() => IsWarnBodyTemperature();
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


        public static IEnumerable<Student> UnsubmittedStudents(DatabaseContext context, int courseId, DateTime date)
        {
            var course = context.Courses.Where(x => x.Id == courseId).FirstOrDefault();
            var students = course.AssignedStudentAccounts();
            var xs = context.HealthList.Include(x => x.Student).Where(x => x.MeasuredAt == date).Select(x => x.Student.Account).ToArray();

            var unsubmitted = students.Except(xs);
            return context.Students.Where(x => unsubmitted.Contains(x.Account)).OrderBy(x => x.Account);
        }
        public static IEnumerable<string> UnregisteredAccounts(DatabaseContext context, int courseId)
        {
            var course = context.Courses.Where(x => x.Id == courseId).FirstOrDefault();
            var students = context.Students.Select(x => x.Account);

            return course.AssignedStudentAccounts().Except(students);
        }

        public static IEnumerable<Health> Search(DatabaseContext context, int courseId, string filterString)
        {
            string condition = "", order = "";
            if (filterString.Contains("order by"))
            {
                var str = filterString.Split("order by");
                if (str.Count() == 2)
                {
                    order = str[0];
                }
                if (str.Count() >= 2)
                {
                    condition = str[0];
                    order = str[1];
                }
            }
            else
            {
                condition = filterString;
            }
            IEnumerable<Health> HealthList = context.HealthList.Include(x => x.Student).OrderBy(x => x.MeasuredAt).AsNoTracking();
            var parser = new Regex("^(?<lhs>[a-zA-Z0-9./]+)(?<comp>(==|!=|<=|>=|<|>))(?<rhs>[^>=<!\\s]+)$");
            var matches = condition.Split(" ").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => parser.Match(x)).Where(x => x.Success);
            foreach (var match in matches)
            {
                if (match.Groups.ContainsKey("lhs") && match.Groups["lhs"].Success && !string.IsNullOrWhiteSpace(match.Groups["lhs"].Value))
                {
                    switch ((match.Groups["lhs"].Value, match.Groups["comp"].Value, match.Groups["rhs"].Value))
                    {
                        case ("error", "==", string errorStr): if (bool.TryParse(errorStr, out bool error1)) HealthList = HealthList.Where(x => x.HasWrongValue() == error1); break;
                        case ("error", "!=", string errorStr): if (bool.TryParse(errorStr, out bool error2)) HealthList = HealthList.Where(x => x.HasWrongValue() != error2); break;
                        case (string errorStr, "==", "error"): if (bool.TryParse(errorStr, out bool error3)) HealthList = HealthList.Where(x => x.HasWrongValue() == error3); break;
                        case (string errorStr, "!=", "error"): if (bool.TryParse(errorStr, out bool error4)) HealthList = HealthList.Where(x => x.HasWrongValue() != error4); break;

                        case ("student", "==", string student): HealthList = HealthList.Where(x => x.Student.Account.StartsWith(student)); break;
                        case ("student", "!=", string student): HealthList = HealthList.Where(x => !x.Student.Account.StartsWith(student)); break;
                        case (string student, "==", "student"): HealthList = HealthList.Where(x => x.Student.Account.StartsWith(student)); break;
                        case (string student, "!=", "student"): HealthList = HealthList.Where(x => !x.Student.Account.StartsWith(student)); break;

                        case ("timeframe", "==", string timeframe): HealthList = HealthList.Where(x => x.TimeFrame == timeframe); break;
                        case ("timeframe", "!=", string timeframe): HealthList = HealthList.Where(x => x.TimeFrame != timeframe); break;
                        case (string timeframe, "==", "timeframe"): HealthList = HealthList.Where(x => x.TimeFrame == timeframe); break;
                        case (string timeframe, "!=", "timeframe"): HealthList = HealthList.Where(x => x.TimeFrame != timeframe); break;

                        case ("date", "==", string dateStr): if (DateTime.TryParse(dateStr, out var date1)) HealthList = HealthList.Where(x => x.MeasuredAt == date1); break;
                        case ("date", "!=", string dateStr): if (DateTime.TryParse(dateStr, out var date2)) HealthList = HealthList.Where(x => x.MeasuredAt != date2); break;
                        case ("date", "<=", string dateStr): if (DateTime.TryParse(dateStr, out var date3)) HealthList = HealthList.Where(x => x.MeasuredAt <= date3); break;
                        case ("date", ">=", string dateStr): if (DateTime.TryParse(dateStr, out var date4)) HealthList = HealthList.Where(x => x.MeasuredAt >= date4); break;
                        case ("date", "<", string dateStr): if (DateTime.TryParse(dateStr, out var date5)) HealthList = HealthList.Where(x => x.MeasuredAt < date5); break;
                        case ("date", ">", string dateStr): if (DateTime.TryParse(dateStr, out var date6)) HealthList = HealthList.Where(x => x.MeasuredAt > date6); break;
                        case (string dateStr, "==", "date"): if (DateTime.TryParse(dateStr, out var date7)) HealthList = HealthList.Where(x => date7 == x.MeasuredAt); break;
                        case (string dateStr, "!=", "date"): if (DateTime.TryParse(dateStr, out var date8)) HealthList = HealthList.Where(x => date8 != x.MeasuredAt); break;
                        case (string dateStr, "<=", "date"): if (DateTime.TryParse(dateStr, out var date9)) HealthList = HealthList.Where(x => date9 <= x.MeasuredAt); break;
                        case (string dateStr, ">=", "date"): if (DateTime.TryParse(dateStr, out var date10)) HealthList = HealthList.Where(x => date10 >= x.MeasuredAt); break;
                        case (string dateStr, "<", "date"): if (DateTime.TryParse(dateStr, out var date11)) HealthList = HealthList.Where(x => date11 < x.MeasuredAt); break;
                        case (string dateStr, ">", "date"): if (DateTime.TryParse(dateStr, out var date12)) HealthList = HealthList.Where(x => date12 > x.MeasuredAt); break;

                        case ("temp", "==", string tempStr): if (decimal.TryParse(tempStr, out var temp1)) HealthList = HealthList.Where(x => x.BodyTemperature == temp1); break;
                        case ("temp", "!=", string tempStr): if (decimal.TryParse(tempStr, out var temp2)) HealthList = HealthList.Where(x => x.BodyTemperature != temp2); break;
                        case ("temp", "<=", string tempStr): if (decimal.TryParse(tempStr, out var temp3)) HealthList = HealthList.Where(x => x.BodyTemperature <= temp3); break;
                        case ("temp", ">=", string tempStr): if (decimal.TryParse(tempStr, out var temp4)) HealthList = HealthList.Where(x => x.BodyTemperature >= temp4); break;
                        case ("temp", "<", string tempStr): if (decimal.TryParse(tempStr, out var temp5)) HealthList = HealthList.Where(x => x.BodyTemperature < temp5); break;
                        case ("temp", ">", string tempStr): if (decimal.TryParse(tempStr, out var temp6)) HealthList = HealthList.Where(x => x.BodyTemperature > temp6); break;
                        case (string tempStr, "==", "temp"): if (decimal.TryParse(tempStr, out var temp7)) HealthList = HealthList.Where(x => temp7 == x.BodyTemperature); break;
                        case (string tempStr, "!=", "temp"): if (decimal.TryParse(tempStr, out var temp8)) HealthList = HealthList.Where(x => temp8 != x.BodyTemperature); break;
                        case (string tempStr, "<=", "temp"): if (decimal.TryParse(tempStr, out var temp9)) HealthList = HealthList.Where(x => temp9 <= x.BodyTemperature); break;
                        case (string tempStr, ">=", "temp"): if (decimal.TryParse(tempStr, out var temp10)) HealthList = HealthList.Where(x => temp10 >= x.BodyTemperature); break;
                        case (string tempStr, "<", "temp"): if (decimal.TryParse(tempStr, out var temp11)) HealthList = HealthList.Where(x => temp11 < x.BodyTemperature); break;
                        case (string tempStr, ">", "temp"): if (decimal.TryParse(tempStr, out var temp12)) HealthList = HealthList.Where(x => temp12 > x.BodyTemperature); break;
                    }

                }
            }

            if (!string.IsNullOrWhiteSpace(order))
            {
                bool firstSort = true;
                IOrderedEnumerable<Health> OrderedHealthList = null;

                foreach (var match in order.Split(" ").Select(x => Regex.Match(x, "^((?<asc>[a-zA-Z0-9]+)|~(?<dsc>[a-zA-Z0-9]+))$")).Where(x => x.Success))
                {
                    if (match.Groups.ContainsKey("asc") && match.Groups["asc"].Success && !string.IsNullOrWhiteSpace(match.Groups["asc"].Value))
                    {
                        if (firstSort)
                        {
                            if (match.Groups["asc"].Value == "student") OrderedHealthList = HealthList.OrderBy(x => x.Student.Account);
                            if (match.Groups["asc"].Value == "timeframe") OrderedHealthList = HealthList.OrderBy(x => x.TimeFrame);
                            if (match.Groups["asc"].Value == "date") OrderedHealthList = HealthList.OrderBy(x => x.MeasuredAt);
                            if (match.Groups["asc"].Value == "temp") OrderedHealthList = HealthList.OrderBy(x => x.BodyTemperature);
                            if (match.Groups["asc"].Value == "error") OrderedHealthList = HealthList.OrderBy(x => x.HasWrongValue());
                        }
                        else
                        {
                            if (match.Groups["asc"].Value == "student") OrderedHealthList = OrderedHealthList.ThenBy(x => x.Student.Account);
                            if (match.Groups["asc"].Value == "timeframe") OrderedHealthList = OrderedHealthList.ThenBy(x => x.TimeFrame);
                            if (match.Groups["asc"].Value == "date") OrderedHealthList = OrderedHealthList.ThenBy(x => x.MeasuredAt);
                            if (match.Groups["asc"].Value == "temp") OrderedHealthList = OrderedHealthList.ThenBy(x => x.BodyTemperature);
                            if (match.Groups["asc"].Value == "error") OrderedHealthList = OrderedHealthList.ThenBy(x => x.HasWrongValue());
                        }
                        firstSort = false;
                    }
                    if (match.Groups.ContainsKey("dsc") && match.Groups["dsc"].Success && !string.IsNullOrWhiteSpace(match.Groups["dsc"].Value))
                    {
                        if (firstSort)
                        {
                            if (match.Groups["dsc"].Value == "student") OrderedHealthList = HealthList.OrderByDescending(x => x.Student.Account);
                            if (match.Groups["dsc"].Value == "timeframe") OrderedHealthList = HealthList.OrderByDescending(x => x.TimeFrame);
                            if (match.Groups["dsc"].Value == "date") OrderedHealthList = HealthList.OrderByDescending(x => x.MeasuredAt);
                            if (match.Groups["dsc"].Value == "temp") OrderedHealthList = HealthList.OrderByDescending(x => x.BodyTemperature);
                            if (match.Groups["dsc"].Value == "error") OrderedHealthList = HealthList.OrderByDescending(x => x.HasWrongValue());
                        }
                        else
                        {
                            if (match.Groups["dsc"].Value == "student") OrderedHealthList = OrderedHealthList.ThenByDescending(x => x.Student.Account);
                            if (match.Groups["dsc"].Value == "timeframe") OrderedHealthList = OrderedHealthList.ThenByDescending(x => x.TimeFrame);
                            if (match.Groups["dsc"].Value == "date") OrderedHealthList = OrderedHealthList.ThenByDescending(x => x.MeasuredAt);
                            if (match.Groups["dsc"].Value == "temp") OrderedHealthList = OrderedHealthList.ThenByDescending(x => x.BodyTemperature);
                            if (match.Groups["dsc"].Value == "error") OrderedHealthList = OrderedHealthList.ThenByDescending(x => x.HasWrongValue());
                        }
                        firstSort = false;
                    }
                }
                if (!firstSort)
                {
                    HealthList = OrderedHealthList.ToArray();
                }
            }

            var course = context.Courses.Where(x => x.Id == courseId).FirstOrDefault();
            var students = course.AssignedStudentAccounts();
            var xs = HealthList.Where(x => students.Contains(x.Student.Account));
            return xs;
        }
    }
}
