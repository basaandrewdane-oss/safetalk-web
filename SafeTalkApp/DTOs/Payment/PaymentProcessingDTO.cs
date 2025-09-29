using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Payment
{
    public class PaymentProcessingDTO
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string Currency { get; set; }
        public int AppointmentID { get; set; }
    }
}