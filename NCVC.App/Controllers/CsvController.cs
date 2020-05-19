using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NCVC.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using NCVC.App.Models;
using System.Text;
using System;

namespace NCVC.App.Controllers
{

    [Authorize]
    public class CsvController : Controller
    {
        DatabaseService DB;
        EnvironmentVariableService EV;
        public CsvController(DatabaseService db, EnvironmentVariableService ev)
        {
            DB = db;
            EV = ev;
        }

        [HttpGet("course/{courseId}/report.csv")]
        public IActionResult CsvFile(string courseId, [FromQuery]string filterString)
        {
            Console.WriteLine($"courseId: {courseId}");
            Console.WriteLine($"filterString: {filterString}");
            var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : "";
            if (!int.TryParse(courseId, out var id))
            {
                return new NotFoundResult();
            }
            var staff = DB.Context.Staffs.Where(x => x.Account == HttpContext.User.Identity.Name).FirstOrDefault();
            if (staff == null)
            {
                return new NotFoundResult();
            }
            var isTestMode = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEST_MODE"));
            var course = DB.Context.Courses.ToArray().Where(x => x.Id == id && (isTestMode || staff.IsAdmin || x.AssignedStaffAccounts().Contains(HttpContext.User.Identity.Name))).FirstOrDefault();
            if (course == null)
            {
                return new NotFoundResult();
            }

            var (count, list) = Health.Search(DB.Context, EV, course.Id, filterString ?? "");
            byte[] result;
            using (var ms = new MemoryStream())
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var tfs = EV.GetTimeFrames();
                var hasTimeFrames = tfs != null ? EV.GetTimeFrames()?.Count() > 0 : false;

                using (var w = new StreamWriter(ms, Encoding.GetEncoding("Shift_JIS")))
                {
                    w.Write("観察者番号,");
                    w.Write("名前,");
                    w.Write("日付,");
                    if (hasTimeFrames)
                    {
                        w.Write("時間帯,");
                    }
                    w.Write("体温,");
                    w.Write("せき,");
                    w.Write("息苦しさ,");
                    w.Write("鼻水,");
                    w.Write("のどの痛み,");
                    w.Write("体のだるさ,");
                    w.Write("下痢,");
                    w.Write("頭痛,");
                    w.Write("その他風邪症状,");
                    w.Write("その他症状詳細,");
                    w.Write("解熱剤・せき止め薬・かぜ薬等の服用,");
                    w.Write("検査実施,");
                    w.Write("検査結果,");
                    w.WriteLine();
                    foreach (var health in list.Where(x => !x.IsInfected))
                    {
                        w.Write($"{health.Student.Account},");
                        w.Write($"{health.Student.Name},");
                        w.Write($"{health.MeasuredAt.ToShortDateString()},");
                        if (hasTimeFrames)
                        {
                            w.Write($"{health.TimeFrame},");
                        }
                        if (health.IsEmptyData)
                        {
                            w.Write($",,,,,,,,,,,,,");
                        }
                        else
                        {
                            w.Write($"{health.BodyTemperature},");
                            w.Write($"{health.StringColumn1},");
                            w.Write($"{health.StringColumn2},");
                            w.Write($"{health.StringColumn3},");
                            w.Write($"{health.StringColumn4},");
                            w.Write($"{health.StringColumn5},");
                            w.Write($"{health.StringColumn6},");
                            w.Write($"{health.StringColumn7},");
                            w.Write($"{health.StringColumn8},");
                            w.Write($"{health.StringColumn9},");
                            w.Write($"{health.StringColumn10},");
                            w.Write($"{health.StringColumn11},");
                            w.Write($"{health.StringColumn12},");
                        }
                        w.WriteLine();
                    }
                }
                result = ms.ToArray();
            }
            return File(result, "text/csv");
        }

        [HttpGet("course/{courseId}/report-infected.csv")]
        public IActionResult InfectedCsvFile(string courseId, [FromQuery]string filterString)
        {
            var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : "";
            if (!int.TryParse(courseId, out var id))
            {
                return new NotFoundResult();
            }
            var staff = DB.Context.Staffs.Where(x => x.Account == HttpContext.User.Identity.Name).FirstOrDefault();
            if(staff == null)
            {
                return new NotFoundResult();
            }
            var isTestMode = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TEST_MODE"));
            var course = DB.Context.Courses.ToArray().Where(x => x.Id == id && (isTestMode || staff.IsAdmin || x.AssignedStaffAccounts().Contains(HttpContext.User.Identity.Name))).FirstOrDefault();
            if (course == null)
            {
                return new NotFoundResult();
            }

            var (count, list) = Health.Search(DB.Context, EV, course.Id, filterString ?? "");
            byte[] result;
            using (var ms = new MemoryStream())
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var hasTimeFrames = EV.GetTimeFrames().Count() > 0;

                using (var w = new StreamWriter(ms, Encoding.GetEncoding("Shift_JIS")))
                {
                    w.Write("観察者番号,");
                    w.Write("名前,");
                    w.Write("日付,");
                    w.Write("記録時刻1回目,");
                    w.Write("体温1回目,");
                    w.Write("酸素飽和度1回目,");
                    w.Write("記録時刻2回目,");
                    w.Write("体温2回目,");
                    w.Write("酸素飽和度2回目,");
                    w.Write("せき,");
                    w.Write("息苦しさ,");
                    w.Write("体のだるさ,");
                    w.Write("吐き気,");
                    w.Write("下痢,");
                    w.Write("意識障害,");
                    w.Write("食事が食べられない,");
                    w.Write("尿が出ていない,");
                    w.Write("その他の症状,");
                    w.Write("具体の症状,");
                    w.WriteLine();
                    foreach (var health in list.Where(x => x.IsInfected))
                    {
                        w.Write($"{health.Student.Account},");
                        w.Write($"{health.Student.Name},");
                        w.Write($"{health.MeasuredAt.ToShortDateString()},");
                        if(health.IsEmptyData)
                        {
                            w.Write($",,,,,,,,,,,,,,,,");
                        }
                        else
                        {
                            w.Write($"{health.InfectedMeasuredTime1},");
                            w.Write($"{health.InfectedBodyTemperature1},");
                            if(health.InfectedOxygenSaturation1 < 0)
                            {
                                w.Write(",");
                            }
                            else
                            {
                                w.Write($"{health.InfectedOxygenSaturation1},");
                            }
                            w.Write($"{health.InfectedMeasuredTime2},");
                            w.Write($"{health.InfectedBodyTemperature2},");
                            if (health.InfectedOxygenSaturation2 < 0)
                            {
                                w.Write(",");
                            }
                            else
                            {
                                w.Write($"{health.InfectedOxygenSaturation2},");
                            }
                            w.Write($"{health.InfectedStringColumn1},");
                            w.Write($"{health.InfectedStringColumn2},");
                            w.Write($"{health.InfectedStringColumn3},");
                            w.Write($"{health.InfectedStringColumn4},");
                            w.Write($"{health.InfectedStringColumn5},");
                            w.Write($"{health.InfectedStringColumn6},");
                            w.Write($"{health.InfectedStringColumn7},");
                            w.Write($"{health.InfectedStringColumn8},");
                            w.Write($"{health.InfectedStringColumn9},");
                            w.Write($"{health.InfectedStringColumn10},");
                        }
                        w.WriteLine();
                    }
                }
                result = ms.ToArray();
            }
            return File(result, "text/csv");
        }

    }
}
