using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using NCVC.App.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Text;

namespace NCVC.App.Models
{
    public class SeedData
    {
        public static void Initialize(DatabaseContext context, IConfiguration config)
        { 
            var t = context.Staffs.AnyAsync();
            t.Wait();
            if (t.Result)
            {
                // DB has been seeded  
                return;
            }
            var staff = new Staff()
            {
                Account = "admin",
                Name = "管理者",
                EncryptedPassword = Staff.Encrypt("password", config),
                IsAdmin = true,
            };
            context.Add(staff);
            context.SaveChanges();
        }
    }
}
