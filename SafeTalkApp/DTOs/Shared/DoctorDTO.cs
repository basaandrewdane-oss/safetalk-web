using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Shared
{
    public class DoctorDTO
    {
        public int userID { get; set; }
        public string fullName { get; set; }
        public string email { get; set; }
        public DateTime birthDate { get; set; }
        public string phoneNumber { get; set; }
        public string licenseNumber { get; set; }
        public string specialization { get; set; }
    }
}