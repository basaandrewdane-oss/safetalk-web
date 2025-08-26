using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class SafeTalkController : Controller
    {
        // GET: SafeTalk
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
    }
}