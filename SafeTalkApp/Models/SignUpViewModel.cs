using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class SignUpViewModel
    {
        public int userID { get; set; }
        public int roleID { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public DateTime birthDate { get; set; }
        public int genderID { get; set; }
        public string phoneNumber { get; set; }
        public string licenseNumber { get; set; }
        public string specialization { get; set; }
        public List<UserAvailabilityTblModel> availability { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}