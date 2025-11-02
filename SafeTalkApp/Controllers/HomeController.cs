using MySql.Data.MySqlClient;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _safeTalkService;
        public HomeController(IHomeService safeTalkService)
        {
            _safeTalkService = safeTalkService;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home";
            return View();
        }

        public ActionResult Doctors()
        {
            ViewBag.Title = "Doctors";
            return View();
        }

        public ActionResult DoctorsView()
        {
            ViewBag.Title = "Doctors";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Title = "Contact Us";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "About Us";
            return View();
        }

        public ActionResult Terms()
        {
            ViewBag.Title = "Terms and Conditions";
            return View();
        }

        public JsonResult GetTerms()
        {
            var response = _safeTalkService.GetTerms();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDoctors()
        {
            var result = _safeTalkService.GetVerifiedDoctors();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SubmitFeedback(FeedbackDTO data)
        {
            var result = _safeTalkService.SubmitFeedback(data);
            return Json(result);
        }
    }
}