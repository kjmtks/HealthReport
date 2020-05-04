using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NCVC.App.Models;

namespace NCVC.App.Services
{
    public partial class EnvironmentVariableService
    {

        public bool IsOverrideMode()
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OVERRIDE"));
        }

        public string GetTitle()
        {
            return Environment.GetEnvironmentVariable("TITLE") ?? "Health Report";
        }
        public string GetMailSubject()
        {
            return Environment.GetEnvironmentVariable("MAIL_SUBJECT") ?? "健康フォローアップ用健康観察データの報告";
        }
        public string GetMailInfectedSubject()
        {
            return Environment.GetEnvironmentVariable("MAIL_INFECTED_SUBJECT") ?? "【感染者用】健康観察データの報告";
        }

        public string GetSubdir()
        {
            return Environment.GetEnvironmentVariable("SUBDIR") ?? "";
        }

        public bool IsShowUnsubmittedUsers()
        {
            return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SHOW_UNSUBMITTED_USERS"));
        }

        public IEnumerable<TimeFrame> GetTimeFrames()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TIMEFRAME")))
            {
                return null;
            }
            else
            {
                return Environment.GetEnvironmentVariable("TIMEFRAME").Split(";", StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim())
                    .Select(x =>
                    {
                        var xs = x.Split(" ", 2).Select(x => x.Trim()).ToArray();
                        var name = xs[0];
                        xs = xs[1].Split("-").Select(x => x.Trim()).ToArray();
                        if(!TimeSpan.TryParse(xs[0], out var start))
                        {
                            return null;
                        }
                        if(!TimeSpan.TryParse(xs[1], out var end))
                        {
                            return null;
                        }
                        return new TimeFrame()
                        {
                            Name = name,
                            Start = new Time() { Hour = start.Hours, Minite = start.Minutes, Second = start.Seconds },
                            End = new Time() { Hour = end.Hours, Minite = end.Minutes, Second = end.Seconds },
                        };
                    })
                    .Where(x => x != null);
            }
        }
    }
}
