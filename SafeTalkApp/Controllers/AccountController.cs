using Microsoft.Owin.Security;
using MySqlX.XDevAPI.Common;
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

        [AllowAnonymous]
        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public ActionResult Login()
        {
            if (User?.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            ViewBag.Title = "Login";
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
            ViewBag.Title = "Waiting for Approval";
            return View();
        }

        public ActionResult Signup()
        {
            ViewBag.Title = "Sign Up";
            return View("~/Views/Account/Signup/Index.cshtml");
        }

        public JsonResult RegisterUser(SignUpDTO signUp)
        {
            var result = _accountService.RegisterUser(signUp);
            return Json(result);
        }

        public JsonResult EmailExists(string email)
        {
            var result = _accountService.EmailExists(email);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AuthenticateUser(LoginDTO login)
        {
            var response = _accountService.AuthenticateUser(login);

            if (response.success)
            {
                var identity = _accountService.GenerateUserIdentity(response.data);

                var authManager = HttpContext.GetOwinContext().Authentication;
                authManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
            }

            return Json(response);
        }

        public ActionResult VerifyEmail(string token)
        {
            ViewBag.Title = "Verify Email";
            ViewBag.Token = token; // pass token to the view
            return View();         // returns a Razor page with AngularJS
        }

        public JsonResult VerifyEmailToken(string token)
        {
            var result = _accountService.VerifyEmail(token);
            return Json(result);
        }

        public JsonResult ResendVerificationEmail(string email)
        {
            var result = _accountService.ResendVerificationEmail(email);
            return Json(result);
        }

        public ActionResult Error()
        {
            ViewBag.Title = "Error";
            return View();
        }

        public ActionResult EmailVerified()
        {
            ViewBag.Title = "Email Verified";
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

        public JsonResult ForgotPassword(string email)
        {
            var result = _accountService.ForgotPassword(email);
            return Json(result);
        }

        public ActionResult ResetPassword()
        {
            ViewBag.Title = "Reset Password";
            return View();
        }

        public JsonResult ResetUserPassword(ResetPasswordDTO resetData)
        {
            var result = _accountService.ResetPassword(resetData);
            return Json(result);
        }
    }
}