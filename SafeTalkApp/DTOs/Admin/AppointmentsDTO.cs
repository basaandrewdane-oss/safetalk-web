using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Admin
{
    public class AppointmentsDTO
    {
        public int appointmentID { get; set; }
        public DateTime date { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public int status { get; set; }
        public string patientName { get; set; }
        public string doctorName { get; set; }
        public string chiefComplaint { get; set; }
        public string rejectReason { get; set; }
    }
}