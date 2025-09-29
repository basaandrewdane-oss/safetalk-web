using Microsoft.AspNet.Identity;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class ConsultationController : Controller
    {
        private readonly ConsultationService _consultationService;

        public ConsultationController(ConsultationService consultationService)
        {
            _consultationService = consultationService;
        }
        public ActionResult Consultations()
        {
            if (User.IsInRole("Doctor"))
            {
                return View("~/Views/Consultation/Doctor/Index.cshtml");
            }
            else if (User.IsInRole("User") || User.IsInRole("Patient"))
            {
                return View("~/Views/Consultation/User/Index.cshtml");
            }

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult ChatRoom(int appointmentID)
        {
            using (var db = new SafeTalkAppContext())
            {
                var response = _consultationService.GetAppointment(appointmentID);

                if (response.data == null)
                {
                    return RedirectToAction("Unexisting", "Consultation");
                }

                var appointment = response.data;
                var currentUserId = User.Identity.GetUserId<int>();

                if (appointment.doctorID != currentUserId && appointment.patientID != currentUserId)
                {
                    return RedirectToAction("Unauthorized", "Consultation");
                }

                //var now = DateTime.Now;
                //var startDateTime = appointment.date + appointment.startTime;
                //var endDateTime = appointment.date + appointment.endTime;

                //if (now < startDateTime || now > endDateTime)
                //    return RedirectToAction("Inactive", "Consultation");

                var startDateTime = appointment.date + appointment.startTime; // DateTime + TimeSpan
                ViewBag.AppointmentStartTime = startDateTime.ToString("o");

                var endDateTime = appointment.date + appointment.endTime;
                ViewBag.AppointmentEndTime = endDateTime.ToString("o");

                return View(appointment);
            }
        }

        public ActionResult Unexisting()
        {
            return View();
        }

        public ActionResult Unauthorized()
        {
            return View();
        }

        public ActionResult Inactive()
        {
            return View();
        }

        public JsonResult GetChatMessages(int appointmentID)
        {
            try
            {
                var currentUserId = User.Identity.GetUserId<int>();
                var messages = _consultationService.GetChatMessages(appointmentID);
                return Json(new
                {
                    success = true,
                    currentUserId,
                    messages
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error retrieving chat messages: " + ex.Message);
                return Json(new { success = false, message = "An error occurred while retrieving chat messages." }, JsonRequestBehavior.AllowGet);
            }
        }

        // === User Consultation Actions ===
        public JsonResult GetPatientConsultations()
        {
            try
            {
                int userID = User.Identity.GetUserId<int>(); // Assuming you have a way to get the current user's ID
                var consultations = _consultationService.GetPatientConsultations(userID);
                return Json(consultations, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // === Doctor Consultation Actions ===
        public JsonResult GetDoctorConsultations()
        {
            try
            {
                int userID = User.Identity.GetUserId<int>(); // Assuming you have a way to get the current user's ID
                var consultations = _consultationService.GetDoctorConsultations(userID);
                return Json(consultations, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}