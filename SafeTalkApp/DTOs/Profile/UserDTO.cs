using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Profile
{
    public class UserDTO
    {
        public int userID { get; set; }
        public string email { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public int roleID { get; set; }
        public string role { get; set; }
        public string specialization { get; set; }
        public string contactNumber { get; set; }
        public string profilePictureUrl { get; set; }
    }
}