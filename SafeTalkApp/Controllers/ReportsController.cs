using Microsoft.AspNet.Identity;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    [Authorize(Roles = "Admin,Doctor,User")]
    public class ReportsController : Controller
    {
        private readonly IReportsService _reportsService;

        public ReportsController(IReportsService reportsService)
        {
            _reportsService = reportsService;
        }
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetConsultationReport()
        {
            var userID = User.Identity.GetUserId<int>();
            var response = _reportsService.GetConsultationReport(userID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ConsultationReport()
        {
            ViewBag.Title = "Consultation Report";
            return View();
        }

        public JsonResult GetPatientHistory(int? patientID)
        {
            var doctorID = User.Identity.GetUserId<int>();
            var response = _reportsService.GetPatientHistory(patientID, doctorID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDoctorHistory(int? doctorID)
        {
            var patientID = User.Identity.GetUserId<int>();
            var response = _reportsService.GetDoctorHistory(doctorID, patientID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PatientHistory()
        {
            ViewBag.Title = "My History";
            return View();
        }

        public ActionResult DoctorHistory()
        {
            ViewBag.Title = "My History";
            return View();
        }

        public ActionResult FollowUpReport()
        {
            return View();
        }

        public JsonResult GetMissedAppointments()
        {
            var userID = User.Identity.GetUserId<int>();
            var response = _reportsService.GetMissedAppointments(userID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
    }
}