using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Account
{
    public class AvailabilityDTO
    {
        public int dayID { get; set; }
        public string availabilityStart { get; set; }
        public string availabilityEnd { get; set; }
        public decimal fee { get; set; }
    }
}