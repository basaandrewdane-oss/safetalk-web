using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using static SafeTalkApp.Controllers.AppointmentController;

namespace SafeTalkApp.Controllers
{
    public class PaymentController : Controller
    {
        // GET: Payment
        public JsonResult SubmitPayment(int appointmentID, HttpPostedFileBase paymentProof)
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

                    // Validate file
                    if (paymentProof == null || paymentProof.ContentLength == 0)
                    {
                        return Json(new { success = false, message = "No payment proof uploaded." }, JsonRequestBehavior.AllowGet);
                    }

                    // Validate file type (optional but recommended)
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                    var fileExtension = Path.GetExtension(paymentProof.FileName).ToLower();
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return Json(new { success = false, message = "Invalid file type. Only JPG, PNG, or PDF allowed." }, JsonRequestBehavior.AllowGet);
                    }
                    // Save payment image
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadPath = Server.MapPath("~/Uploads/Payments/");
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var fullPath = Path.Combine(uploadPath, fileName);
                    paymentProof.SaveAs(fullPath);
                    // Create payment record
                    var payment = new PaymentTblModel
                    {
                        appointmentID = appointmentID,
                        imagePath = "/Uploads/Payments/" + fileName, // For easier access on frontend
                        status = PaymentStatus.Pending,
                        transactionId = Guid.NewGuid().ToString(), // Temporary transaction ID
                        dateCreated = DateTime.Now,
                        dateUpdated = DateTime.Now
                    };
                    db.payment_tbl.Add(payment);

