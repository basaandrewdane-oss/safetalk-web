using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public static class PaymentStatus
    {
        public const int Pending = 0; // payment not made yet
        public const int Completed = 1; // payment completed successfully
        //public const int Failed = 2; // payment failed
        //public const int Refunded = 3; // payment refunded
        //public const int VerificationPending = 4; // proof of payment submitted, pending verification
        //public const int VerificationFailed = 5; // proof of payment verification failed
        //public const int VerificationApproved = 6; // proof of payment verified and approved
    }
}