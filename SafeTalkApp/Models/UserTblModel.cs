using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SafeTalkApp.Models
{
    public class UserTblModel
    {
        public int userID { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        [Column(TypeName = "date")]
        public DateTime birthDate { get; set; }
        public int genderID { get; set; }
        public string phoneNumber { get; set; }
        public string licenseNumber { get; set; }
        public string specialization { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string profilePictureUrl { get; set; }
        public bool isVerified { get; set; }
        public string emailVerificationToken { get; set; }
        public DateTime? emailVerificationExpiry { get; set; }
        public bool isEmailVerified { get; set; }
        public int failedLoginAttempts { get; set; }
        public DateTime? lastFailedLoginAttempt { get; set; }
        public bool isLocked { get; set; }
        public DateTime? lockoutEnd { get; set; }
        public DateTime dateCreated { get; set; }
        public DateTime dateUpdated { get; set; }
    }
}