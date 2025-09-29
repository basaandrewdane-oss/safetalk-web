using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Appointment
{
    public class BookAppointmentDTO
    {
        public int doctorID { get; set; }
        public DateTime date { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public decimal fee { get; set; }
        public string chiefComplaint { get; set; }
    }
}