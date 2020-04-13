using MailKit;
using MailKit.Net.Imap;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MailKit.Security;

namespace NCVC.App.Models
{
    public class CsvGetter
    {
        public static async Task<(int, int)> LoadReceivedCsv(DatabaseContext context, Course course, Staff operaotr, int lastIndex)
        {
            course = context.Courses.Where(x => x.Id == course.Id).FirstOrDefault();
            var registeredStudentAccounts = course.AssignedStudentAccounts();
            var canOverride = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OVERRIDE"));

            bool flag = false;
            var (data, index, count) = await GetCsvFromIMAP(course.ImapMailUserAccount, course.ImapMailUserPassword, course.ImapHost, course.ImapPort, course.ImapMailSubject, lastIndex, course.SecurityMode);
            foreach (var (rowList, received, idx) in data)
            {
                var row = rowList.ToArray();
                if (row.Count() < 16)
                {
                    continue;
                }

                for (var i = 0; i<row.Length; i++)
                {
                    row[i] = row[i].Trim(new char[] { '"' });
                }

                var hash = row[0];
                var name = row[1];


                var success = DateTime.TryParseExact(row[2], new string[] { "yyyy/MM/dd", "yyyy/M/dd", "yyyy/MM/d", "yyyy/M/d" },
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var date);
                if (!success)
                {
                    continue;
                }

                var student = context.Students.Where(x => x.Hash == hash && registeredStudentAccounts.Contains(x.Account)).FirstOrDefault();
                if (student == null)
                {
                    continue;
                }
                student.LastUploadedAt = received;
                context.Update(student);
                context.SaveChanges();
                var health = context.HealthList.Where(x => x.StudentId == student.Id && x.MeasuredAt == date).FirstOrDefault();
                var existed = health != null;
                if (!existed)
                {
                    health = new Health()
                    {
                        RawUserId = hash,
                        RawUserName = name,
                        MeasuredAt = date,
                        UploadedAt = received,
                        Student = student,
                        MailIndex = idx,
                    };
                }
                else if(canOverride)
                {
                    health.RawUserId = hash;
                    health.RawUserName = name;
                    health.MeasuredAt = date;
                    health.UploadedAt = received;
                    health.Student = student;
                    health.MailIndex = idx;
                }
                if(health != null)
                {
                    if (!decimal.TryParse(row[3].Trim(), out var tmpr))
                    {
                        tmpr = 0;
                    }
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
                    if (!existed)
                    {
                        context.Add(health);
                    }
                    else
                    {
                        context.Update(health);
                    }
                    context.SaveChanges();
                    flag = true;
                }
            }
            if(flag)
            {
                var history = new History()
                {
                    Count = data.Count(),
                    Operator = operaotr,
                    OperatedAt = DateTime.Now,
                    LastIndex = index,
                    Course = course
                };
                context.Add(history);
                context.SaveChanges();
            }

            return (index, data.Count());
        }

        private static async Task<(IEnumerable<(IEnumerable<string>, DateTime, int)>, int, int)> GetCsvFromIMAP(string account, string password, string host, int port, string mail_subject, int min_index, string securityMode)
        {
            int lastIndex = min_index;
            int count = 0;
            var table = new List<(IEnumerable<string>, DateTime, int)>();
            using (var client = new ImapClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                if(securityMode == "ssl")
                {
                    client.Connect(host, port, SecureSocketOptions.SslOnConnect);
                }
                else if (securityMode == "none")
                {
                    client.Connect(host, port, SecureSocketOptions.None);
                }
                else
                {
                    client.Connect(host, port, SecureSocketOptions.Auto);
                }
                client.Authenticate(account, password);



                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);
                IList<IMessageSummary> messages;
                messages = await inbox.FetchAsync(min_index + 1, -1, MessageSummaryItems.Envelope);
                count = messages.Count();
                lastIndex = count > 0 ? messages.Last().Index : -1;
                foreach (var msg in messages.Where(x => x.Envelope.Subject.Contains(mail_subject)))
                {
                    var message = await inbox.GetMessageAsync(msg.Index);
                    var received = message.Date.DateTime;
                    foreach (var atc in getAttachments(message))
                    {
                        if (Regex.IsMatch(atc.Item1, "\\.csv$"))
                        {
                            using (var r = new StreamReader(atc.Item2, System.Text.Encoding.GetEncoding("Shift_JIS")))
                            {
                                if (!r.EndOfStream)
                                {
                                    r.ReadLine(); // for skip header row
                                    while (!r.EndOfStream)
                                    {
                                        var line = r.ReadLine().Trim();
                                        var csv = line.Split(",").Select(x => x.Trim());
                                        table.Add((csv, received, msg.Index));
                                    }
                                }
                            }
                        }
                    }
                }
                client.Disconnect(true);
            }
            return (table, lastIndex, count);
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
