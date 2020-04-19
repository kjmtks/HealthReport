using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Microsoft.EntityFrameworkCore;
using NCVC.App.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using NCVC.App.Services;
using System.Net.Http;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

namespace NCVC.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options => {
                options.CheckConsentNeeded = context => !context.User.Identity.IsAuthenticated;
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
                options.HttpOnly = HttpOnlyPolicy.Always;
            });

            var subdir = Environment.GetEnvironmentVariable("SUBDIR") ?? "";
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Path = subdir;
            });

            services
               .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(options => { options.LoginPath = $"{subdir}/Login"; options.LogoutPath = $"{subdir}/Logout"; });
            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"/data/DataProtectionKey"));

            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddDbContext<DatabaseContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DatabaseContext")));
            services.AddScoped<DatabaseService>();

            services.AddSingleton<NotifierService>();

            services.AddSingleton<EnvironmentVariableService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var subdir = Environment.GetEnvironmentVariable("SUBDIR");
            if (!string.IsNullOrWhiteSpace(subdir))
            {
                app.UsePathBase(subdir);
            }

            app.UseExceptionHandler("/Error");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "../node_modules")),
                RequestPath = "/node_modules"
            });
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapFallbackToPage("/_Host");
            });

            app.UseCookiePolicy();
          
        }
    }
}
