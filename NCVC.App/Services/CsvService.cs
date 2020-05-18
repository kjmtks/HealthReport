using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using NCVC.App.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NCVC.App.Services
{
    public partial class CsvService
    {
        public DatabaseContext Context { get; }
        public EnvironmentVariableService EnvironmentVariable { get; }
        public CsvService(DatabaseContext context, EnvironmentVariableService environmentVariable)
        {
            Context = context;
            EnvironmentVariable = environmentVariable;
        }

        public async Task<(int, int, int, int)> PullCsv(Staff staff, Course course, int? index = null)
        {
            int count = 0;
            int lastIndex = index.HasValue ? index.Value : course.MailBox.ImapMailIndexOffset;
            var addHealthDateResults = new List<AddHealthDateResult>();

            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                var secureSocketOption = course.MailBox.SecurityMode switch
                {
                    "ssl" => SecureSocketOptions.SslOnConnect,
                    "tls" => SecureSocketOptions.StartTls,
                    "none" => SecureSocketOptions.None,
                    _ => SecureSocketOptions.Auto,
                };
                client.Connect(course.MailBox.ImapHost, course.MailBox.ImapPort, secureSocketOption);
                client.Authenticate(course.MailBox.ImapMailUserAccount, course.MailBox.ImapMailUserPassword);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);
                IList<IMessageSummary> messages;
                messages = await inbox.FetchAsync(lastIndex + 1, -1, MessageSummaryItems.Envelope);
                count = messages.Count();
                lastIndex = count > 0 ? messages.Last().Index : -1;

                string normal_subject = EnvironmentVariable.GetMailSubject();
                string infected_subject = EnvironmentVariable.GetMailInfectedSubject();

                foreach (var msg in messages.Where(x => x.Envelope.Subject.Contains(normal_subject) || x.Envelope.Subject.Contains(infected_subject)))
                {
                    var message = await inbox.GetMessageAsync(msg.Index);
                    var received_at = message.Date.DateTime;
                    foreach (var atc in getAttachments(message))
                    {
                        if (Regex.IsMatch(atc.Item1, "\\.csv$"))
                        {
                            using (var r = new StreamReader(atc.Item2, System.Text.Encoding.GetEncoding("Shift_JIS")))
                            {
                                if (!r.EndOfStream)
                                {
                                    var num = 0;
                                    r.ReadLine(); // skip header row
                                    while (!r.EndOfStream)
                                    {
                                        var flg = false;
                                        var line = r.ReadLine().Trim();
                                        var row = line.Split(",").Select(x => x.Trim(new char[] { '"', ' ' })).ToArray();
                                        if(row.Count() <= 2)
                                        {
                                            continue;
                                        }
                                        if (!DateTime.TryParseExact(row[2], new string[] { "yyyy/MM/dd", "yyyy/M/dd", "yyyy/MM/d", "yyyy/M/d" },
                                                System.Globalization.CultureInfo.InvariantCulture,
                                                System.Globalization.DateTimeStyles.None, out var date))
                                        {
                                            continue;
                                        }
                                        if (msg.Envelope.Subject.Contains(normal_subject) && row.Count() >= 16)
                                        {
                                            addHealthDateResults.Add(addHealthData(course, row, date, received_at, msg.Index, false));
                                            flg = true;
                                            num++;
                                        }
                                        if (msg.Envelope.Subject.Contains(infected_subject) && row.Count() >= 19)
                                        {
                                            addHealthDateResults.Add(addHealthData(course, row, date, received_at, msg.Index, true));
                                            flg = true;
                                            num++;
                                        }

                                        if (flg && num % 1000 == 0)
                                        {
                                            await Context.SaveChangesAsync();
                                            Console.WriteLine($"Reading health data from #'{msg.Index}': {num}");
                                        }
                                    }
                                }
                            }
                            await Context.SaveChangesAsync();
                            Console.WriteLine($"Finishing reading health data from #'{msg.Index}'");
                        }
                    }
                }
                client.Disconnect(true);
            }


            if (count > 0)
            {
                var history = new History()
                {
                    Count = count,
                    Operator = staff,
                    OperatedAt = DateTime.Now,
                    LastIndex = lastIndex,
                    CourseId = course.Id
                };
                Context.Add(history);
                Context.SaveChanges();
            }

            return (lastIndex, count, addHealthDateResults.Where(x => x == AddHealthDateResult.AddNewData).Count(), addHealthDateResults.Where(x => x == AddHealthDateResult.UpdateData).Count());
        }

        public enum AddHealthDateResult { AddNewData, UpdateData, Nop }
        private AddHealthDateResult addHealthData(Course course, string[] row, DateTime measuredDate, DateTime received_at, int msg_index, bool isInfected)
        {
            var canOverride = EnvironmentVariable.IsOverrideMode();
            var timeFrames = EnvironmentVariable.GetTimeFrames();

            var hash = row[0];
            var name = row[1];

            var student = Context.CourseStudentAssignments.Include(x => x.Student).Include(x => x.Course)
                .Where(x => x.CourseId == course.Id).Select(x => x.Student).Where(x => x.Hash == hash).FirstOrDefault();
            if (student == null)
            {
                return AddHealthDateResult.Nop;
            }

            string timeFrameName;
            if (received_at.Year == measuredDate.Year && received_at.Month == measuredDate.Month && received_at.Day == measuredDate.Day)
            {
                timeFrameName = timeFrames?.Where(frame => frame.IsIn(received_at))?.FirstOrDefault()?.Name ?? "";
            }
            else
            {
                timeFrameName = timeFrames?.LastOrDefault()?.Name ?? "";
            }

            var health = Context.HealthList.Where(x => x.StudentId == student.Id && x.MeasuredAt == measuredDate && (isInfected || x.TimeFrame == timeFrameName)).FirstOrDefault();
            var existed = health != null;
            if (!existed)
            {
                health = new Health()
                {
                    RawUserId = hash,
                    RawUserName = name,
                    MeasuredAt = measuredDate,
                    UploadedAt = received_at,
                    Student = student,
                    MailIndex = msg_index,
                    IsInfected = isInfected,
                    IsEmptyData = false
                };
            }
            else if (canOverride)
            {
                health.RawUserId = hash;
                health.RawUserName = name;
                health.MeasuredAt = measuredDate;
                health.UploadedAt = received_at;
                health.Student = student;
                health.MailIndex = msg_index;
                health.IsInfected = isInfected;
                health.IsEmptyData = false;
            }
            if(health == null)
            {
                return AddHealthDateResult.Nop;
            }

            if (!isInfected)
            {
                if (!decimal.TryParse(row[3].Trim(), out var tmpr))
                {
                    tmpr = 0;
                }
                health.TimeFrame = timeFrameName;
                health.BodyTemperature = tmpr;
                health.StringColumn1 = row[4].Trim();
                health.StringColumn2 = row[5].Trim();
                health.StringColumn3 = row[6].Trim();
                health.StringColumn4 = row[7].Trim();
                health.StringColumn5 = row[8].Trim();
                health.StringColumn6 = row[9].Trim();
                health.StringColumn7 = row[10].Trim();
                health.StringColumn8 = row[11].Trim();
                health.StringColumn9 = row[12].Trim();
                health.StringColumn10 = row[13].Trim();
                health.StringColumn11 = row[14].Trim();
                health.StringColumn12 = row[15].Trim();
                health.HasError = health.HasWrongValue();
                health.HasWarning = health.HasWarnValue();
            }
            else
            {
                if (!TimeSpan.TryParse(row[3].Trim(), out var time1))
                {
                    time1 = new TimeSpan(0, 0, 0);
                }
                if (!decimal.TryParse(row[4].Trim(), out var tmpr1))
                {
                    tmpr1 = 0;
                }
                if (string.IsNullOrWhiteSpace(row[5].Trim()) || !int.TryParse(row[5].Trim(), out var ox1))
                {
                    ox1 = -1;
                }
                if (!TimeSpan.TryParse(row[6].Trim(), out var time2))
                {
                    time2 = new TimeSpan(0, 0, 0);
                }
                if (!decimal.TryParse(row[7].Trim(), out var tmpr2))
                {
                    tmpr2 = 0;
                }
                if (string.IsNullOrWhiteSpace(row[8].Trim()) || int.TryParse(row[8].Trim(), out var ox2))
                {
                    ox2 = -1;
                }
                health.InfectedBodyTemperature1 = tmpr1;
                health.InfectedBodyTemperature2 = tmpr2;
                health.InfectedMeasuredTime1 = time1;
                health.InfectedMeasuredTime2 = time2;
                health.InfectedOxygenSaturation1 = ox1;
                health.InfectedOxygenSaturation2 = ox2;

                health.InfectedStringColumn1 = row[9].Trim();
                health.InfectedStringColumn2 = row[10].Trim();
                health.InfectedStringColumn3 = row[11].Trim();
                health.InfectedStringColumn4 = row[12].Trim();
                health.InfectedStringColumn5 = row[13].Trim();
                health.InfectedStringColumn6 = row[14].Trim();
                health.InfectedStringColumn7 = row[15].Trim();
                health.InfectedStringColumn8 = row[16].Trim();
                health.InfectedStringColumn9 = row[17].Trim();
                health.InfectedStringColumn10 = row[18].Trim();
            }
            if (!existed)
            {
                Context.Add(health);
                return AddHealthDateResult.AddNewData;
            }
            else
            {
                Context.Update(health);
                return AddHealthDateResult.UpdateData;
            }
        }

        private static IEnumerable<(string, MemoryStream)> getAttachments(MimeMessage message)
        {
            var attachments = new List<(string, MemoryStream)>();

            foreach (var attachment in message.Attachments)
            {
                string filename;
                var mem = new MemoryStream();
                if (attachment is MessagePart)
                {
                    filename = attachment.ContentDisposition?.FileName ?? "noname";
                    var part = (MessagePart)attachment;

                    part.Message.WriteTo(mem);
                }
                else
                {
                    var part = (MimePart)attachment;
                    filename = part.FileName;

                    part.Content.DecodeTo(mem);
                }
                mem.Seek(0, SeekOrigin.Begin);
                attachments.Add((filename, mem));
            }
            return attachments;
        }
    }
}


