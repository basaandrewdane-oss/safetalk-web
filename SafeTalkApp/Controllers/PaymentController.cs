using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SafeTalkApp.Models;
using SafeTalkApp.Services;
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
    [Authorize(Roles = "Admin,User")]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public JsonResult SubmitPayment(int appointmentID, HttpPostedFileBase paymentProof)
        {
            try
            {
                var response = _paymentService.SubmitPayment(appointmentID, paymentProof);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public JsonResult CreatePayPalOrder(int appointmentID)
        {
            try
            {
                var response = _paymentService.CreatePayPalOrder(
                    appointmentID,
                    Url.Action("ReviewPayPalOrder", "Payment", new { appointmentID }, Request.Url.Scheme),
                    Url.Action("Appointments", "Appointment", null, Request.Url.Scheme)
                    );

                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult ReviewPayPalOrder(string token, int appointmentID)
        {
            try
            {
                var response = _paymentService.ReviewPayPalOrder(token, appointmentID);
                if (!response.success)
                {
                    return RedirectToAction("Appointments", "Appointment");
                }
                ViewBag.Title = "Review PayPal Payment";
                return View("PaymentReview", response.data);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public ActionResult CapturePayPalOrder(string token, int appointmentID)
        {
            try
            {
                var response = _paymentService.CapturePayPalOrder(token, appointmentID);
                ViewBag.Title = "Processing PayPal Payment";
                return View("PaymentProcessing", response.data);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public JsonResult VerifyPayment(int appointmentID)
        {
            try
            {
                var response = _paymentService.VerifyPayment(appointmentID);
                return Json(response);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}