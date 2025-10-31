using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Admin
{
    public class UsersDTO
    {
        public int userID { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public bool isVerified { get; set; }
        public DateTime dateCreated { get; set; }
    }
}