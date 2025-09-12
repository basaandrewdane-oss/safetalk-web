using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Account
{
    public class LoginDTO
    {
        public string email { get; set; }
        public string password { get; set; }
        public string role { get; set; }
        public bool verified { get; set; }
        public int userID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public bool success { get; set; }
        public string message { get; set; }

    }
}