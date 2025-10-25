using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Appointment
{
    public class DoctorAppointmentDTO
    {
        public int appointmentID { get; set; }
        public DateTime date { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public int status { get; set; }
        public string rejectReason { get; set; }
        public string patientName { get; set; }
        public string patientEmail { get; set; }
        public string paymentImage { get; set; }
        public string transcriptPath { get; set; }
    }
}