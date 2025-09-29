using Org.BouncyCastle.Asn1.Ocsp;
using SafeTalkApp.DTOs.Payment;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using static SafeTalkApp.Controllers.PaymentController;

namespace SafeTalkApp.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ISafeTalkAppContext _db;
        private readonly IPayPalService _payPalService;

        public PaymentService(ISafeTalkAppContext db, IPayPalService payPalService)
        {
            _db = db;
            _payPalService = payPalService;
        }

        public ApiResponse<bool> SubmitPayment(int appointmentID, HttpPostedFileBase paymentProof)
        {
            try
            {
                var appointment = _db.appointments_tbl.Find(appointmentID);
                if (appointment == null)
                {
                    return ApiResponse<bool>.Fail("Appointment not found.");
                }

                // Validate file
                if (paymentProof == null || paymentProof.ContentLength == 0)
                {
                    return ApiResponse<bool>.Fail("No payment proof uploaded.");
                }

                // Validate file type (optional but recommended)
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var fileExtension = Path.GetExtension(paymentProof.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return ApiResponse<bool>.Fail("Invalid file type. Only JPG, PNG, or PDF allowed.");
                }
                // Save payment image
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var uploadPath = HttpContext.Current.Server.MapPath("~/Uploads/Payments/");
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
                    amount = appointment.fee,
                    paymentDate = DateTime.Now,
                    dateCreated = DateTime.Now,
                    dateUpdated = DateTime.Now
                };
                _db.payment_tbl.Add(payment);

                appointment.status = AppointmentStatus.PaymentSubmitted; // Update appointment status
                appointment.dateUpdated = DateTime.Now;
                _db.SaveChanges();
                return ApiResponse<bool>.Ok(true, "Payment Submitted");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail("There was a problem in submitting payment please try again later " + ex.Message);
            }
        }

        public ApiResponse<string> CreatePayPalOrder(int appointmentID, string returnUrl, string cancelUrl)
        {
            try
            {
                var appointment = _db.appointments_tbl.Find(appointmentID);
                if (appointment == null)
                {
                    return ApiResponse<string>.Fail("Appointment not found.");
                }

                var result = _payPalService.CreateOrder(
                    appointment.fee,
                    returnUrl,
                    cancelUrl,
                    appointmentID
                );

                var approvalLink = result["links"].First(l => l["rel"].ToString() == "approve")["href"].ToString();
                return ApiResponse<string>.Ok(approvalLink);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail("There was a problem in creating order please try again later" + ex.Message);
            }
        }

        public ApiResponse<PaymentReviewDTO> ReviewPayPalOrder(string token, int appointmentID)
        {
            try
            {
                var appointment = _db.appointments_tbl.Find(appointmentID);
                var doctorName = _db.user_tbl.Where(u => u.userID == appointment.doctorID).Select(u => u.firstName + " " + u.lastName).FirstOrDefault();

                if (appointment == null)
                {
                    ApiResponse<PaymentReviewDTO>.Fail("Appointment not found.");
                }

                var dto = new PaymentReviewDTO
                {
                    Token = token,
                    AppointmentID = appointmentID,
                    Fee = appointment.fee,
                    DoctorName = doctorName,
                    Date = appointment.date,
                    Time = appointment.startTime + " - " + appointment.endTime
                };

                return ApiResponse<PaymentReviewDTO>.Ok(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentReviewDTO>.Fail("There was a problem reviewing payment order please try again later" + ex.Message);
            }
        }

        public ApiResponse<PaymentProcessingDTO> CapturePayPalOrder(string token, int appointmentID)
        {
            try
            {
                var result = _payPalService.CaptureOrder(token);

                var status = result["status"].ToString();
                if (status != "COMPLETED")
                {
                    return ApiResponse<PaymentProcessingDTO>.Fail("Payment not Completed");
                }

                var transactionId = result["purchase_units"][0]["payments"]["captures"][0]["id"].ToString();
                var referenceId = result["purchase_units"][0]["reference_id"].ToString();
                var amount = result["purchase_units"][0]["payments"]["captures"][0]["amount"]["value"].ToString();
                var currency = result["purchase_units"][0]["payments"]["captures"][0]["amount"]["currency_code"].ToString();

                if (referenceId != appointmentID.ToString())
                {
                    return ApiResponse<PaymentProcessingDTO>.Fail("Appointment ID mismatch");
                }

                if (_db.payment_tbl.Any(p => p.transactionId == transactionId))
                {
                    return ApiResponse<PaymentProcessingDTO>.Fail("Payment already processed.");
                }


                var appointment = _db.appointments_tbl.Find(appointmentID);
                if (appointment == null)
                {
                    return ApiResponse<PaymentProcessingDTO>.Fail("Appointment not found.");
                }
                appointment.status = AppointmentStatus.Paid;
                appointment.dateUpdated = DateTime.Now;

                _db.payment_tbl.Add(new PaymentTblModel
                {
                    appointmentID = appointmentID,
                    status = PaymentStatus.Completed,
                    transactionId = transactionId,
                    amount = decimal.Parse(amount),
                    paymentDate = DateTime.Now,
                    dateCreated = DateTime.Now,
                    dateUpdated = DateTime.Now,
                });

                _db.SaveChanges();

                var dto = new PaymentProcessingDTO
                {
                    Success = true,
                    TransactionId = transactionId,
                    Amount = amount,
                    Currency = currency,
                    AppointmentID = appointmentID
                };

                return ApiResponse<PaymentProcessingDTO>.Ok(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<PaymentProcessingDTO>.Fail("Error: " + ex.Message);
            }
        }

        public ApiResponse<bool> VerifyPayment(int appointmentID)
        {
            try
            {
                var payment = _db.payment_tbl.FirstOrDefault(p => p.appointmentID == appointmentID);
                if (payment == null)
                {
                    return ApiResponse<bool>.Fail("Payment not found.");
                }

                payment.status = PaymentStatus.Completed; // Assuming you want to mark it as verified
                payment.dateUpdated = DateTime.Now;

                _db.SaveChanges();

                var appointment = _db.appointments_tbl.Find(appointmentID);
                if (appointment != null)
                {
                    appointment.status = AppointmentStatus.Paid; // Update appointment status
                    appointment.dateUpdated = DateTime.Now;
                    _db.SaveChanges();
                }

                return ApiResponse<bool>.Ok(true, "Payment Verified");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail(ex.Message);
            }

        }
    }
}