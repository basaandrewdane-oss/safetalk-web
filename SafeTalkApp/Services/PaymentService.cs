using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using SafeTalkApp.DTOs.Payment;
using SafeTalkApp.DTOs.Shared;
using SafeTalkApp.Interfaces;
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
        private readonly IEmailService _emailService;
        private readonly IFileStorageService _fileStorage;
        private readonly IDateTimeProvider _time;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(ISafeTalkAppContext db, IPayPalService payPalService, IEmailService emailService, IFileStorageService fileStorage,
        IDateTimeProvider time, ILogger<PaymentService> logger)
        {
            _db = db;
            _payPalService = payPalService;
            _emailService = emailService;
            _fileStorage = fileStorage;
            _time = time;
            _logger = logger;
        }

        public ApiResponse<bool> SubmitPayment(int appointmentID, HttpPostedFileBase paymentProof)
        {
            try
            {
                var appointment = _db.appointments_tbl.Find(appointmentID);
                if (appointment == null)
                    return ApiResponse<bool>.Fail("Appointment not found.");

                if (paymentProof == null || paymentProof.ContentLength == 0)
                    return ApiResponse<bool>.Fail("No payment proof uploaded.");

                // File validation
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var ext = Path.GetExtension(paymentProof.FileName).ToLower();
                if (!allowed.Contains(ext))
                    return ApiResponse<bool>.Fail("Invalid file type.");

                // Save file through abstraction
                var imagePath = _fileStorage.SavePaymentProof(paymentProof);

                var now = _time.Now;

                var payment = new PaymentTblModel
                {
                    appointmentID = appointmentID,
                    imagePath = imagePath,
                    status = PaymentStatus.Pending,
                    transactionId = Guid.NewGuid().ToString(),
                    amount = appointment.fee,
                    paymentDate = now,
                    dateCreated = now,
                    dateUpdated = now
                };

                _db.payment_tbl.Add(payment);

                appointment.status = AppointmentStatus.PaymentSubmitted;
                appointment.dateUpdated = now;

                _db.SaveChanges();

                TrySendEmail(appointment, payment);

                return ApiResponse<bool>.Ok(true, "Payment Submitted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting payment");
                return ApiResponse<bool>.Fail("Problem submitting payment: " + ex.Message);
            }
        }

        private void TrySendEmail(AppointmentsTblModel appointment, PaymentTblModel payment)
        {
            try
            {
                var admin = _db.user_tbl
                    .Join(_db.user_role_tbl, u => u.userID, r => r.userID, (u, r) => new { u, r })
                    .Where(x => x.r.roleID == 3)
                    .Select(x => x.u)
                    .FirstOrDefault();

                var patient = _db.user_tbl.Find(appointment.patientID);
                var doctor = _db.user_tbl.Find(appointment.doctorID);

                if (admin != null)
                    _emailService.SendPaymentSubmittedEmail(admin, appointment, payment, patient, doctor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email sending failed");
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
                _logger.LogError(ex, "error calling paypal");
                return ApiResponse<string>.Fail("There was a problem in creating order please try again later " + ex.Message);
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
                    return ApiResponse<PaymentReviewDTO>.Fail("Appointment not found.");
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

                var payment = new PaymentTblModel
                {
                    appointmentID = appointmentID,
                    status = PaymentStatus.Completed,
                    transactionId = transactionId,
                    amount = decimal.Parse(amount),
                    paymentDate = DateTime.Now,
                    dateCreated = DateTime.Now,
                    dateUpdated = DateTime.Now,
                };

                _db.payment_tbl.Add(payment);
                _db.SaveChanges();

                // Send emails after successful payment
                try
                {
                    var patient = _db.user_tbl.FirstOrDefault(u => u.userID == appointment.patientID);
                    var doctor = _db.user_tbl.FirstOrDefault(u => u.userID == appointment.doctorID);

                    if (patient != null && doctor != null)
                    {
                        _emailService.SendPayPalPaymentConfirmationToPatient(patient, doctor, appointment, payment);
                        _emailService.SendPayPalPaymentNotificationToDoctor(doctor, patient, appointment, payment);
                    }
                }
                catch (Exception emailEx)
                {
                    System.Diagnostics.Debug.WriteLine("Error sending payment confirmation emails: " + emailEx.Message);
                }

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

                    // Retrieve patient and doctor for email
                    var patient = _db.user_tbl.Find(appointment.patientID);
                    var doctor = _db.user_tbl.Find(appointment.doctorID);

                    // Send notification emails
                    if (patient != null && doctor != null)
                    {
                        try
                        {
                            _emailService.SendPaymentVerifiedEmailToPatient(patient, doctor, appointment, payment);
                            _emailService.SendPaymentVerifiedEmailToDoctor(doctor, patient, appointment, payment);
                        }
                        catch (Exception emailEx)
                        {
                            System.Diagnostics.Debug.WriteLine("Error sending payment verified emails: " + emailEx.Message);
                        }
                    }
                }

                return ApiResponse<bool>.Ok(true, "Payment Verified");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail(ex.Message);
            }

        }

        public ApiResponse<bool> RejectPayment(int appointmentID)
        {
            try
            {
                var payment = _db.payment_tbl.FirstOrDefault(p => p.appointmentID == appointmentID);
                if (payment == null)
                {
                    return ApiResponse<bool>.Fail("Payment not found.");
                }

                payment.status = PaymentStatus.Failed; // Mark as failed or rejected
                payment.dateUpdated = DateTime.Now;

                _db.SaveChanges();

                var appointment = _db.appointments_tbl.Find(appointmentID);
                if (appointment != null)
                {
                    appointment.status = AppointmentStatus.Rejected; // Update appointment status
                    appointment.dateUpdated = DateTime.Now;
                    _db.SaveChanges();

                    // Retrieve patient and doctor for email
                    var patient = _db.user_tbl.Find(appointment.patientID);
                    var doctor = _db.user_tbl.Find(appointment.doctorID);

                    // Send notification emails
                    if (patient != null && doctor != null)
                    {
                        try
                        {
                            _emailService.SendPaymentRejectedEmailToPatient(patient, doctor, appointment, payment);
                            _emailService.SendPaymentRejectedEmailToDoctor(doctor, patient, appointment, payment);
                        }
                        catch (Exception emailEx)
                        {
                            System.Diagnostics.Debug.WriteLine("Error sending payment verified emails: " + emailEx.Message);
                        }
                    }
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