using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Account
{
    public class SignUpDTO
    {
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public string birthDate { get; set; }
        public int genderID { get; set; }
        public string phoneNumber { get; set; }
        public string licenseNumber { get; set; }
        public string specialization { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public int roleID { get; set; }
        public int? slotDuration { get; set; } // in minutes, nullable for non-doctors
        public List<AvailabilityDTO> availability { get; set; }
    }
}