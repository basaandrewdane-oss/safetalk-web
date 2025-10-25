using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Account
{
    public class AvailabilityDTO
    {
        public int availabilityID { get; set; }
        public int dayID { get; set; }
        public string day { get; set; }
        public string availabilityStart { get; set; }
        public string availabilityEnd { get; set; }
        public int? slotDuration { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public decimal fee { get; set; }
    }
}