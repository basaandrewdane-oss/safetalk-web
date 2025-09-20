using SafeTalkApp.DTOs.Admin;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }
        public ActionResult FAQs()
        {
            return View();
        }

        public JsonResult GetFaqs()
        {
            var response = _adminService.GetFaqs();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddFaq(FAQsDTO faq)
        {
            var response = _adminService.AddFaq(faq);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateFaq(FAQsDTO faq)
        {
            var response = _adminService.UpdateFaq(faq);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteFaq(int faqID)
        {
            var response = _adminService.DeleteFaq(faqID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Prompts()
        {
            return View();
        }

        public JsonResult GetPrompts()
        {
            var response = _adminService.GetPrompts();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PendingDoctors()
        {
            return View();
        }

        public JsonResult GetPendingDoctors()
        {
            var response = _adminService.GetPendingDoctors();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VerifyDoctor(int userID)
        {
            var result = _adminService.VerifyDoctor(userID);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}