using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public static class ReferralStatus
    {
        public const int Pending = 1;
        public const int Sent = 2;
        public const int Acknowledged = 3;
        public const int Completed = 4;
    }
}