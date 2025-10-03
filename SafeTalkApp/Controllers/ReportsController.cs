using Microsoft.AspNet.Identity;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
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
            return View();
        }

        //public JsonResult GetPatients()
        //{
        //    var doctorID = User.Identity.GetUserId<int>();
        //    var response = _reportsService.GetPatients(doctorID);
        //    return Json(response, JsonRequestBehavior.AllowGet);
        //}

        public JsonResult GetPatientHistory(int? patientID)
        {
            var doctorID = User.Identity.GetUserId<int>();
            var response = _reportsService.GetPatientHistory(patientID, doctorID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PatientHistory()
        {
            return View();
        }
    }
}