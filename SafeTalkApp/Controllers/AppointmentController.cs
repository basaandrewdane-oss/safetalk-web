using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeTalkApp.DTOs.Appointment;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    [Authorize(Roles = "User,Patient,Doctor")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        public ActionResult Appointments()
        {
            if (User.IsInRole("Doctor"))
            {
                ViewBag.Title = "My Appointments";
                return View("~/Views/Appointment/Doctor/Index.cshtml");
            }
            else if (User.IsInRole("User") || User.IsInRole("Patient"))
            {
                ViewBag.Title = "My Appointments";
                return View("~/Views/Appointment/User/Index.cshtml");
            }

            return RedirectToAction("Index", "Home");
        }

        public JsonResult GetAppointmentStatus(int appointmentId)
        {
            var response = _appointmentService.GetAppointmentStatus(appointmentId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }


        // Patient Appointment Actions
        [Authorize(Roles = "User")]
        public ActionResult Book()
        {
            ViewBag.Title = "Book Appointment";
            return View("~/Views/Appointment/User/Book.cshtml");
        }

        public JsonResult GetDoctors()
        {
            var result = _appointmentService.GetDoctors();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDoctorsAvailability(int userID)
        {
            var response = _appointmentService.GetDoctorsAvailability(userID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult BookAppointment(BookAppointmentDTO model)
        {
            var patientID = User.Identity.GetUserId<int>(); // still pulled from Identity
            var response = _appointmentService.BookAppointment(model, patientID);
            return Json(response);
        }

        public JsonResult CheckSlotAvailability(BookAppointmentDTO model)
        {
            var response = _appointmentService.CheckSlotAvailability(model);
            return Json(response);
        }

        public JsonResult GetPatientAppointments()
        {
            int userID = User.Identity.GetUserId<int>();
            var response = _appointmentService.GetPatientAppointments(userID);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CancelAppointment(int appointmentID)
        {
            var response = _appointmentService.CancelAppointment(appointmentID);
            return Json(response);
        }

        // Doctor Appointment Actions
        public JsonResult GetDoctorAppointments()
        {
            int doctorId = User.Identity.GetUserId<int>();
            var response = _appointmentService.GetDoctorAppointments(doctorId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ApproveAppointment(int appointmentID)
        {
            var response = _appointmentService.ApproveAppointment(appointmentID);
            return Json(response);
        }

        public JsonResult RejectAppointment(AppointmentResultDTO data)
        {
            var response = _appointmentService.RejectAppointment(data);
            return Json(response);
        }

    }
}