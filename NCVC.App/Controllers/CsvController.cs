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
        public CsvController(DatabaseService db)
        {
            DB = db;
        }

        [HttpGet("course/{courseId}/report.csv")]
        public IActionResult CsvFile(string courseId, [FromQuery]string filterString)
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
            var course = DB.Context.Courses.Include(x => x.Histories).ToArray().Where(x => x.Id == id && (isTestMode || staff.IsAdmin || x.AssignedStaffAccounts().Contains(HttpContext.User.Identity.Name))).FirstOrDefault();
            if (course == null)
            {
                return new NotFoundResult();
            }

            var list = Health.Search(DB.Context, course.Id, filterString ?? "");
            byte[] result;
            using (var ms = new MemoryStream())
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                using (var w = new StreamWriter(ms, Encoding.GetEncoding("Shift_JIS")))
                {
                    w.Write("アカウント,");
                    w.Write("名前,");
                    w.Write("日付,");
                    if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TIMEFRAME")))
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
                    foreach (var health in list)
                    {
                        w.Write($"{health.Student.Account},");
                        w.Write($"{health.Student.Name},");
                        w.Write($"{health.MeasuredAt.ToShortDateString()},");
                        if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TIMEFRAME")))
                        {
                            w.Write($"{health.TimeFrame},");
                        }
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
                        w.WriteLine();
                    }
                }
                result = ms.ToArray();
            }
            return File(result, "text/csv");
        }

    }
}
