using Microsoft.AspNet.Identity;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetDashboardStats()
        {
            var userID = User.Identity.GetUserId<int>();
            var role = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Doctor") ? "Doctor" : "User";

            var response = _dashboardService.GetDashboardStats(userID, role);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}