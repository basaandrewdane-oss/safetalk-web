using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using SafeTalkApp.DTOs.Profile;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    [Authorize(Roles = "Admin,User,Doctor")]
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly IAccountService _accountService;

        public ProfileController(IProfileService profileService, IAccountService accountService)
        {
            _profileService = profileService;
            _accountService = accountService;
        }
        public ActionResult Index()
        {
            if (User.IsInRole("Doctor"))
            {
                ViewBag.Title = "My Profile";
                return View("~/Views/Profile/Doctor/Index.cshtml");
            }
            else if (User.IsInRole("User") || User.IsInRole("Patient"))
            {
                ViewBag.Title = "My Profile";
                return View("~/Views/Profile/User/Index.cshtml");
            }
            else if (User.IsInRole("Admin"))
            {
                ViewBag.Title = "My Profile";
                return View("~/Views/Profile/Admin/Index.cshtml");
            }

            return RedirectToAction("Index", "SafeTalk");
        }

        public ActionResult ProfileView()
        {
            ViewBag.Title = "My Profile";
            return View();
        }

        public JsonResult GetProfile()
        {
            var userId = User.Identity.GetUserId();
            var result = _profileService.GetProfile(userId);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateProfile(ProfileUpdateDTO dto)
        {
            var userId = User.Identity.GetUserId();
            var response = _profileService.UpdateProfile(dto, userId);
            if (response.success)
            {
                // Re-fetch updated user info to rebuild claims
                var updatedUser = _profileService.GetUserById(userId);
                if (updatedUser != null)
                {
                    var identity = _accountService.GenerateUserIdentity(updatedUser);

                    var authManager = HttpContext.GetOwinContext().Authentication;
                    authManager.SignOut(); // Remove old claims
                    authManager.SignIn(new AuthenticationProperties { IsPersistent = false }, identity);
                }
            }
            return Json(response);
        }
    }
}