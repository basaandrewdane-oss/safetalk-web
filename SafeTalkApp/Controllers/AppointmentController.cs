using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeTalkApp.Models;
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
    public class AppointmentController : Controller
    {
        public ActionResult Appointments()
        {
            if (User.IsInRole("Doctor"))
            {
                return View("~/Views/Appointment/Doctor/Index.cshtml");
            }
            else if (User.IsInRole("User") || User.IsInRole("Patient"))
            {
                return View("~/Views/Appointment/User/Index.cshtml");
            }

            return RedirectToAction("Index", "Home");
        }

        public JsonResult GetAppointmentStatus(int appointmentId)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var appointment = db.appointments_tbl.FirstOrDefault(a => a.appointmentID == appointmentId);
                    if (appointment == null)
                        return Json(new { status = "NotFound" }, JsonRequestBehavior.AllowGet);

                    return Json(new
                    {
                        status = appointment.status,
                        endTime = appointment.date.Add(appointment.endTime) // or however your time is stored
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }


        // Patient Appointment Actions
        public ActionResult Book()
        {
            return View("~/Views/Appointment/User/Book.cshtml");
        }

        public JsonResult GetDoctors()
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var doctors = (from u in db.user_tbl
                                   join ur in db.user_role_tbl on u.userID equals ur.userID
                                   join r in db.role_tbl on ur.roleID equals r.roleID
                                   where r.roleName == "Doctor" /*&& u.isVerified == true*/
                                   select new
                                   {
                                       u.userID,
                                       u.firstName,
                                       u.lastName,
                                       u.email,
                                       u.specialization,
                                   }).ToList();
                    return Json(doctors, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetDoctorsAvailability(int userID)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var availability = (from a in db.user_availability_tbl
                                        join d in db.days_of_week_tbl on a.dayID equals d.dayID
                                        where a.userID == userID
                                        select new
                                        {
                                            a.availabilityID,
                                            a.userID,
                                            d.dayID,
                                            d.day,
                                            a.availabilityStart,
                                            a.availabilityEnd,
                                            a.fee
                                        }).ToList();

                    var result = availability.Select(a => new
                    {
                        a.availabilityID,
                        a.userID,
                        a.dayID,
                        a.day,
                        a.fee,
                        slots = GenerateTimeSlots(a.availabilityStart, a.availabilityEnd)
                    }).ToList();

                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<TimeSlot> GenerateTimeSlots(TimeSpan start, TimeSpan end, int intervalMinutes = 30)
        {
            var slots = new List<TimeSlot>();

            while (start + TimeSpan.FromMinutes(intervalMinutes) <= end)
            {
                var next = start + TimeSpan.FromMinutes(intervalMinutes);
                slots.Add(new TimeSlot
                {
                    Start = start.ToString(@"hh\:mm"),
                    End = next.ToString(@"hh\:mm")
                });
                start = next;
            }

            return slots;
        }

        public JsonResult BookAppointment(AppointmentsTblModel model)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var existingAppointment = (from a in db.appointments_tbl
                                               where a.doctorID == model.doctorID
                                               && a.date == model.date
                                               && ((model.startTime >= a.startTime && model.startTime < a.endTime)
                                                   || (model.endTime > a.startTime && model.endTime <= a.endTime))
                                               select a).FirstOrDefault();

                    if (existingAppointment != null)
                    {
                        return Json(new { success = false, message = "This time slot is already booked." }, JsonRequestBehavior.AllowGet);
                    }

                    model.patientID = User.Identity.GetUserId<int>(); // Assuming you have a way to get the current user's ID
                    var appointment = new AppointmentsTblModel
                    {
                        doctorID = model.doctorID,
                        patientID = model.patientID,
                        date = model.date,
                        startTime = model.startTime,
                        endTime = model.endTime,
                        fee = model.fee,
                        status = AppointmentStatus.Pending,
                        dateCreated = DateTime.Now, // Use appropriate date handling
                        dateUpdated = DateTime.Now // Use appropriate date handling
                    };
                    db.appointments_tbl.Add(appointment);
                    db.SaveChanges();

                    //var doctor = db.user_tbl.Find(model.doctorID);
                    //var patient = db.user_tbl.Find(model.patientID);

                    //if (doctor == null || patient == null)
                    //{
                    //    return Json(new { success = false, message = "Doctor or patient not found." }, JsonRequestBehavior.AllowGet);
                    //}

                    // Optionally, send emails to doctor and patient
                    // Uncomment the following lines if you want to send emails
                    //SendAppointmentEmails(doctor.email, doctor.firstName + " " + doctor.lastName, patient.firstName + " " + patient.lastName, appointment.date, appointment.startTime);
                    //SendPatientConfirmation(patient.email, patient.firstName + " " + patient.lastName, doctor.firstName + " " + doctor.lastName, appointment.date, appointment.startTime);

                    //SendAppointmentEmails(doctor.email, doctor.name, patient.name, appointment.date, appointment.startTime);
                    //SendPatientConfirmation(patient.email, patient.name, doctor.name, appointment.date, appointment.startTime);

                    return Json(new { success = true, message = "Appointment booked successfully." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetPatientAppointments()
        {
            try
            {
                int userID = User.Identity.GetUserId<int>(); // Assuming you have a way to get the current user's ID
                using (var db = new SafeTalkAppContext())
                {
                    var appointments = (from a in db.appointments_tbl
                                        join d in db.user_tbl on a.doctorID equals d.userID
                                        where a.patientID == userID
                                        orderby a.date descending, a.startTime descending
                                        select new
                                        {
                                            a.appointmentID,
                                            a.date,
                                            a.startTime,
                                            a.endTime,
                                            a.fee,
                                            a.status,
                                            doctorName = d.firstName + " " + d.lastName,
                                            doctorEmail = d.email,
                                            d.phoneNumber
                                        }).ToList();
                    return Json(appointments, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CancelAppointment(int appointmentID)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var appointment = db.appointments_tbl.Find(appointmentID);
                    if (appointment == null)
                    {
                        return Json(new { success = false, message = "Appointment not found." }, JsonRequestBehavior.AllowGet);
                    }
                    appointment.status = AppointmentStatus.Canceled; // Assuming you want to mark it as rejected
                    appointment.dateUpdated = DateTime.Now;
                    db.SaveChanges();

                    // Optionally, send cancellation email to doctor and patient
                    //var doctor = db.user_tbl.Find(appointment.doctorID);
                    //var patient = db.user_tbl.Find(appointment.patientID);
                    //if (doctor == null || patient == null) 
                    //{
                    //    return Json(new { success = false, message = "Doctor or patient not found." }, JsonRequestBehavior.AllowGet);
                    //}
                    //SendEmail(doctor.email, "Appointment Cancelled", $"Your appointment with {patient.firstName} {patient.lastName} on {appointment.date.ToShortDateString()} at {appointment.startTime} has been cancelled.");
                    //SendEmail(patient.email, "Appointment Cancelled", $"Your appointment with Dr. {doctor.firstName} {doctor.lastName} on {appointment.date.ToShortDateString()} at {appointment.startTime} has been cancelled.");

                    return Json(new { success = true, message = "Appointment cancelled successfully." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // Doctor Appointment Actions
        public JsonResult GetDoctorAppointments()
        {
            try
            {
                int userID = User.Identity.GetUserId<int>(); // Assuming you have a way to get the current user's ID
                using (var db = new SafeTalkAppContext())
                {
                    var appointments = (from a in db.appointments_tbl
                                        join d in db.user_tbl on a.patientID equals d.userID
                                        join p in db.payment_tbl on a.appointmentID equals p.appointmentID into pay
                                        from p in pay.DefaultIfEmpty()
                                        where a.doctorID == userID
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
                                            paymentImage = p.imagePath,
                                            transcriptPath = a .transcriptFilePath, // Assuming you store the transcript file path in the appointment model
                                        }).ToList();
                    return Json(appointments, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult ApproveAppointment(int appointmentID)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var appointment = db.appointments_tbl.Find(appointmentID);
                    if (appointment == null)
                    {
                        return Json(new { success = false, message = "Appointment not found." }, JsonRequestBehavior.AllowGet);
                    }
                    appointment.status = AppointmentStatus.Approved;
                    appointment.dateUpdated = DateTime.Now;
                    db.SaveChanges();
                    // Send confirmation email to patient
                    //var patient = db.user_tbl.Find(appointment.patientID);
                    //if (patient != null)
                    //{
                    //    string formattedDate = appointment.date.ToString("yyyy-MM-dd");
                    //    string formattedTime = appointment.startTime.ToString(@"hh\:mm");
                    //    EmailHelper.SendEmail(
                    //        patient.email,
                    //        "Appointment Confirmation",
                    //        $"Hi {patient.firstName},<br>Your appointment with Dr. {User.Identity.GetUserName()} is confirmed on {formattedDate} at {formattedTime}."
                    //    );
                    //}
                    return Json(new { success = true, message = "Appointment approved and confirmation email sent." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        // Helpers
        public void SendAppointmentEmails(string doctorEmail, string doctorName, string patientName, DateTime date, TimeSpan time)
        {
            string formattedDate = date.ToString("yyyy-MM-dd");
            string formattedTime = time.ToString(@"hh\:mm");
            string body = $"Hi Dr. {doctorName},<br>You have a new appointment request with {patientName} on {formattedDate} at {formattedTime}.";
            // Email to doctor
            SendEmail(doctorEmail, "New Appointment Booked", body);
        }

        public void SendPatientConfirmation(string patientEmail, string patientName, string doctorName, DateTime date, TimeSpan time)
        {
            string formattedDate = date.ToString("yyyy-MM-dd");
            string formattedTime = time.ToString(@"hh\:mm");
            string body = $"Hi {patientName},<br>Your appointment with Dr. {doctorName} on {formattedDate} at {formattedTime}. is awaiting confirmation";
            // Email to patient
            SendEmail(patientEmail, "Pending Confirmation", body);
        }

        public void SendEmail(string toEmail, string subject, string body)
        {
            string smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
            int smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            string smtpUser = ConfigurationManager.AppSettings["SmtpUser"];
            string smtpPass = ConfigurationManager.AppSettings["SmtpPass"];

            var smtpClient = new SmtpClient(smtpHost)
            {
                Port = smtpPort,
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(toEmail);

            smtpClient.Send(mailMessage);
        }

    }
}