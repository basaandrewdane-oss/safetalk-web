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
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        public ActionResult FAQs()
        {
            ViewBag.Title = "Manage FAQs";
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
            return Json(response);
        }

        public JsonResult UpdateFaq(FAQsDTO faq)
        {
            var response = _adminService.UpdateFaq(faq);
            return Json(response);
        }

        public JsonResult DeleteFaq(int faqID)
        {
            var response = _adminService.DeleteFaq(faqID);
            return Json(response);
        }

        public ActionResult Prompts()
        {
            ViewBag.Title = "Manage Prompts";
            return View();
        }

        public JsonResult GetPrompts()
        {
            var response = _adminService.GetPrompts();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PendingDoctors()
        {
            ViewBag.Title = "Pending Doctor Verifications";
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
            return Json(result);
        }

        public ActionResult Payments()
        {
            ViewBag.Title = "Manage Payments";
            return View();
        }

        public JsonResult GetPayments()
        {
            var response = _adminService.GetPayments();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTerms()
        {
            var response = _adminService.GetTerms();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult UpdateTerms(TermsUpdateDTO dto)
        {
            var response = _adminService.UpdateTerms(dto.content);
            return Json(response);
        }

        public ActionResult ManageTerms()
        {
            ViewBag.Title = "Manage Terms and Conditions";
            return View();
        }

        public ActionResult ManageUsers()
        {
            ViewBag.Title = "Manage Users";
            return View();
        }

        public JsonResult GetUsers()
        {
            var response = _adminService.GetUsers();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult VerifyUser(int userID)
        {
            var response = _adminService.VerifyUser(userID);
            return Json(response);
        }

        public JsonResult DeleteUser(int userID)
        {
            var response = _adminService.DeleteUser(userID);
            return Json(response);
        }

        public JsonResult GetAppointmentsForAdmin()
        {
            var response = _adminService.GetAppointmentsForAdmin();
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Appointments()
        {
            ViewBag.Title = "Appointments";
            return View();
        }
    }
}