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
            return View();
        }

        public ActionResult Doctors()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public JsonResult GetDoctors()
        {
            var result = _safeTalkService.GetVerifiedDoctors();
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}