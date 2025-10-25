using SafeTalkApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Consultation
{
    public class ReferralDTO
    {
        public int appointmentID { get; set; }
        public int doctorID { get; set; }
        public int patientID { get; set; }
        public string reason { get; set; }
        public string notes { get; set; }
        public UrgencyLevel urgencyLevel { get; set; }
        public int status { get; set; }
        public DateTime dateCreated { get; set; }
        public string sentTo { get; set; }
    }
}