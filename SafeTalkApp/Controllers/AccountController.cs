using Microsoft.Owin.Security;
using SafeTalkApp.DTOs.Account;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Logout()
        {
            var authManager = HttpContext.GetOwinContext().Authentication;
            authManager.SignOut("ApplicationCookie");
            return RedirectToAction("Login", "Account");
        }

        public ActionResult Waiting()
        {
            return View();
        }

        public ActionResult Signup(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return RedirectToAction("SelectRole"); // optional fallback
            }

            role = role.Trim().ToLower();

            switch (role)
            {
                case "user":
                    return View("~/Views/Account/Signup/User/index.cshtml");
                case "doctor":
                    return View("~/Views/Account/Signup/Doctor/index.cshtml");
                default:
                    return View("Error"); // Or return a not found message
            }
        }

        public JsonResult RegisterUser(SignUpDTO signUp)
        {
            var result = _accountService.RegisterUser(signUp);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AuthenticateUser(LoginDTO login)
        {
            var result = _accountService.AuthenticateUser(login);

            if (result.success)
            {
                var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, result.data.userID.ToString()),
                            new Claim(ClaimTypes.Name, result.data.email),
                            new Claim(ClaimTypes.GivenName, result.data.firstName + " " + result.data.lastName),
                            new Claim(ClaimTypes.Role, result.data.role ?? "User")
                        };

                var identity = new ClaimsIdentity(claims, "ApplicationCookie");

                var authManager = HttpContext.GetOwinContext().Authentication;
                authManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult VerifyEmail(string token)
        {
            var verified = _accountService.VerifyEmail(token);
            return verified.success ? View("EmailVerified") : View("Error");
        }

        public ActionResult Error()
        {
            return View();
        }

        public ActionResult EmailVerified()
        {
            return View();
        }

        public JsonResult GetRoles()
        {
            var result = _accountService.GetRoles();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetGenders()
        {
            var result = _accountService.GetGenders();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDaysOfWeek()
        {
            var result = _accountService.GetDaysOfWeek();
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        //public ActionResult CreateAdmin()
        //{
        //    using (var db = new SafeTalkAppContext())
        //    {
        //        var email = "admin@safetalk.com";

        //        if (db.user_tbl.Any(u => u.email == email))
        //            return Content("Admin already exists.");

        //        var adminUser = new UserTblModel
        //        {
        //            firstName = "System",
        //            lastName = "Admin",
        //            birthDate = new DateTime(1990, 1, 1),
        //            genderID = 1, // or any valid gender ID
        //            phoneNumber = "09123456789",
        //            email = email,
        //            password = BCrypt.Net.BCrypt.HashPassword("AdminPassword123!"),
        //            dateCreated = DateTime.Now,
        //            dateUpdated = DateTime.Now,
        //            isVerified = true
        //        };
        //        db.user_tbl.Add(adminUser);
        //        db.SaveChanges();

        //        var adminRole = new UserRoleTblModel
        //        {
        //            userID = adminUser.userID,
        //            roleID = 3, // Admin role ID
        //            dateCreated = DateTime.Now,
        //            dateUpdated = DateTime.Now
        //        };
        //        db.user_role_tbl.Add(adminRole);
        //        db.SaveChanges();

        //        return Content("Admin account created.");
        //    }
        //}
    }
}