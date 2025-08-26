using Microsoft.AspNet.Identity;
using SafeTalkApp.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SafeTalkApp.Controllers
{
    public class ConsultationController : Controller
    {
        // GET: Consultation
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
                var appointment = db.appointments_tbl.FirstOrDefault(a => a.appointmentID == appointmentID);

                if (appointment == null)
                    return RedirectToAction("Unexisting", "Consultation");

                var currentUserId = User.Identity.GetUserId<int>();
                if (appointment.doctorID != currentUserId && appointment.patientID != currentUserId)
                    return RedirectToAction("Unauthorized", "Consultation");

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

                using (var db = new SafeTalkAppContext())
                {
                    var messages = (from m in db.chat_message_tbl
                                    join u in db.user_tbl on m.senderID equals u.userID
                                    where m.appointmentID == appointmentID
                                    orderby m.sentAt ascending
                                    select new
                                    {
                                        m.messageID,
                                        m.appointmentID,
                                        m.senderID,
                                        senderName = u.firstName + " " + u.lastName,
                                        m.message,
                                        m.sentAt
                                    }).ToList();
                    return Json(new
                    {
                        success = true,
                        currentUserId,
                        messages
                    }, JsonRequestBehavior.AllowGet);
                }
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
                using (var db = new SafeTalkAppContext())
                {
                    int userID = User.Identity.GetUserId<int>(); // Assuming you have a way to get the current user's ID
                    var appointments = (from a in db.appointments_tbl
                                        join d in db.user_tbl on a.doctorID equals d.userID
                                        join p in db.payment_tbl on a.appointmentID equals p.appointmentID
                                        where a.patientID == userID && p.status == PaymentStatus.Completed
                                        orderby a.date descending, a.startTime descending
                                        select new
                                        {
                                            a.appointmentID,
                                            a.date,
                                            a.startTime,
                                            a.endTime,
                                            a.status,
                                            doctorName = d.firstName + " " + d.lastName,
                                            doctorEmail = d.email,
                                            paymentImage = p.imagePath
                                        }).ToList();
                    return Json(appointments, JsonRequestBehavior.AllowGet);
                }
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
                using (var db = new SafeTalkAppContext())
                {
                    int userID = User.Identity.GetUserId<int>(); // Assuming you have a way to get the current user's ID
                    var appointments = (from a in db.appointments_tbl
                                        join d in db.user_tbl on a.patientID equals d.userID
                                        join p in db.payment_tbl on a.appointmentID equals p.appointmentID
                                        where a.doctorID == userID && p.status == PaymentStatus.Completed
                                        orderby a.date descending, a.startTime descending
                                        select new
                                        {
                                            a.appointmentID,
                                            a.date,
                                            a.startTime,
                                            a.endTime,
                                            a.status,
                                            patientName = d.firstName + " " + d.lastName,
                                            patientEmail = d.email,
                                            paymentImage = p.imagePath
                                        }).ToList();
                    return Json(appointments, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}