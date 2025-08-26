using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public static class AppointmentStatus
    {
        public const int Pending = 0; // waiting for doctor approval
        public const int Approved = 1; // doctor approved, waiting for payment
        public const int PaymentSubmitted = 2; // proof uploaded, pending verification
        public const int Paid = 3; // proof verified
        public const int Rejected = 4; // doctor rejected
        public const int Canceled = 5; // patient canceled
        public const int Completed = 6; // consultation completed
    }
}