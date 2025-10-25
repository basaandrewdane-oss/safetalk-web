using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Appointment
{
    public class AppointmentResultDTO
    {
        public int appointmentID { get; set; }
        public int doctorID { get; set; }
        public int patientID { get; set; }
        public string doctorName { get; set; }
        public string patientName { get; set; }
        public string doctorEmail { get; set; }
        public string patientEmail { get; set; }
        public decimal fee { get; set; }
        public string chiefComplaint { get; set; }
        public string rejectReason { get; set; }
        public int status { get; set; }
        public string transcriptFilePath { get; set; }
        public DateTime date { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public bool hasReferral { get; set; }
        public int referralID { get; set; }
    }
}