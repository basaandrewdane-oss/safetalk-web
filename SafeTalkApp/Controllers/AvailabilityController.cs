using Microsoft.AspNet.Identity;
using SafeTalkApp.DTOs.Account;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class AvailabilityController : Controller
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }
        // GET: Availability
        public ActionResult Index()
        {
            ViewBag.Title = "My Availability";
            return View();
        }

        public JsonResult GetAvailability()
        {
            var userID = User.Identity.GetUserId<int>();
            var response = _availabilityService.GetAvailability(userID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveAvailability(List<AvailabilityDTO> availabilities)
        {
            var userID = User.Identity.GetUserId<int>();
            var response = _availabilityService.SaveAvailability(userID, availabilities);
            return Json(response);
        }
    }
}