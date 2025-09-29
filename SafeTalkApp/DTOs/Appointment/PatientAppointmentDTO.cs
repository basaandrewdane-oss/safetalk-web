using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Appointment
{
    public class PatientAppointmentDTO
    {
        public int appointmentID { get; set; }
        public DateTime date { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public decimal fee { get; set; }
        public int status { get; set; }
        public string doctorName { get; set; }
        public string doctorEmail { get; set; }
        public string phoneNumber { get; set; }
    }
}