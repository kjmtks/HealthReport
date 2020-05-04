using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Novell.Directory.Ldap;

namespace NCVC.App.Models
{
    public enum HealthItemValueType
    {
        Decimal = 0, String = 1
    }
    public class CourseStudentAssignment
    {
        public int CourseId { get; set; }
        public int StudentId { get; set; }

        public virtual Course Course { get; set; }
        public virtual Student Student { get; set; }
    }

    public class Course
    {
        public int Id { get; set; }

        [Required, StringLength(64)]
        public string Name { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Today;

        public int NumOfDaysToSearch { get; set; } = 60;
        public virtual ICollection<History> Histories { get; set; } = new List<History>();

        [Required, EmailAddress]
        public string EmailAddress { get; set; }
        [Required, StringLength(256)]
        public string ImapHost { get; set; }
        [Required]
        public int ImapPort { get; set; }
        [Required, StringLength(256)]
        public string ImapMailUserAccount { get; set; }
        [Required, StringLength(256)]
        public string ImapMailUserPassword { get; set; }
        [Required]
        public int ImapMailIndexOffset { get; set; }

        public string FilterButtons { get; set; }

        public string InitialFilter { get; set; } = "date==today";

        public string SecurityMode { get; set; }
        public string StaffAccounts { get; set; }


        [NotMapped]
        public string StudentAccounts { get; set; }
        public virtual ICollection<CourseStudentAssignment> StudentAssignments { get; set; } = new List<CourseStudentAssignment>();



        public Course GetEntityForEditOrRemove(DatabaseContext context, IConfiguration config) =>
            context.Courses.Include(x => x.StudentAssignments).ThenInclude(x => x.Student).Where(x => x.Id == Id).FirstOrDefault();
        public Course GetEntityAsNoTracking(DatabaseContext context, IConfiguration config) =>
            context.Courses.Where(x => x.Id == Id).AsNoTracking().FirstOrDefault();





        public void CreateNew(DatabaseContext context, IConfiguration config)
        {
            context.Add(this);
            context.SaveChanges();

            var newStudents = (StudentAccounts ?? "").Split(new string[] { " ", "\t", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in newStudents)
            {
                Student student = context.Students.Where(x => x.Account == s).FirstOrDefault();
                if (student != null)
                {
                    var a = new CourseStudentAssignment()
                    {
                        CourseId = Id,
                        StudentId = student.Id
                    };
                    context.Add(a);
                }
            }
        }

        public void Update(DatabaseContext context, IConfiguration config, Course previous)
        {
            var newStudents = (StudentAccounts ?? "").Split(new string[] { " ", "\t", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            var oldStudentd = context.CourseStudentAssignments.Include(x => x.Student).Where(x => x.CourseId == Id).Select(x => x.Student.Account).AsNoTracking().ToArray();
            var addStudents = newStudents.Except(oldStudentd);
            var removeStudents = oldStudentd.Except(newStudents);

            foreach (var s in addStudents)
            {
                Student student = context.Students.Where(x => x.Account == s).FirstOrDefault();
                if (student != null)
                {
                    var a = new CourseStudentAssignment()
                    {
                        CourseId = Id,
                        StudentId = student.Id
                    };
                    context.Add(a);
                }
            }
            foreach (var s in removeStudents)
            {
                var a = context.CourseStudentAssignments.Include(x => x.Student).Where(x => x.Student.Account == s).FirstOrDefault();
                if (a != null)
                {
                    StudentAssignments.Remove(a);
                }
            }


            context.Update(this);
        }

        public void Remove(DatabaseContext context, IConfiguration config)
        {
            var me = GetEntityForEditOrRemove(context, config);
            context.Remove(me);
        }

        public IEnumerable<string> AssignedStaffAccounts()
        {
            return StaffAccounts?.Split(new string[] { " ", "\r", "\n", "\t", "," }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0];
        }

        public bool ServerSideValidationOnCreate(DatabaseContext context, IConfiguration config, Action<string, string> AddValidationError)
        {
            bool result = true;
            var same_name_instance = context.Courses.Where(u => u.Name == Name).FirstOrDefault();
            if (same_name_instance != null)
            {
                AddValidationError("Name", "既に同じ名前のコースが登録されています．");
                result = false;
            }
            return result;
        }
        public bool ServerSideValidationOnUpdate(DatabaseContext context, IConfiguration config, Action<string, string> AddValidationError)
        {
            bool result = true;
            var same_name_instance = context.Courses.Where(u => u.Name == Name).FirstOrDefault();
            if (same_name_instance != null && same_name_instance.Id != Id)
            {
                AddValidationError("Name", "既に同じ名前のコースが登録されています．");
                result = false;
            }
            return result;
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Account { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }


        public string Hash { get; set; } // 報告者IDのこと

        public virtual ICollection<Health> HealthList { get; set; } = new List<Health>();

        public virtual ICollection<CourseStudentAssignment> CourseAssignments { get; set; } = new List<CourseStudentAssignment>();

        [NotMapped]
        [RegularExpression(@"^KN[0-9]+$")]
        [Required, StringLength(6, MinimumLength = 5)]
        public string NewHash { get; set; }


        public bool HasTag(string tag)
        {
            return Tags?.Split()?.Contains(tag) ?? false;
        }

        public static (bool, string) LdapAuthenticate(string account, string password)
        {
            var ldap_host = Environment.GetEnvironmentVariable("LDAP_HOST");
            var ldap_port = int.Parse(Environment.GetEnvironmentVariable("LDAP_PORT"));
            var ldap_base = Environment.GetEnvironmentVariable("LDAP_BASE");
            var ldap_id_attr = Environment.GetEnvironmentVariable("LDAP_ID_ATTR");
            var ldap_name_attr = Environment.GetEnvironmentVariable("LDAP_NAME_ATTR");
            var authenticator = new LdapAuthenticator(ldap_host, ldap_port, ldap_base, ldap_id_attr, entry => {
                foreach(LdapAttribute a in entry.getAttributeSet())
                {
                    Console.WriteLine($"{a.Name}: {a.StringValue}");
                }
                var attrs = entry.getAttributeSet();
                var xs = ldap_name_attr.Split(";");
                if (xs.Length == 1)
                {
                    return attrs.getAttribute(xs[0]).StringValue;
                }
                else
                {
                    return attrs.getAttribute(xs[0], xs[1]).StringValue;
                }
            });
            return authenticator.Authenticate(account, password);
        }

        public static string FindStudentNameByLdap(string account, string search_user_account, string search_user_password)
        {
            var ldap_host = Environment.GetEnvironmentVariable("LDAP_HOST");
            var ldap_port = int.Parse(Environment.GetEnvironmentVariable("LDAP_PORT"));
            var ldap_base = Environment.GetEnvironmentVariable("LDAP_BASE");
            var ldap_id_attr = Environment.GetEnvironmentVariable("LDAP_ID_ATTR");
            var ldap_name_attr = Environment.GetEnvironmentVariable("LDAP_NAME_ATTR");


            var authenticator = new LdapAuthenticator(ldap_host, ldap_port, ldap_base, ldap_id_attr, entry => {
                foreach (LdapAttribute a in entry.getAttributeSet())
                {
                    Console.WriteLine($"{a.Name}: {a.StringValue}");
                }
                var attrs = entry.getAttributeSet();
                var xs = ldap_name_attr.Split(";");
                if (xs.Length == 1)
                {
                    return attrs.getAttribute(xs[0]).StringValue;
                }
                else
                {
                    return attrs.getAttribute(xs[0], xs[1]).StringValue;
                }
            });
            return authenticator.FindName(account, search_user_account, search_user_password);
        }


        public Student GetEntityForEditOrRemove(DatabaseContext context, IConfiguration config) =>
            context.Students.Where(x => x.Id == Id).FirstOrDefault();
        public Student GetEntityAsNoTracking(DatabaseContext context, IConfiguration config) =>
            context.Students.Where(x => x.Id == Id).AsNoTracking().FirstOrDefault();

        public void CreateNew(DatabaseContext context, IConfiguration config)
        {
            context.Add(this);
        }

        public void Update(DatabaseContext context, IConfiguration config, Student previous)
        {
            context.Update(this);
        }

        public void Remove(DatabaseContext context, IConfiguration config)
        {
            var me = GetEntityForEditOrRemove(context, config);
            context.Remove(me);
        }

        public bool ServerSideValidationOnCreate(DatabaseContext context, IConfiguration config, Action<string, string> AddValidationError)
        {
            bool result = true;
            return result;
        }
        public bool ServerSideValidationOnUpdate(DatabaseContext context, IConfiguration config, Action<string, string> AddValidationError)
        {
            bool result = true;
            return result;
        }
    }

    public class Staff
    {
        public int Id { get; set; }

        [RegularExpression(@"^[_A-Za-z][_A-Za-z0-9-]+$")]
        [Required, StringLength(32)]
        public string Account { get; set; }
        public string EncryptedPassword { get; set; }

        [Required, StringLength(64)]
        public string Name { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsInitialized { get; set; }
        public bool LdapUser { get; set; }

        [NotMapped]
        public string Password { get; set; }
        public bool Authenticate(string password, IConfiguration config)
        {
            return this.EncryptedPassword == Encrypt(password, config);
        }
        public static string Encrypt(string rawPassword, IConfiguration config)
        {
            using (RijndaelManaged rijndael = new RijndaelManaged())
            {
                rijndael.BlockSize = 128;
                rijndael.KeySize = 128;
                rijndael.Mode = CipherMode.CBC;
                rijndael.Padding = PaddingMode.PKCS7;

                var key = config.GetValue<string>("ApplicationKey");

                rijndael.IV = Encoding.UTF8.GetBytes(key.Substring(0, 16));
                rijndael.Key = Encoding.UTF8.GetBytes(key.Substring(16, 16));
                ICryptoTransform encryptor = rijndael.CreateEncryptor(rijndael.Key, rijndael.IV);

                byte[] encrypted;
                using (MemoryStream mStream = new MemoryStream())
                {
                    using (CryptoStream ctStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(ctStream))
                        {
                            sw.Write(rawPassword);
                        }
                        encrypted = mStream.ToArray();
                    }
                }
                return (System.Convert.ToBase64String(encrypted));
            }
        }

        public Staff GetEntityForEditOrRemove(DatabaseContext context, IConfiguration config) =>
            context.Staffs.Where(x => x.Id == Id).FirstOrDefault();
        public Staff GetEntityAsNoTracking(DatabaseContext context, IConfiguration config) =>
            context.Staffs.Where(x => x.Id == Id).AsNoTracking().FirstOrDefault();

        public void CreateNew(DatabaseContext context, IConfiguration config)
        {
            EncryptedPassword = Encrypt(Password, config);
            context.Add(this);
        }

        public void Update(DatabaseContext context, IConfiguration config, Staff previous)
        {
            if (string.IsNullOrEmpty(this.Password))
            {
                EncryptedPassword = previous.EncryptedPassword;
            }
            else
            {
                EncryptedPassword = Encrypt(Password, config);
            }
            context.Update(this);
        }

        public void Remove(DatabaseContext context, IConfiguration config)
        {
            var me = GetEntityForEditOrRemove(context, config);
            context.Remove(me);
        }

        public bool ServerSideValidationOnCreate(DatabaseContext context, IConfiguration config, Action<string, string> AddValidationError)
        {
            bool result = true;
            var instance = context.Staffs.Where(u => u.Account == Account).FirstOrDefault();
            if (instance != null)
            {
                AddValidationError("Account", "Arleady exist");
                result = false;
            }
            if (string.IsNullOrEmpty(Password))
            {
                AddValidationError("Password", "The Password field is required.");
                result = false;
            }
            return result;
        }
        public bool ServerSideValidationOnUpdate(DatabaseContext context, IConfiguration config, Action<string, string> AddValidationError)
        {
            bool result = true;
            var instance = context.Staffs.Where(u => u.Account == Account).FirstOrDefault();
            if (instance != null && instance.Id != Id)
            {
                AddValidationError("Account", "Arleady exist");
                result = false;
            }
            return result;
        }
    }


    public class History
    {
        public int Id { get; set; }
        public int Count { get; set; }
        public int OperatorId { get; set; }
        public virtual Staff Operator { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int LastIndex { get; set; }
        public DateTime OperatedAt { get; set; }
    }
}
