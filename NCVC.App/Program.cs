using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

using NCVC.App.Models;
using Microsoft.Extensions.DependencyInjection;
using NCVC.App.Services;

namespace NCVC.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            CreateDbIfNotExists(host);

            host.Run();
        }

        private static void CreateDbIfNotExists(IHost host) {
            using(var scope = host.Services.CreateScope()) {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<DatabaseContext>();
                    var config = services.GetRequiredService<IConfiguration>();
                    DbInitializer.Initialize(context);
                    SeedData.Initialize(context, config);
                } catch (Exception e) {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(e, "An error occured creating the Database.");
                }
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var env = Environment.GetEnvironmentVariable("PROTOCOL")?.ToLower();
                    if(env == "https")
                    {
                        webBuilder.UseStartup<Startup>().UseUrls("https://*:8080");
                    }
                    else
                    {
                        webBuilder.UseStartup<Startup>().UseUrls("http://*:8080");
                    }
                });
    }
}
