using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult Index()
        {
            if (User.IsInRole("Doctor"))
            {
                return View("~/Views/Profile/Doctor/Index.cshtml");
            }
            else if (User.IsInRole("User") || User.IsInRole("Patient"))
            {
                return View("~/Views/Profile/User/Index.cshtml");
            }
            else if (User.IsInRole("Admin"))
            {
                return View("~/Views/Profile/Admin/Index.cshtml");
            }

            return RedirectToAction("Index", "SafeTalk");
        }

        // Admin Profile

        // Doctor Profile

        // User Profile
    }
}