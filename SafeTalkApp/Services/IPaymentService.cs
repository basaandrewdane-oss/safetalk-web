using SafeTalkApp.DTOs.Payment;
using SafeTalkApp.DTOs.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Services
{
    public interface IPaymentService
    {
        ApiResponse<bool> SubmitPayment(int appointmentID, HttpPostedFileBase paymentProof);
        ApiResponse<string> CreatePayPalOrder(int appointmentID, string returnUrl, string cancelUrl);
        ApiResponse<PaymentReviewDTO> ReviewPayPalOrder(string token, int appointmentID);
        ApiResponse<PaymentProcessingDTO> CapturePayPalOrder(string token, int appointmentID);
        ApiResponse<bool> VerifyPayment(int appointmentID);
        ApiResponse<bool> RejectPayment(int appointmentID);
    }
}