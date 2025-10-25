using System;
using System.Collections.Generic;
using System.EnterpriseServices.Internal;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class AppointmentsTblModel
    {
        public int appointmentID { get; set; }
        public int doctorID { get; set; }
        public int patientID { get; set; }
        public DateTime date { get; set; }
        public TimeSpan startTime { get; set; }
        public TimeSpan endTime { get; set; }
        public decimal fee { get; set; }
        public string chiefComplaint { get; set; }
        public string rejectReason { get; set; }
        public int status { get; set; }
        public string transcriptFilePath { get; set; }
        public string audioFileHash { get; set; }
        public string transcriptHash { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
    }
}