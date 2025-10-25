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
    }
}