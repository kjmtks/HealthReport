using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using NCVC.App.ViewModels;
using NCVC.App.Models;
using NCVC.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NCVC.App.Controllers
{

    [AllowAnonymous]
    public class LoginController : Controller
    {
        DatabaseService DB;
        IConfiguration Config;
        public LoginController(DatabaseService db, IConfiguration config)
        {
            DB = db;
            Config = config;
        }

        [HttpGet("Logout", Name = "Logout")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : "";
            return LocalRedirect($"{pathBase}/Login");
        }

        [HttpGet("Login", Name = "Login")]
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        private enum AuthorizationMode { LdapStaff, DbStaff, LdapStudent, Invalid }

        [HttpPost("Login")]
        public async Task<ActionResult> Login([FromQuery]string ReturnUrl, LoginViewModel m)
        {
            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var useLdap = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("LDAP_HOST"));
            AuthorizationMode mode;
            var staff = DB.Context.Staffs.Where(x => x.Account == m.Name && !x.LdapUser).FirstOrDefault();
            if (staff != null)
            {
                mode = AuthorizationMode.DbStaff;
            }
            else if(useLdap)
            {
                var st = Environment.GetEnvironmentVariable("LDAP_REGEX_STAFF")?.Trim();
                var regex = new Regex(st);
                if (regex.IsMatch(m.Name))
                {
                    mode = AuthorizationMode.LdapStaff;
                }
                else
                {
                    mode = AuthorizationMode.LdapStudent;
                }
            }
            else
            {
                mode = AuthorizationMode.Invalid;
            }

            switch(mode)
            {
                case AuthorizationMode.LdapStudent:
                    return await loginLdapStudent(m.Name, m.Password, m.RememberMe);
                case AuthorizationMode.LdapStaff:
                    return await loginLdapStaff(m.Name, m.Password, m.RememberMe);
                case AuthorizationMode.DbStaff:
                    return await loginDbStaff(m.Name, m.Password, m.RememberMe);
            }

            var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : "";
            return LocalRedirect($"{pathBase}/Login");
        }

        private async Task<ActionResult> loginLdapStudent(string account, string password, bool rememberMe)
        {
            var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : "";
            var claims = new List<Claim>();
            ClaimsIdentity claimsIdentity;

            var (result, name) = Student.LdapAuthenticate(account, password);
            if (!result)
            {
                return LocalRedirect($"{pathBase}/Login");
            }
            claims.Add(new Claim(ClaimTypes.Name, account));
            claims.Add(new Claim(ClaimTypes.Surname, name));
            claims.Add(new Claim(ClaimTypes.Role, "Student"));
            claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
              CookieAuthenticationDefaults.AuthenticationScheme,
              new ClaimsPrincipal(claimsIdentity),
              new AuthenticationProperties
              {
                  IsPersistent = rememberMe
              });
            var student = DB.Context.Students.Where(x => x.Account == account).FirstOrDefault();
            if (student == null)
            {
                student = new Student()
                {
                    Account = account,
                    Name = name
                };
                DB.Context.Add(student);
                DB.Context.SaveChanges();
            }
            return LocalRedirect($"{pathBase}/StudentRegistration");
        }

        private async Task<ActionResult> loginLdapStaff(string account, string password, bool rememberMe)
        {
            var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : "";
            var claims = new List<Claim>();
            ClaimsIdentity claimsIdentity;

            var (result, name) = Student.LdapAuthenticate(account, password);
            if (!result)
            {
                return LocalRedirect($"{pathBase}/Login");
            }
            claims.Add(new Claim(ClaimTypes.Name, account));
            claims.Add(new Claim(ClaimTypes.Surname, name));
            claims.Add(new Claim(ClaimTypes.Role, "Staff"));
            var staff = DB.Context.Staffs.Where(x => x.Account == account && x.LdapUser).FirstOrDefault();
            if (staff == null)
            {
                staff = new Staff()
                {
                    Account = account,
                    Name = name,
                    IsInitialized = true,
                    LdapUser = true
                };
                DB.Context.Add(staff);
                DB.Context.SaveChanges();
            }
            if (staff.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
              CookieAuthenticationDefaults.AuthenticationScheme,
              new ClaimsPrincipal(claimsIdentity),
              new AuthenticationProperties
              {
                  IsPersistent = rememberMe
              });
            return LocalRedirect($"{pathBase}/");
        }

        private async Task<ActionResult> loginDbStaff(string account, string password, bool rememberMe)
        {
            var pathBase = HttpContext.Request.PathBase.HasValue ? HttpContext.Request.PathBase.Value : "";
            var claims = new List<Claim>();
            ClaimsIdentity claimsIdentity;

            var staff = DB.Context.Staffs.Where(x => x.Account == account && !x.LdapUser).FirstOrDefault();
            if (staff == null || !staff.Authenticate(password, Config))
            {
                return LocalRedirect($"{pathBase}/Login");
            }

            claims.Add(new Claim(ClaimTypes.Name, account));
            claims.Add(new Claim(ClaimTypes.Surname, staff.Name));
            claims.Add(new Claim(ClaimTypes.Role, "Staff"));
            if (staff.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
              CookieAuthenticationDefaults.AuthenticationScheme,
              new ClaimsPrincipal(claimsIdentity),
              new AuthenticationProperties
              {
                  IsPersistent = rememberMe
              });
            return LocalRedirect(staff.IsInitialized ? $"{pathBase}/" : $"{pathBase}/EditProfile");
        }
    }
}
