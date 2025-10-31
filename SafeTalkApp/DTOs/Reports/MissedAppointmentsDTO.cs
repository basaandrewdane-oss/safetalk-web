using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Reports
{
    public class MissedAppointmentsDTO
    {
        public string patientName { get; set; }
        public string doctorName { get; set; }
        public DateTime date { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public int status { get; set; }
    }
}