                    appointment.status = AppointmentStatus.PaymentSubmitted; // Update appointment status
                    appointment.dateUpdated = DateTime.Now;
                    db.SaveChanges();
                    return Json(new { success = true, message = "Payment submitted successfully." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult CreatePayPalOrder(int appointmentID)
        {
            using (var db = new SafeTalkAppContext())
            {
                var appointment = db.appointments_tbl.Find(appointmentID);
                if (appointment == null)
                    return Json(new { success = false, message = "Appointment not found." });

                var paypal = new PayPalHelper();
                var result = paypal.CreateOrder(
                    appointment.fee,
                    Url.Action("ReviewPayPalOrder", "Payment", new { appointmentID }, Request.Url.Scheme),
                    Url.Action("Appointments", "Appointment", null, Request.Url.Scheme),
                    appointmentID
                );

                var approvalLink = result["links"].First(l => l["rel"].ToString() == "approve")["href"].ToString();
                return Json(new { success = true, approvalUrl = approvalLink });
            }
        }

        public ActionResult ReviewPayPalOrder(string token, int appointmentID)
        {
            using (var db = new SafeTalkAppContext())
            {
                var appointment = db.appointments_tbl.Find(appointmentID);
                var doctorName = db.user_tbl.Where(u => u.userID == appointment.doctorID).Select(u => u.firstName + " " + u.lastName).FirstOrDefault();
                if (appointment == null)
                    return RedirectToAction("Appointments", "Appointment");

                ViewBag.Token = token;
                ViewBag.AppointmentID = appointmentID;
                ViewBag.Fee = appointment.fee;
                ViewBag.DoctorName = doctorName; // adjust to your model
                ViewBag.Date = appointment.date;
                ViewBag.Time = appointment.startTime;
            }

            return View("PaymentReview");
        }

        public ActionResult CapturePayPalOrder(string token, int appointmentID)
        {
            try
            {
                var paypal = new PayPalHelper();
                var result = paypal.CaptureOrder(token);

                var status = result["status"].ToString();
                if (status != "COMPLETED")
                {
                    ViewBag.Success = false;
                    ViewBag.ErrorMessage = "Payment not completed.";
                    return View("PaymentProcessing");
                }

                var transactionId = result["purchase_units"][0]["payments"]["captures"][0]["id"].ToString();
                var referenceId = result["purchase_units"][0]["reference_id"].ToString();
                var amount = result["purchase_units"][0]["payments"]["captures"][0]["amount"]["value"].ToString();
                var currency = result["purchase_units"][0]["payments"]["captures"][0]["amount"]["currency_code"].ToString();

                if (referenceId != appointmentID.ToString())
                {
                    ViewBag.Success = false;
                    ViewBag.ErrorMessage = "Appointment ID mismatch.";
                    return View("PaymentProcessing");
                }

                using (var db = new SafeTalkAppContext())
                {
                    if (db.payment_tbl.Any(p => p.transactionId == transactionId))
                    {
                        ViewBag.Success = false;
                        ViewBag.ErrorMessage = "Payment already processed.";
                        return View("PaymentProcessing");
                    }


                    var appointment = db.appointments_tbl.Find(appointmentID);
                    if (appointment != null)
                    {
                        appointment.status = AppointmentStatus.Paid;
                        appointment.dateUpdated = DateTime.Now;

                        db.payment_tbl.Add(new PaymentTblModel
                        {
                            appointmentID = appointmentID,
                            status = PaymentStatus.Completed,
                            dateCreated = DateTime.Now,
                            dateUpdated = DateTime.Now,
                            transactionId = transactionId
                        });

                        db.SaveChanges();

                        // Pass details to the Razor view
                        ViewBag.Success = true;
                        ViewBag.TransactionId = transactionId;
                        ViewBag.Amount = amount;
                        ViewBag.Currency = currency;
                        ViewBag.AppointmentID = appointmentID;
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Success = false;
                ViewBag.ErrorMessage = ex.Message;
            }

            return View("PaymentProcessing");
        }

        public JsonResult VerifyPayment(int appointmentID)
        {
            try
            {
                using (var db = new SafeTalkAppContext())
                {
                    var payment = db.payment_tbl.FirstOrDefault(p => p.appointmentID == appointmentID);
                    if (payment == null)
                    {
                        return Json(new { success = false, message = "Payment not found." }, JsonRequestBehavior.AllowGet);
                    }
                    payment.status = PaymentStatus.Completed; // Assuming you want to mark it as verified
                    payment.dateUpdated = DateTime.Now;
                    db.SaveChanges();
                    var appointment = db.appointments_tbl.Find(appointmentID);
                    if (appointment != null)
                    {
                        appointment.status = AppointmentStatus.Paid; // Update appointment status
                        appointment.dateUpdated = DateTime.Now;
                        db.SaveChanges();
                    }
                    return Json(new { success = true, message = "Payment verified successfully." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public class PayPalHelper
        {
            private readonly string clientId = ConfigurationManager.AppSettings["PayPalClientID"];
            private readonly string secret = ConfigurationManager.AppSettings["PayPalSecret"];
            private readonly string mode = ConfigurationManager.AppSettings["PayPalMode"]; // sandbox or live

            private string GetBaseUrl() => mode == "sandbox" ? "https://api-m.sandbox.paypal.com" : "https://api-m.paypal.com";

            private string GetAccessToken()
            {
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    client.Headers[HttpRequestHeader.Authorization] = "Basic " +
                        Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{secret}"));

                    var bytes = client.UploadData(GetBaseUrl() + "/v1/oauth2/token", "POST",
                        Encoding.UTF8.GetBytes("grant_type=client_credentials"));

                    var result = JObject.Parse(Encoding.UTF8.GetString(bytes));
                    return result["access_token"].ToString();
                }
            }

            public JObject CreateOrder(decimal amount, string returnUrl, string cancelUrl, int appointmentID)
            {
                var token = GetAccessToken();
                var order = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new {
                            amount = new { currency_code = "PHP", value = amount.ToString("F2") },
                            reference_id = appointmentID.ToString() // Use appointment ID as reference
                        }
                    },
                    application_context = new
                    {
                        return_url = returnUrl,
                        cancel_url = cancelUrl,
                        user_action = "CONTINUE"
                    }
                };

                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;

                    var response = client.UploadString(GetBaseUrl() + "/v2/checkout/orders", "POST",
                        JsonConvert.SerializeObject(order));

                    return JObject.Parse(response);
                }
            }

            public JObject CaptureOrder(string orderId)
            {
                var token = GetAccessToken();
                using (var client = new WebClient())
                {
                    client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    client.Headers[HttpRequestHeader.Authorization] = "Bearer " + token;

                    var response = client.UploadString(GetBaseUrl() + $"/v2/checkout/orders/{orderId}/capture", "POST", "");
                    return JObject.Parse(response);
                }
            }
        }
    }
}