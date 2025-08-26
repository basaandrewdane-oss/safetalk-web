using Microsoft.AspNet.Identity;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class UserController : Controller
    {
        // GET: User

        public ActionResult Doctors()
        {
            return View();
        }

        public ActionResult Consultations()
        {
            return View();
        }

        public ActionResult Resources()
        {
            return View();
        }
    }
}