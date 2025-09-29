using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Appointment
{
    public class DoctorAvailabilityDTO
    {
        public int availabilityID { get; set; }
        public int userID { get; set; }
        public int dayID { get; set; }
        public string day { get; set; }
        public decimal fee { get; set; }
        public IEnumerable<TimeSlotDTO> slots { get; set; }

        public class TimeSlotDTO
        {
            public string start { get; set; }
            public string end { get; set; }
        }
    }
}