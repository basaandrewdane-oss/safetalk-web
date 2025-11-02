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
        public const int Failed = 2; // payment failed
    }
}