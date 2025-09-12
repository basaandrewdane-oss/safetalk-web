using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Account
{
    public class AvailabilityDTO
    {
        public int dayID { get; set; }
        public TimeSpan availabilityStart { get; set; }
        public TimeSpan availabilityEnd { get; set; }
        public decimal fee { get; set; }
    }
}