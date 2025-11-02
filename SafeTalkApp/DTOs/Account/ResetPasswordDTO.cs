using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SafeTalkApp.DTOs.Account
{
    public class ResetPasswordDTO
    {
        public string token { get; set; }
        public string newPassword { get; set; }
    }
}