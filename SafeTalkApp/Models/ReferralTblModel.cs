using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class ReferralTblModel
    {
        public int referralID { get; set; }
        public int appointmentID { get; set; }
        public int doctorID { get; set; }
        public int patientID { get; set; }
        public string reason { get; set; }
        public string notes { get; set; }
        public int urgencyLevel { get; set; }
        public int status { get; set; }
        public DateTime dateCreated { get; set; }
        public string sentTo { get; set; }
    }
}