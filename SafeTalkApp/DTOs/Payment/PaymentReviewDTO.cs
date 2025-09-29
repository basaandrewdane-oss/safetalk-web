using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Payment
{
    public class PaymentReviewDTO
    {
        public string Token { get; set; }
        public int AppointmentID { get; set; }
        public decimal Fee { get; set; }
        public string DoctorName { get; set; }
        public DateTime Date { get; set; }
        public string Time { get; set; }
    }
}